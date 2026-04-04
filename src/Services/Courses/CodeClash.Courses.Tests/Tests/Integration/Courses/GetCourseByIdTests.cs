using CodeClash.Courses.Features.Courses;
using CodeClash.Courses.Features.Courses.GetCourseById;
using CodeClash.Courses.Tests.Infrastructure;
using CodeClash.Courses.Tests.TestData;
using CodeClash.MongoDB.Extensions;
using Shouldly;

namespace CodeClash.Courses.Tests.Tests.Integration.Courses;

[Collection(IntegrationCollection.Name)]
public sealed class GetCourseByIdTests(IntegrationFixture fixture) : IAsyncLifetime
{
    public ValueTask InitializeAsync() => new(fixture.ResetAsync());
    public ValueTask DisposeAsync() => default;

    [Fact]
    public async Task Handle_ExistingCourse_ReturnsCourseDetail()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(title: "Test Course"), Ct.Token)).Value.Id;

        var result = await fixture.Mediator.Send(new GetCourseByIdQuery(courseId), Ct.Token);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(courseId);
        result.Value.Title.ShouldBe("Test Course");
        result.Value.AuthorId.ShouldBe("author-1");
    }

    [Fact]
    public async Task Handle_NonExistentCourse_ReturnsNotFound()
    {
        var fakeId = MongoHelper.RandomId();

        var result = await fixture.Mediator.Send(new GetCourseByIdQuery(fakeId), Ct.Token);

        result.IsFailure.ShouldBeTrue();
        result.FirstError.Code.ShouldBe(CourseErrors.NotFound(fakeId).Code);
    }

    [Fact]
    public async Task Handle_CourseWithModulesAndLessons_ReturnsFullHierarchy()
    {
        var courseId = (await fixture.Mediator.Send(CourseFactory.CreateCommand(), Ct.Token)).Value.Id;
        var moduleId = (await fixture.Mediator.Send(ModuleFactory.CreateCommand(courseId), Ct.Token)).Value.ModuleId;
        await fixture.Mediator.Send(LessonFactory.CreateCommand(courseId, moduleId), Ct.Token);

        var result = await fixture.Mediator.Send(new GetCourseByIdQuery(courseId), Ct.Token);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Modules.Count.ShouldBe(1);
        result.Value.Modules[0].Lessons.Count.ShouldBe(1);
    }
}
