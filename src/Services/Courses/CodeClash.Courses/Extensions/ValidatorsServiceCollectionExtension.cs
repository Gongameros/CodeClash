using FluentValidation;
using ServiceScan.SourceGenerator;

namespace CodeClash.Courses.Extensions;

public static partial class ValidatorsServiceCollectionExtension
{
    [GenerateServiceRegistrations(AssignableTo = typeof(IValidator<>), Lifetime = ServiceLifetime.Singleton)]
    public static partial IServiceCollection AddValidators(this IServiceCollection services);
}
