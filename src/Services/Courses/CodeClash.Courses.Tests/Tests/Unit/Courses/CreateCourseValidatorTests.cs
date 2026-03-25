using CodeClash.Courses.Features.Courses.CreateCourse;
using Shouldly;

namespace CodeClash.Courses.Tests.Unit.Courses;

public sealed class CreateCourseValidatorTests
{
    private readonly CreateCourseCommandValidator _validator = new();

    private static CreateCourseCommand ValidCommand() => new(
        "author-1",
        "Valid Title",
        "Valid description.",
        [CodingTechnology.CSharp],
        CourseDifficulty.Beginner,
        [],
        null);

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.ValidateAsync(ValidCommand());

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_EmptyTitle_FailsValidation()
    {
        var command = ValidCommand() with { Title = "" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Title));
    }

    [Fact]
    public async Task Validate_TitleTooLong_FailsValidation()
    {
        var command = ValidCommand() with { Title = new string('X', 201) };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Title));
    }

    [Fact]
    public async Task Validate_EmptyDescription_FailsValidation()
    {
        var command = ValidCommand() with { Description = "" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Description));
    }

    [Fact]
    public async Task Validate_DescriptionTooLong_FailsValidation()
    {
        var command = ValidCommand() with { Description = new string('X', 5001) };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_EmptyCodingTechnologies_FailsValidation()
    {
        var command = ValidCommand() with { CodingTechnologies = [] };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.CodingTechnologies));
    }

    [Fact]
    public async Task Validate_MultipleTechnologies_PassesValidation()
    {
        var command = ValidCommand() with
        {
            CodingTechnologies = [CodingTechnology.CSharp, CodingTechnology.TypeScript]
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }
}
