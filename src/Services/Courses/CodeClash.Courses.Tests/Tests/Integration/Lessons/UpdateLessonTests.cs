using CodeClash.Courses.Features.Courses;
using CodeClash.Courses.Features.Lessons.UpdateLesson;
using CodeClash.Courses.Tests.Infrastructure;
using CodeClash.Courses.Tests.TestData;
using CodeClash.MongoDB.Extensions;
using MongoDB.Driver;
using Shouldly;

namespace CodeClash.Courses.Tests.Tests.Integration.Lessons;

[Collection(IntegrationCollection.Name)]
public sealed class UpdateLessonTests(IntegrationFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_UpdateTitle_ChangesLessonTitle()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;
        var lessonId = (await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId))).Value.LessonId;

        var result = await fixture.Mediator.Send(new UpdateLessonCommand(
            courseId, moduleId, lessonId, "author-1",
            Title: "Updated Title",
            null, null, null, null));

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        var lesson = course!.Modules.First(m => m.ModuleId == moduleId)
                              .Lessons.First(l => l.LessonId == lessonId);
        lesson.Title.ShouldBe("Updated Title");
    }

    [Fact]
    public async Task Handle_UpdateContent_ChangesContent()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;
        var lessonId = (await fixture.Mediator.Send(
            LessonFactory.CreateCommand(courseId, moduleId, type: LessonType.Theory))).Value.LessonId;

        var result = await fixture.Mediator.Send(new UpdateLessonCommand(
            courseId, moduleId, lessonId, "author-1",
            null, null, null,
            Content: "## Updated Content",
            null));

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        var lesson = course!.Modules.First(m => m.ModuleId == moduleId)
                              .Lessons.First(l => l.LessonId == lessonId);
        lesson.Content.ShouldBe("## Updated Content");
    }

    [Fact]
    public async Task Handle_WrongAuthor_ReturnsNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(authorId: "author-1"))).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;
        var lessonId = (await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId))).Value.LessonId;

        var result = await fixture.Mediator.Send(new UpdateLessonCommand(
            courseId, moduleId, lessonId, "wrong-author",
            Title: "New Title", null, null, null, null));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.NotFound(courseId).Code);
    }

    [Fact]
    public async Task Handle_NonExistentLesson_ReturnsLessonNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;
        var fakeLessonId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(new UpdateLessonCommand(
            courseId, moduleId, fakeLessonId, "author-1",
            Title: "New Title", null, null, null, null));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.LessonNotFound(fakeLessonId).Code);
    }

    [Fact]
    public async Task Handle_NoFieldsProvided_SucceedsWithoutChanges()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;
        var lessonId = (await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId))).Value.LessonId;

        var result = await fixture.Mediator.Send(new UpdateLessonCommand(
            courseId, moduleId, lessonId, "author-1",
            null, null, null, null, null));

        result.IsSuccess.ShouldBeTrue();
    }
}
