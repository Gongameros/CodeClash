using CodeClash.Courses.Features.Courses;
using CodeClash.Courses.Features.Lessons.AddLesson;
using CodeClash.Courses.Tests.Infrastructure;
using CodeClash.Courses.Tests.TestData;
using CodeClash.MongoDB.Extensions;
using MongoDB.Driver;
using Shouldly;

namespace CodeClash.Courses.Tests.Tests.Integration.Lessons;

[Collection(IntegrationCollection.Name)]
public sealed class AddLessonTests(IntegrationFixture fixture) : IAsyncLifetime
{
    public ValueTask InitializeAsync() => new(fixture.ResetAsync());
    public ValueTask DisposeAsync() => default;

    [Fact]
    public async Task Handle_TheoryLesson_AddsLessonWithContent()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;

        var result = await fixture.Mediator.Send(new AddLessonCommand(
            courseId, moduleId, "author-1",
            "Intro to C#", LessonType.Theory, 0,
            Content: "# Hello World",
            Challenge: null), Ct.Token);

        result.IsSuccess.ShouldBeTrue();
        result.Value.LessonId.ShouldNotBeNullOrEmpty();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync(Ct.Token);
        var module = course!.Modules.First(m => m.ModuleId == moduleId);
        module.Lessons.Count.ShouldBe(1);
        module.Lessons[0].Title.ShouldBe("Intro to C#");
        module.Lessons[0].Type.ShouldBe(LessonType.Theory);
        module.Lessons[0].Content.ShouldBe("# Hello World");
    }

    [Fact]
    public async Task Handle_CodingChallengeLesson_AddsLessonWithoutContent()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;

        var result = await fixture.Mediator.Send(new AddLessonCommand(
            courseId, moduleId, "author-1",
            "FizzBuzz", LessonType.CodingChallenge, 0,
            Content: null,
            Challenge: null), Ct.Token);

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync(Ct.Token);
        var lesson = course!.Modules.First(m => m.ModuleId == moduleId).Lessons[0];
        lesson.Type.ShouldBe(LessonType.CodingChallenge);
        lesson.Content.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WrongAuthor_ReturnsModuleNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(authorId: "author-1"), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;

        var result = await fixture.Mediator.Send(new AddLessonCommand(
            courseId, moduleId, "wrong-author",
            "Lesson", LessonType.Theory, 0, null, null), Ct.Token);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.ModuleNotFound(moduleId).Code);
    }

    [Fact]
    public async Task Handle_NonExistentModule_ReturnsModuleNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var fakeModuleId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(new AddLessonCommand(
            courseId, fakeModuleId, "author-1",
            "Lesson", LessonType.Theory, 0, null, null), Ct.Token);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.ModuleNotFound(fakeModuleId).Code);
    }

    [Fact]
    public async Task Handle_MultipleLessons_AppendsAll()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;

        await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId, title: "Lesson 1", order: 0), Ct.Token);
        await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId, title: "Lesson 2", type: LessonType.Quiz, order: 1), Ct.Token);

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync(Ct.Token);
        course!.Modules.First(m => m.ModuleId == moduleId).Lessons.Count.ShouldBe(2);
    }
}
