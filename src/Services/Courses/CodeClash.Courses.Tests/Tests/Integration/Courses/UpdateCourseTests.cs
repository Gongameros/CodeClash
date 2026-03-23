using CodeClash.Courses.Features.Courses;
using CodeClash.Courses.Features.Courses.UpdateCourse;
using CodeClash.Courses.Tests.Infrastructure;
using CodeClash.Courses.Tests.TestData;
using CodeClash.MongoDB.Extensions;
using MongoDB.Driver;
using Shouldly;

namespace CodeClash.Courses.Tests.Tests.Integration.Courses;

[Collection(IntegrationCollection.Name)]
public sealed class UpdateCourseTests(IntegrationFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_ValidUpdate_UpdatesCourse()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;

        var result = await fixture.Mediator.Send(new UpdateCourseCommand(
            courseId, "author-1",
            Title: "Updated Title",
            Description: "Updated description.",
            CodingTechnologies: null,
            Difficulty: null,
            Tags: null,
            ThumbnailUrl: null,
            IsPublished: null));

        result.IsSuccess.ShouldBeTrue();

        var updated = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        updated.ShouldNotBeNull();
        updated.Title.ShouldBe("Updated Title");
        updated.Description.ShouldBe("Updated description.");
    }

    [Fact]
    public async Task Handle_PublishCourse_SetsIsPublishedTrue()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;

        var result = await fixture.Mediator.Send(new UpdateCourseCommand(
            courseId, "author-1", null, null, null, null, null, null, IsPublished: true));

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        course!.IsPublished.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WrongAuthor_ReturnsNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(authorId: "author-1"))).Value.Id;

        var result = await fixture.Mediator.Send(new UpdateCourseCommand(
            courseId, "wrong-author",
            Title: "New Title",
            null, null, null, null, null, null));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.NotFound(courseId).Code);
    }

    [Fact]
    public async Task Handle_NonExistentCourse_ReturnsNotFound()
    {
        var fakeId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(new UpdateCourseCommand(
            fakeId, "author-1",
            Title: "Title",
            null, null, null, null, null, null));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.NotFound(fakeId).Code);
    }

    [Fact]
    public async Task Handle_NoFieldsProvided_SucceedsWithoutChanges()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(title: "Original"))).Value.Id;

        var result = await fixture.Mediator.Send(new UpdateCourseCommand(
            courseId, "author-1", null, null, null, null, null, null, null));

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        course!.Title.ShouldBe("Original");
    }
}
