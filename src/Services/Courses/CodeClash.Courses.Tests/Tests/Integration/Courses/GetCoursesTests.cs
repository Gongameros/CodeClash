using CodeClash.Courses.Features.Courses.GetCourses;
using CodeClash.Courses.Tests.Infrastructure;
using CodeClash.Courses.Tests.TestData;
using Shouldly;

namespace CodeClash.Courses.Tests.Tests.Integration.Courses;

[Collection(IntegrationCollection.Name)]
public sealed class GetCoursesTests(IntegrationFixture fixture) : IAsyncLifetime
{
    public ValueTask InitializeAsync() => new(fixture.ResetAsync());
    public ValueTask DisposeAsync() => default;

    [Fact]
    public async Task Handle_EmptyCollection_ReturnsEmptyList()
    {
        var result = await fixture.Mediator.Send(new GetCoursesQuery(), Ct.Token);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WithCourses_ReturnsAllCourses()
    {
        await fixture.Mediator.Send(CourseFactory.CreateCommand(title: "Course A"), Ct.Token);
        await fixture.Mediator.Send(CourseFactory.CreateCommand(title: "Course B"), Ct.Token);
        await fixture.Mediator.Send(CourseFactory.CreateCommand(title: "Course C"), Ct.Token);

        var result = await fixture.Mediator.Send(new GetCoursesQuery(), Ct.Token);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        for (var i = 1; i <= 5; i++)
            await fixture.Mediator.Send(CourseFactory.CreateCommand(title: $"Course {i}"), Ct.Token);

        var result = await fixture.Mediator.Send(new GetCoursesQuery(Page: 2, PageSize: 2), Ct.Token);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Handle_InvalidPage_ReturnsValidationFailure()
    {
        var result = await fixture.Mediator.Send(new GetCoursesQuery(Page: 0), Ct.Token);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_PageSizeTooLarge_ReturnsValidationFailure()
    {
        var result = await fixture.Mediator.Send(new GetCoursesQuery(PageSize: 101), Ct.Token);

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_ReturnedItems_ContainExpectedFields()
    {
        await fixture.Mediator.Send(
            CourseFactory.CreateCommand(title: "My Course", difficulty: CourseDifficulty.Advanced), Ct.Token);

        var result = await fixture.Mediator.Send(new GetCoursesQuery(), Ct.Token);

        result.IsSuccess.ShouldBeTrue();
        var item = result.Value.Single();
        item.Title.ShouldBe("My Course");
        item.Difficulty.ShouldBe(CourseDifficulty.Advanced);
        item.Id.ShouldNotBeNullOrEmpty();
    }
}
