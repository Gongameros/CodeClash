using CodeClash.Courses.Features.Courses.GetCourses;
using CodeClash.Courses.Tests.Infrastructure;
using CodeClash.Courses.Tests.TestData;
using Shouldly;

namespace CodeClash.Courses.Tests.Tests.Integration.Courses;

[Collection(IntegrationCollection.Name)]
public sealed class GetCoursesTests(IntegrationFixture fixture) : IAsyncLifetime
{
    public Task InitializeAsync() => fixture.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Handle_EmptyCollection_ReturnsEmptyList()
    {
        var result = await fixture.Mediator.Send(new GetCoursesQuery());

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WithCourses_ReturnsAllCourses()
    {
        await fixture.Mediator.Send(CourseFactory.CreateCommand(title: "Course A"));
        await fixture.Mediator.Send(CourseFactory.CreateCommand(title: "Course B"));
        await fixture.Mediator.Send(CourseFactory.CreateCommand(title: "Course C"));

        var result = await fixture.Mediator.Send(new GetCoursesQuery());

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        for (var i = 1; i <= 5; i++)
            await fixture.Mediator.Send(CourseFactory.CreateCommand(title: $"Course {i}"));

        var result = await fixture.Mediator.Send(new GetCoursesQuery(Page: 2, PageSize: 2));

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_InvalidPage_ReturnsValidationFailure()
    {
        var result = await fixture.Mediator.Send(new GetCoursesQuery(Page: 0));

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_PageSizeTooLarge_ReturnsValidationFailure()
    {
        var result = await fixture.Mediator.Send(new GetCoursesQuery(PageSize: 101));

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ReturnedItems_ContainExpectedFields()
    {
        await fixture.Mediator.Send(
            CourseFactory.CreateCommand(title: "My Course", difficulty: CourseDifficulty.Advanced));

        var result = await fixture.Mediator.Send(new GetCoursesQuery());

        result.IsSuccess.ShouldBeTrue();
        var item = result.Value.Single();
        item.Title.ShouldBe("My Course");
        item.Difficulty.ShouldBe(CourseDifficulty.Advanced);
        item.Id.ShouldNotBeNullOrEmpty();
    }
}
