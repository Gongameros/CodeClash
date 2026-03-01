using FluentValidation;

namespace CodeClash.Courses.Features.Lessons.UpdateLesson;

public sealed class UpdateLessonCommandValidator : AbstractValidator<UpdateLessonCommand>
{
    public UpdateLessonCommandValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200).When(x => x.Title is not null);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0).When(x => x.Order is not null);
    }
}