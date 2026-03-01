using FluentValidation;

namespace CodeClash.Courses.Features.Lessons.AddLesson;

public sealed class AddLessonCommandValidator : AbstractValidator<AddLessonCommand>
{
    public AddLessonCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
    }
}