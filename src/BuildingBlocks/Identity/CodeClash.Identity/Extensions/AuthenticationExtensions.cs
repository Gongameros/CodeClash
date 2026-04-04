using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

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

        AuthenticationBuilder authBuilder;

        if (!string.IsNullOrEmpty(keycloakOptions.BaseUrl))
        {
            // Direct URL mode — bypass Aspire service discovery (used in Azure)
            var authority = $"{keycloakOptions.BaseUrl.TrimEnd('/')}/realms/{keycloakOptions.Realm}";
            authBuilder = builder.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.Audience = keycloakOptions.Audience;
                    options.RequireHttpsMetadata = false;
                    configureOptions?.Invoke(options);
                });
        }
        else
        {
            // Service discovery mode (local development via Aspire)
            authBuilder = builder.Services.AddAuthentication()
                .AddKeycloakJwtBearer(
                    serviceName: serviceName,
                    realm: keycloakOptions.Realm,
                    options =>
                    {
                        options.Audience = keycloakOptions.Audience;
                        options.RequireHttpsMetadata = false;
                        if (builder.Environment.IsDevelopment())
                        {
                            options.TokenValidationParameters.ValidateIssuer = false;
                        }

                        configureOptions?.Invoke(options);
                    });
        }

        builder.Services.AddAuthorization();
        return authBuilder;
    }

    public static AuthenticationBuilder AddKeycloakWebAuthentication(
        this IHostApplicationBuilder builder,
        string serviceName,
        Action<CookieAuthenticationOptions>? configureCookie = null,
        Action<OpenIdConnectOptions>? configureOidc = null)
    {
        var keycloakOptions = new KeycloakOptions();
        builder.Configuration.GetSection(KeycloakOptions.SectionName).Bind(keycloakOptions);

        if (string.IsNullOrEmpty(keycloakOptions.Realm))
            throw new InvalidOperationException("Keycloak:Realm configuration is required.");

        AuthenticationBuilder authBuilder;

        if (!string.IsNullOrEmpty(keycloakOptions.BaseUrl))
        {
            // Direct URL mode — bypass Aspire service discovery (used in Azure)
            var authority = $"{keycloakOptions.BaseUrl.TrimEnd('/')}/realms/{keycloakOptions.Realm}";
            authBuilder = builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    configureCookie?.Invoke(options);
                })
                .AddOpenIdConnect(options =>
                {
                    options.Authority = authority;
                    options.ClientId = keycloakOptions.ClientId;
                    options.ClientSecret = keycloakOptions.ClientSecret;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.RequireHttpsMetadata = false;
                    options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("offline_access");

                    options.MapInboundClaims = false;
                    options.TokenValidationParameters.NameClaimType = "preferred_username";
                    options.TokenValidationParameters.RoleClaimType = "roles";

                    configureOidc?.Invoke(options);
                });
        }
        else
        {
            // Service discovery mode (local development via Aspire)
            authBuilder = builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    configureCookie?.Invoke(options);
                })
                .AddKeycloakOpenIdConnect(
                    serviceName: serviceName,
                    realm: keycloakOptions.Realm,
                    options =>
                    {
                        options.ClientId = keycloakOptions.ClientId;
                        options.ClientSecret = keycloakOptions.ClientSecret;
                        options.ResponseType = OpenIdConnectResponseType.Code;
                        options.SaveTokens = true;
                        options.GetClaimsFromUserInfoEndpoint = true;
                        options.RequireHttpsMetadata = false;

                        options.Scope.Clear();
                        options.Scope.Add("openid");
                        options.Scope.Add("profile");
                        options.Scope.Add("email");
                        options.Scope.Add("offline_access");

                        options.MapInboundClaims = false;
                        options.TokenValidationParameters.NameClaimType = "preferred_username";
                        options.TokenValidationParameters.RoleClaimType = "roles";

                        configureOidc?.Invoke(options);
                    });
        }

        builder.Services.AddAuthorization();
        return authBuilder;
    }
}
