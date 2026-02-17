using CodeClash.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace CodeClash.Courses.Extensions;

// TODO: Refactoring
public static class OpenApiKeycloakExtensions
{
    public static OpenApiOptions AddKeycloakSecurityScheme(this OpenApiOptions options,
        IConfiguration configuration, string keycloakResourceName)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            var keycloakBase = configuration[$"services:{keycloakResourceName}:https:0"]
                               ?? configuration[$"services:{keycloakResourceName}:http:0"]
                               ?? throw new InvalidOperationException("Cannot resolve Keycloak URL.");

            var realm = configuration[$"{KeycloakOptions.SectionName}:Realm"]
                        ?? throw new InvalidOperationException("Keycloak realm not found.");

            var realmUrl = $"{keycloakBase.TrimEnd('/')}/realms/{realm}/protocol/openid-connect";

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

            document.Components.SecuritySchemes.Add("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{realmUrl}/auth"),
                        TokenUrl = new Uri($"{realmUrl}/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            ["openid"] = "OpenID Connect identity",
                            ["profile"] = "User profile information",
                            ["email"] = "User email address"
                        }
                    }
                }
            });

            document.Security =
            [
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("oauth2"),
                        ["openid", "profile", "email"]
                    }
                }
            ];

            document.SetReferenceHostDocument();
            return Task.CompletedTask;
        });

        return options;
    }
}
