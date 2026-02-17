using System.Security.Claims;
using CodeClash.Courses.Extensions;
using CodeClash.Identity.Extensions;
using CodeClash.ServiceDefaults;
using CodeClash.Shared.Constants;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Keycloak authentication
builder.AddKeycloakAuthentication(Resources.Keycloak);
builder.Services.AddHttpContextAccessor();

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


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .RequireAuthorization();

app.MapGet("users/me", (ClaimsPrincipal claimsPrincipal) =>
    {
        return claimsPrincipal.Claims
            .GroupBy(c => c.Type)
            .ToDictionary(g => g.Key, g => g.Count() == 1
                ? g.First().Value
                : string.Join(", ", g.Select(c => c.Value)));
    })
    .RequireAuthorization();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
