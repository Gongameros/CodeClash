using CodeClash.Courses.Features.Courses;
using CodeClash.Courses.Features.Lessons.DeleteLesson;
using CodeClash.Courses.Tests.Infrastructure;
using CodeClash.Courses.Tests.TestData;
using CodeClash.MongoDB.Extensions;
using MongoDB.Driver;
using Shouldly;

namespace CodeClash.Courses.Tests.Tests.Integration.Lessons;

[Collection(IntegrationCollection.Name)]
public sealed class DeleteLessonTests(IntegrationFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_ExistingLesson_RemovesLesson()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;
        var lessonId = (await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId))).Value.LessonId;

        var result = await fixture.Mediator.Send(new DeleteLessonCommand(courseId, moduleId, lessonId, "author-1"));

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        course!.Modules.First(m => m.ModuleId == moduleId).Lessons.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WrongAuthor_ReturnsModuleNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(authorId: "author-1"))).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;
        var lessonId = (await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId))).Value.LessonId;

        var result = await fixture.Mediator.Send(
            new DeleteLessonCommand(courseId, moduleId, lessonId, "wrong-author"));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.ModuleNotFound(moduleId).Code);

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        course!.Modules.First(m => m.ModuleId == moduleId).Lessons.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_NonExistentModule_ReturnsModuleNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;
        var fakeModuleId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(
            new DeleteLessonCommand(courseId, fakeModuleId, MongoHelper.RandomId(), "author-1"));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.ModuleNotFound(fakeModuleId).Code);
    }

    [Fact]
    public async Task Handle_NonExistentLesson_ReturnsLessonNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;
        var fakeLessonId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(
            new DeleteLessonCommand(courseId, moduleId, fakeLessonId, "author-1"));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.LessonNotFound(fakeLessonId).Code);
    }

    [Fact]
    public async Task Handle_DeleteOneLesson_PreservesOthers()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;
        var lessonId1 = (await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId))).Value.LessonId;
        var lessonId2 = (await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId))).Value.LessonId;

        var result = await fixture.Mediator.Send(
            new DeleteLessonCommand(courseId, moduleId, lessonId1, "author-1"));

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        var lessons = course!.Modules.First(m => m.ModuleId == moduleId).Lessons;
        lessons.Count.ShouldBe(1);
        lessons[0].LessonId.ShouldBe(lessonId2);
    }
}
