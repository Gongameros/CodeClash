using FluentValidation;

namespace CodeClash.Courses.Features.Modules.AddModule;

public sealed class AddModuleCommandValidator : AbstractValidator<AddModuleCommand>
{
    public AddModuleCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
    }
}