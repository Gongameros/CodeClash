using CodeClash.Utilities;
using CodeClash.Utilities.Options;
using Microsoft.Extensions.Options;

namespace CodeClash.AppHost.Extensions;

/// <summary>
/// Provides extension methods for registering and validating options
/// for <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class DistributedApplicationOptionsExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        /// Registers, binds, and validates options using the section name
        /// defined on the options type.
        /// </summary>
        public IDistributedApplicationBuilder AddValidatedOptions<TOptions>(bool validateDataAnnotations = true,
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
        public IDistributedApplicationBuilder AddValidatedOptions<TOptions, TValidator>(bool validateOnStart = true)
            where TOptions : class, IOptionsSection
            where TValidator : class, IValidateOptions<TOptions>
        {
            builder.Services.AddValidatedOptions<TOptions, TValidator>(
                builder.Configuration,
                validateOnStart);

            return builder;
        }
    }
}
