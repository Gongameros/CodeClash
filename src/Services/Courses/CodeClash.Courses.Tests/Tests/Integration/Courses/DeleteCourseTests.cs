using CodeClash.Courses.Features.Courses;
using CodeClash.Courses.Features.Courses.DeleteCourse;
using CodeClash.Courses.Tests.Infrastructure;
using CodeClash.Courses.Tests.TestData;
using CodeClash.MongoDB.Extensions;
using MongoDB.Driver;
using Shouldly;

namespace CodeClash.Courses.Tests.Tests.Integration.Courses;

[Collection(IntegrationCollection.Name)]
public sealed class DeleteCourseTests(IntegrationFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_ExistingCourse_DeletesCourse()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;

        var result = await fixture.Mediator.Send(new DeleteCourseCommand(courseId, "author-1"));

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        course.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_WrongAuthor_ReturnsNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(authorId: "author-1"))).Value.Id;

        var result = await fixture.Mediator.Send(new DeleteCourseCommand(courseId, "wrong-author"));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.NotFound(courseId).Code);

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        course.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_NonExistentCourse_ReturnsNotFound()
    {
        var fakeId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(new DeleteCourseCommand(fakeId, "author-1"));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.NotFound(fakeId).Code);
    }

    [Fact]
    public async Task Handle_Delete_RemovesAllModulesAndLessons()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;
        await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId));

        var result = await fixture.Mediator.Send(new DeleteCourseCommand(courseId, "author-1"));

        result.IsSuccess.ShouldBeTrue();

        var count = await fixture.Courses.CountDocumentsAsync(FilterDefinition<Course>.Empty);
        count.ShouldBe(0);
    }
}
