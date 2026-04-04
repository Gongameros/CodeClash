using CodeClash.E2E.Tests.Infrastructure;
using CodeClash.E2E.Tests.PageObjects;

namespace CodeClash.E2E.Tests.Tests;

public class CourseListTests : BaseE2ETest
{
    public CourseListTests(E2EFixture fixture) : base(fixture) { }

    [Fact]
    public async Task CourseListPage_LoadsSuccessfully()
    {
        var courseList = new CourseListPage(Page, Fixture.WebBaseUrl);
        await courseList.NavigateAsync();
        await courseList.WaitForLoadedAsync();

        (await courseList.PageHeading.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task CourseListPage_ShowsCreateCourseButton()
    {
        var courseList = new CourseListPage(Page, Fixture.WebBaseUrl);
        await courseList.NavigateAsync();
        await courseList.WaitForLoadedAsync();

        (await courseList.CreateCourseButton.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task CreateCourseButton_NavigatesToCreatePage()
    {
        var courseList = new CourseListPage(Page, Fixture.WebBaseUrl);
        await courseList.NavigateAsync();
        await courseList.WaitForLoadedAsync();

        await courseList.CreateCourseButton.ClickAsync();
        await Page.WaitForURLAsync("**/courses/new", new PageWaitForURLOptions { Timeout = 10_000 });

        Page.Url.ShouldContain("/courses/new");
    }

    [Fact]
    public async Task SearchFilter_ShowsNoResultsForGibberish()
    {
        var courseList = new CourseListPage(Page, Fixture.WebBaseUrl);
        await courseList.NavigateAsync();
        await courseList.WaitForLoadedAsync();

        await courseList.SearchAsync("xyznonexistent12345");
        await Page.WaitForTimeoutAsync(500);

        (await courseList.NoCourseFound.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task CourseListPage_ShowsFiltersPanel()
    {
        var courseList = new CourseListPage(Page, Fixture.WebBaseUrl);
        await courseList.NavigateAsync();
        await courseList.WaitForLoadedAsync();

        var filtersHeading = Page.GetByText("Filters");
        (await filtersHeading.IsVisibleAsync()).ShouldBeTrue();

        var difficultyLabel = Page.GetByText("DIFFICULTY");
        (await difficultyLabel.IsVisibleAsync()).ShouldBeTrue();

        var technologyLabel = Page.GetByText("TECHNOLOGY");
        (await technologyLabel.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task DifficultyFilter_CanBeToggled()
    {
        var courseList = new CourseListPage(Page, Fixture.WebBaseUrl);
        await courseList.NavigateAsync();
        await courseList.WaitForLoadedAsync();

        var beginnerCheckbox = Page.GetByText("Beginner").First;
        await beginnerCheckbox.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Clear filters button should appear
        (await courseList.ClearFiltersButton.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task ClearFilters_ResetsAllFilters()
    {
        var courseList = new CourseListPage(Page, Fixture.WebBaseUrl);
        await courseList.NavigateAsync();
        await courseList.WaitForLoadedAsync();

        // Apply a filter
        await courseList.SearchAsync("test");
        await Page.WaitForTimeoutAsync(500);

        // Clear it
        if (await courseList.ClearFiltersButton.IsVisibleAsync())
        {
            await courseList.ClearFiltersButton.ClickAsync();
            await Page.WaitForTimeoutAsync(500);

            var searchValue = await courseList.SearchInput.InputValueAsync();
            searchValue.ShouldBeEmpty();
        }
    }
}
