// using CodeClash.ServiceDefaults;
// using CodeClash.Shared.Constants;
// using Microsoft.Extensions.Options;
// using Swashbuckle.AspNetCore.SwaggerGen;
//
var builder = WebApplication.CreateBuilder(args);
// builder.Configuration.AddJsonFile("appsettings.CodersGateway.json", false, true);
// builder.Configuration.AddJsonFile("appsettings.CoursesGateway.json", false, true);
//
// if (builder.Environment.IsEnvironment("local"))
// {
//     builder.Configuration.AddJsonFile("appsettings.local.json");
// }
// else
// {
//     builder.Configuration.AddAzureKeyVaultSecrets(Resources.KeyVault);
// }
//
// builder.AddServiceDefaults();
//
// builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
//
// var configuration = builder.Configuration.GetSection("ReverseProxy");
// builder.Services.AddReverseProxy()
//     .LoadFromConfig(configuration)
//     .AddTransforms(context =>
//     {
//         context.RequestTransforms.Add(
//             new ProxySignatureTransformer(
//                 builder.Configuration.GetValue<string>("ProxySignatureSecret")!));
//
//         context.RequestTransforms.Add(
//             new AuthHeaderTransformer());
//     })
//     .AddSwagger(configuration)
//     .AddServiceDiscoveryDestinationResolver();
//
// builder.Services.AddCors(options =>
// {
//     options.AddDefaultPolicy(
//         policy => policy.AllowAnyOrigin()
//             .AllowAnyHeader()
//             .AllowAnyMethod()
//             .SetIsOriginAllowed(_ => true));
// });
//
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
//
// var app = builder.Build();
//
// app.UseSwagger();
// app.UseSwaggerUI(options =>
// {
//     options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
//     options.RoutePrefix = string.Empty;
//     var config = app.Services.GetRequiredService<IOptionsMonitor<ReverseProxyDocumentFilterConfig>>().CurrentValue;
//     options.ConfigureSwaggerEndpoints(config);
// });
//
// app.UseCors();
// app.UseAuthentication();
// app.UseAuthorization();
// app.MapReverseProxy();
// await app.RunAsync();
