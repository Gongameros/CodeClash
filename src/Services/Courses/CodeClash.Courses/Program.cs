using CodeClash.Courses.Extensions;
using CodeClash.Identity.Extensions;
using CodeClash.ServiceDefaults;
using CodeClash.Shared.Constants;
using CodeClash.Utilities.Endpoints;
using Scalar.AspNetCore;
using HttpEndpointServiceCollectionExtensions = CodeClash.Courses.Extensions.HttpEndpointServiceCollectionExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Keycloak authentication
builder.AddKeycloakAuthentication(Resources.Keycloak);
builder.Services.AddHttpContextAccessor();

builder.Services.AddDependencyInjection();

// Add services to the container.
builder.Services.AddOpenApi(options =>
{
    options.AddKeycloakSecurityScheme(builder.Configuration, Resources.Keycloak);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// TODO: Refactoring
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference((options, httpContext)  =>
    {
        options.Title = "CodeClash.Courses API documentation";
        options.Theme = ScalarTheme.DeepSpace;
        var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        options.AddPreferredSecuritySchemes("oauth2")
            .AddAuthorizationCodeFlow("oauth2", flow =>
            {
                flow.ClientId = "codeclash-scalar";
                flow.Pkce = Pkce.Sha256;
                flow.SelectedScopes = ["openid", "profile", "email"];
                flow.RedirectUri = $"{baseUrl}/scalar/v1";
            });
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
HttpEndpointServiceCollectionExtensions.MapEndpoints(app);

app.Run();
