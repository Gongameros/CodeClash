using CodeClash.Courses.Features.Courses.CreateCourse;
using CodeClash.Courses.Tests.Infrastructure;
using CodeClash.Courses.Tests.TestData;
using MongoDB.Driver;
using Shouldly;

namespace CodeClash.Courses.Tests.Tests.Integration.Courses;

[Collection(IntegrationCollection.Name)]
public sealed class CreateCourseTests(IntegrationFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCourseId()
    {
        var result = await fixture.Mediator.Send(CourseFactory.CreateCommand(title: "Learn C#"));

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ValidCommand_PersistsCourseToDatabase()
    {
        var command = CourseFactory.CreateCommand(
            title: "Learn C#",
            description: "A comprehensive C# course.",
            difficulty: CourseDifficulty.Intermediate,
            technologies: [CodingTechnology.CSharp],
            tags: ["csharp"],
            thumbnailUrl: "https://example.com/thumb.png");

        var result = await fixture.Mediator.Send(command);

        var course = await fixture.Courses.Find(c => c.Id == result.Value.Id).FirstOrDefaultAsync();
        course.ShouldNotBeNull();
        course.AuthorId.ShouldBe("author-1");
        course.Title.ShouldBe("Learn C#");
        course.Difficulty.ShouldBe(CourseDifficulty.Intermediate);
        course.Tags.ShouldContain("csharp");
        course.ThumbnailUrl.ShouldBe("https://example.com/thumb.png");
        course.IsPublished.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_EmptyTitle_ReturnsValidationFailure()
    {
        var result = await fixture.Mediator.Send(
            new CreateCourseCommand("author-1", "", "A description.", [CodingTechnology.CSharp],
                CourseDifficulty.Beginner, [], null));

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Handle_EmptyDescription_ReturnsValidationFailure()
    {
        var result = await fixture.Mediator.Send(
            new CreateCourseCommand("author-1", "Valid Title", "", [CodingTechnology.CSharp],
                CourseDifficulty.Beginner, [], null));

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Handle_EmptyCodingTechnologies_ReturnsValidationFailure()
    {
        var result = await fixture.Mediator.Send(
            new CreateCourseCommand("author-1", "Valid Title", "Valid description.", [],
                CourseDifficulty.Beginner, [], null));

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Handle_TitleExceedsMaxLength_ReturnsValidationFailure()
    {
        var result = await fixture.Mediator.Send(
            CourseFactory.CreateCommand(title: new string('A', 201)));

        result.IsFailure.ShouldBeTrue();
    }
}
