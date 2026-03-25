using CodeClash.Courses.Features.Courses;
using CodeClash.Courses.Features.Modules.AddModule;
using CodeClash.Courses.Tests.Infrastructure;
using CodeClash.Courses.Tests.TestData;
using CodeClash.MongoDB.Extensions;
using MongoDB.Driver;
using Shouldly;

namespace CodeClash.Courses.Tests.Tests.Integration.Modules;

[Collection(IntegrationCollection.Name)]
public sealed class AddModuleTests(IntegrationFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_ValidCommand_AddsModuleToCourse()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;

        var result = await fixture.Mediator.Send(
            new AddModuleCommand(courseId, "author-1", "Module 1", "Intro module", 0, 100));

        result.IsSuccess.ShouldBeTrue();
        result.Value.ModuleId.ShouldNotBeNullOrEmpty();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        course!.Modules.Count.ShouldBe(1);
        course.Modules[0].Title.ShouldBe("Module 1");
        course.Modules[0].XpReward.ShouldBe(100);
    }

    [Fact]
    public async Task Handle_MultipleModules_AppendsAll()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand())).Value.Id;

        await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId, title: "Module A", xpReward: 50));
        await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId, title: "Module B", xpReward: 75));

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync();
        course!.Modules.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_WrongAuthor_ReturnsNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(authorId: "author-1"))).Value.Id;

        var result = await fixture.Mediator.Send(
            ModuleFactory.CreateCommand(courseId, authorId: "wrong-author"));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.NotFound(courseId).Code);
    }

    [Fact]
    public async Task Handle_NonExistentCourse_ReturnsNotFound()
    {
        var fakeId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(ModuleFactory.CreateCommand(fakeId));

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.NotFound(fakeId).Code);
    }
}
