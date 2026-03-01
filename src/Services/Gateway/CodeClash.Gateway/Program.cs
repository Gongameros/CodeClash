using CodeClash.Identity.Extensions;
using CodeClash.ServiceDefaults;
using CodeClash.Shared.Constants;
using Scalar.AspNetCore;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.CoursesGateway.json", optional: false, reloadOnChange: true);

builder.AddServiceDefaults();
builder.AddKeycloakAuthentication(Resources.Keycloak);

var internalApiKey = builder.Configuration["InternalApiKey"]!;

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    // .AddServiceDiscoveryDestinationResolver()
    .AddTransforms(ctx =>
    {
        ctx.AddRequestTransform(transformContext =>
        {
            transformContext.ProxyRequest.Headers.Remove("X-Internal-Api-Key");
            transformContext.ProxyRequest.Headers.Add("X-Internal-Api-Key", internalApiKey);
            return ValueTask.CompletedTask;
        });
    });

builder.Services.AddHttpClient("courses", client =>
{
    client.BaseAddress = new Uri($"https+http://{Resources.CoursesService}");
    client.DefaultRequestHeaders.Add("X-Internal-Api-Key", internalApiKey);
}).AddServiceDiscovery();

builder.Services.AddHttpClient("coders", client =>
{
    client.BaseAddress = new Uri($"https+http://{Resources.CodersService}");
    client.DefaultRequestHeaders.Add("X-Internal-Api-Key", internalApiKey);
}).AddServiceDiscovery();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true));
});

var app = builder.Build();

app.MapGet("/openapi/courses/v1.json", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("courses");
    var json = await client.GetStringAsync("/openapi/v1.json");
    return Results.Content(json, "application/json");
});

app.MapGet("/openapi/coders/v1.json", async (IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("coders");
    var json = await client.GetStringAsync("/openapi/v1.json");
    return Results.Content(json, "application/json");
});

app.MapScalarApiReference("/scalar/courses", options =>
{
    options.Title = "CodeClash.Courses API";
    options.Theme = ScalarTheme.DeepSpace;
    options.OpenApiRoutePattern = "/openapi/courses/v1.json";
});

app.MapScalarApiReference("/scalar/coders", options =>
{
    options.Title = "CodeClash.Coders API";
    options.Theme = ScalarTheme.DeepSpace;
    options.OpenApiRoutePattern = "/openapi/coders/v1.json";
});

app.UseCors();
app.MapDefaultEndpoints();
app.MapReverseProxy();

app.Run();
