using CodeClash.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CodeClash.Identity.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var keycloakOptions = new KeycloakOptions();
        configuration.GetSection(KeycloakOptions.SectionName).Bind(keycloakOptions);

        services.Configure<KeycloakOptions>(
            configuration.GetSection(KeycloakOptions.SectionName));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakOptions.Authority;
                options.Audience = keycloakOptions.Audience;
                options.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
                options.MetadataAddress = keycloakOptions.GetMetadataAddress();

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = keycloakOptions.ValidateAudience,
                    ValidAudience = keycloakOptions.Audience,
                    ValidateIssuer = keycloakOptions.ValidateIssuer,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully");
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        // Register claim population service
        services.AddScoped<IClaimPopulationService, ClaimPopulationService>();

        return services;
    }
}
