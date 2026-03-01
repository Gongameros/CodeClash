using CodeClash.Coders.Extensions;
using CodeClash.Identity.Extensions;
using CodeClash.ServiceDefaults;
using CodeClash.Shared.Constants;
using CodeClash.Utilities.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Keycloak authentication
builder.AddKeycloakAuthentication(Resources.Keycloak);
builder.Services.AddHttpContextAccessor();

builder.AddMongoDb();
builder.AddDependencyInjection();

// Add services to the container.
builder.Services.AddOpenApi(options =>
{
    options.AddKeycloakSecurityScheme(builder.Configuration, Resources.Keycloak);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "CodeClash.Coders API documentation";
        options.Theme = ScalarTheme.DeepSpace;
    });
}

app.UseInternalApiKey();
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapEndpoints();

app.Run();
