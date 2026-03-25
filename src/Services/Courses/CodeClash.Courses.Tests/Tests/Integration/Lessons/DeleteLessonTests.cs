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
    public ValueTask InitializeAsync() => new(fixture.ResetAsync());
    public ValueTask DisposeAsync() => default;

    [Fact]
    public async Task Handle_ExistingLesson_RemovesLesson()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;
        var lessonId = (await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId), Ct.Token)).Value.LessonId;

        var result = await fixture.Mediator.Send(new DeleteLessonCommand(courseId, moduleId, lessonId, "author-1"), Ct.Token);

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync(Ct.Token);
        course!.Modules.First(m => m.ModuleId == moduleId).Lessons.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WrongAuthor_ReturnsModuleNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(authorId: "author-1"), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;
        var lessonId = (await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId), Ct.Token)).Value.LessonId;

        var result = await fixture.Mediator.Send(
            new DeleteLessonCommand(courseId, moduleId, lessonId, "wrong-author"), Ct.Token);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.ModuleNotFound(moduleId).Code);

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync(Ct.Token);
        course!.Modules.First(m => m.ModuleId == moduleId).Lessons.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_NonExistentModule_ReturnsModuleNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var fakeModuleId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(
            new DeleteLessonCommand(courseId, fakeModuleId, MongoHelper.RandomId(), "author-1"), Ct.Token);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.ModuleNotFound(fakeModuleId).Code);
    }

    [Fact]
    public async Task Handle_NonExistentLesson_ReturnsLessonNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;
        var fakeLessonId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(
            new DeleteLessonCommand(courseId, moduleId, fakeLessonId, "author-1"), Ct.Token);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.LessonNotFound(fakeLessonId).Code);
    }

    [Fact]
    public async Task Handle_DeleteOneLesson_PreservesOthers()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;
        var lessonId1 = (await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId), Ct.Token)).Value.LessonId;
        var lessonId2 = (await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId), Ct.Token)).Value.LessonId;

        var result = await fixture.Mediator.Send(
            new DeleteLessonCommand(courseId, moduleId, lessonId1, "author-1"), Ct.Token);

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync(Ct.Token);
        var lessons = course!.Modules.First(m => m.ModuleId == moduleId).Lessons;
        lessons.Count.ShouldBe(1);
        lessons[0].LessonId.ShouldBe(lessonId2);
    }
}
