using CodeClash.Utilities.Validation;
using FluentValidation;
using Mediator;
using ServiceScan.SourceGenerator;

namespace CodeClash.Courses.Extensions;

public static partial class ValidatorsExtension
{
    [GenerateServiceRegistrations(AssignableTo = typeof(IValidator<>), Lifetime = ServiceLifetime.Singleton)]
    public static partial IServiceCollection AddValidators(this IServiceCollection services);

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidators();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
