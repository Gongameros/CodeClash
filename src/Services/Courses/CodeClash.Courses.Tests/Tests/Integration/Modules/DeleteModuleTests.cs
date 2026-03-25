using CodeClash.Courses.Features.Courses;
using CodeClash.Courses.Features.Modules.DeleteModule;
using CodeClash.Courses.Tests.Infrastructure;
using CodeClash.Courses.Tests.TestData;
using CodeClash.MongoDB.Extensions;
using MongoDB.Driver;
using Shouldly;

namespace CodeClash.Courses.Tests.Integration.Modules;

[Collection(IntegrationCollection.Name)]
public sealed class DeleteModuleTests(IntegrationFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_ExistingModule_RemovesModule()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;

        var result = await fixture.Mediator.Send(new DeleteModuleCommand(courseId, moduleId, "author-1"));

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        course!.Modules.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WrongAuthor_ReturnsNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(authorId: "author-1"))).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;

        var result = await fixture.Mediator.Send(new DeleteModuleCommand(courseId, moduleId, "wrong-author"));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.NotFound(courseId).Code);

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        course!.Modules.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_NonExistentCourse_ReturnsNotFound()
    {
        var fakeId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(
            new DeleteModuleCommand(fakeId, MongoHelper.RandomId(), "author-1"));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.NotFound(fakeId).Code);
    }

    [Fact]
    public async Task Handle_NonExistentModule_ReturnsModuleNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;
        var fakeModuleId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(
            new DeleteModuleCommand(courseId, fakeModuleId, "author-1"));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.ModuleNotFound(fakeModuleId).Code);
    }

    [Fact]
    public async Task Handle_DeleteOneOfMultipleModules_RemovesOnlyTarget()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;
        var moduleId1 = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;
        var moduleId2 = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId))).Value.ModuleId;

        var result = await fixture.Mediator.Send(new DeleteModuleCommand(courseId, moduleId1, "author-1"));

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        course!.Modules.Count.ShouldBe(1);
        course.Modules[0].ModuleId.ShouldBe(moduleId2);
    }
}
