using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CodeClash.Identity.Extensions;

public static class AuthenticationExtensions
{
    public static AuthenticationBuilder AddKeycloakAuthentication(
        this IHostApplicationBuilder builder,
        string serviceName,
        Action<JwtBearerOptions>? configureOptions = null)
    {
        var keycloakOptions = new KeycloakOptions();
        builder.Configuration.GetSection(KeycloakOptions.SectionName).Bind(keycloakOptions);

        if (string.IsNullOrEmpty(keycloakOptions.Realm))
        {
            throw new InvalidOperationException("Keycloak:Realm configuration is required.");
        }

        if (string.IsNullOrEmpty(keycloakOptions.Audience))
        {
            throw new InvalidOperationException("Keycloak:Audience configuration is required.");
        }

        var authBuilder = builder.Services.AddAuthentication()
            .AddKeycloakJwtBearer(
                serviceName: serviceName,
                realm: keycloakOptions.Realm,
                options =>
                {
                    options.Audience = keycloakOptions.Audience;

                    if (builder.Environment.IsDevelopment())
                    {
                        options.RequireHttpsMetadata = false;
                    }

                    configureOptions?.Invoke(options);
                });

        builder.Services.AddAuthorization();
        return authBuilder;
    }
}
