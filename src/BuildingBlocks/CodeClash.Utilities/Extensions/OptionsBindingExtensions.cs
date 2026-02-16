using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CodeClash.Utilities.Extensions;

/// <summary>
/// Provides extension methods for registering and validating options
/// that implement <see cref="IOptionsSection"/>.
/// </summary>
public static class OptionsExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers, binds, and validates options using the section name
        /// defined on the options type.
        /// </summary>
        public IHostApplicationBuilder AddValidatedOptions<TOptions>(bool validateDataAnnotations = true,
            bool validateOnStart = true)
            where TOptions : class, IOptionsSection
        {
            builder.Services.AddValidatedOptions<TOptions>(
                builder.Configuration,
                validateDataAnnotations,
                validateOnStart);

            return builder;
        }

        /// <summary>
        /// Registers, binds, and validates options with a custom validator.
        /// </summary>
        public IHostApplicationBuilder AddValidatedOptions<TOptions, TValidator>(bool validateOnStart = true)
            where TOptions : class, IOptionsSection
            where TValidator : class, IValidateOptions<TOptions>
        {
            builder.Services.AddValidatedOptions<TOptions, TValidator>(
                builder.Configuration,
                validateOnStart);

            return builder;
        }
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers, binds, and validates options using configuration.
        /// </summary>
        public IServiceCollection AddValidatedOptions<TOptions>(IConfiguration configuration,
            bool validateDataAnnotations = true,
            bool validateOnStart = true)
            where TOptions : class, IOptionsSection
        {
            var optionsBuilder = services
                .AddOptions<TOptions>()
                .Bind(configuration.GetSection(TOptions.SectionName));

            if (validateDataAnnotations)
            {
                optionsBuilder.ValidateDataAnnotations();
            }

            if (validateOnStart)
            {
                optionsBuilder.ValidateOnStart();
            }

            return services;
        }

        /// <summary>
        /// Registers, binds, and validates options with a custom validator.
        /// </summary>
        public IServiceCollection AddValidatedOptions<TOptions, TValidator>(IConfiguration configuration,
            bool validateOnStart = true)
            where TOptions : class, IOptionsSection
            where TValidator : class, IValidateOptions<TOptions>
        {
            services.AddSingleton<IValidateOptions<TOptions>, TValidator>();

            return services.AddValidatedOptions<TOptions>(
                configuration,
                validateDataAnnotations: false,
                validateOnStart: validateOnStart);
        }
    }
}
