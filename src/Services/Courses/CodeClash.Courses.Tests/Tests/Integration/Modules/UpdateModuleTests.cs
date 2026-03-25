using CodeClash.Courses.Features.Courses;
using CodeClash.Courses.Features.Modules.UpdateModule;
using CodeClash.Courses.Tests.Infrastructure;
using CodeClash.Courses.Tests.TestData;
using CodeClash.MongoDB.Extensions;
using MongoDB.Driver;
using Shouldly;

namespace CodeClash.Courses.Tests.Tests.Integration.Modules;

[Collection(IntegrationCollection.Name)]
public sealed class UpdateModuleTests(IntegrationFixture fixture) : IAsyncLifetime
{
    public ValueTask InitializeAsync() => new(fixture.ResetAsync());
    public ValueTask DisposeAsync() => default;

    [Fact]
    public async Task Handle_ValidUpdate_UpdatesModuleTitle()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;

        var result = await fixture.Mediator.Send(new UpdateModuleCommand(
            courseId, moduleId, "author-1",
            Title: "Updated Module",
            Description: null, Order: null, XpReward: null), Ct.Token);

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync(Ct.Token);
        course!.Modules[0].Title.ShouldBe("Updated Module");
    }

    [Fact]
    public async Task Handle_UpdateXpReward_ChangesValue()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;

        var result = await fixture.Mediator.Send(new UpdateModuleCommand(
            courseId, moduleId, "author-1",
            null, null, null, XpReward: 500), Ct.Token);

        result.IsSuccess.ShouldBeTrue();

        var course = await fixture.Courses.Find(c => c.Id == courseId).FirstOrDefaultAsync(Ct.Token);
        course!.Modules[0].XpReward.ShouldBe(500);
    }

    [Fact]
    public async Task Handle_WrongAuthor_ReturnsModuleNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(authorId: "author-1"), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;

        var result = await fixture.Mediator.Send(new UpdateModuleCommand(
            courseId, moduleId, "wrong-author",
            Title: "New Title", null, null, null), Ct.Token);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.ModuleNotFound(moduleId).Code);
    }

    [Fact]
    public async Task Handle_NonExistentModule_ReturnsModuleNotFound()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var fakeModuleId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(new UpdateModuleCommand(
            courseId, fakeModuleId, "author-1",
            Title: "Title", null, null, null), Ct.Token);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.ModuleNotFound(fakeModuleId).Code);
    }

    [Fact]
    public async Task Handle_NoFieldsProvided_SucceedsWithoutChanges()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;

        var result = await fixture.Mediator.Send(new UpdateModuleCommand(
            courseId, moduleId, "author-1",
            null, null, null, null), Ct.Token);

        result.IsSuccess.ShouldBeTrue();
    }
}
