namespace CodeClash.E2E.Tests.PageObjects;

public class CourseListPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public CourseListPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public ILocator PageHeading => _page.GetByText("Courses").First;
    public ILocator CreateCourseButton => _page.GetByRole(AriaRole.Link, new() { Name = "Create Course" });
    public ILocator SearchInput => _page.GetByPlaceholder("Search courses...");
    public ILocator CourseCards => _page.Locator(".mud-card");
    public ILocator NoCourseFound => _page.GetByText("No courses found");
    public ILocator ClearFiltersButton => _page.GetByRole(AriaRole.Button, new() { Name = "Clear Filters" });
    public ILocator ShowingCount => _page.Locator("text=Showing");
    public ILocator Pagination => _page.Locator(".mud-pagination");
    public ILocator LoadingSkeletons => _page.Locator(".mud-skeleton");

    public ILocator DifficultyCheckbox(string difficulty) =>
        _page.GetByRole(AriaRole.Checkbox, new() { Name = difficulty });

    public ILocator TechChip(string techName) =>
        _page.Locator(".mud-chip").Filter(new LocatorFilterOptions { HasText = techName });

    public async Task NavigateAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/courses",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }

    public async Task WaitForLoadedAsync()
    {
        await _page.WaitForFunctionAsync(
            "() => document.querySelectorAll('.mud-skeleton').length === 0",
            null, new PageWaitForFunctionOptions { Timeout = 15_000 });
    }

    public async Task SearchAsync(string text)
    {
        await SearchInput.FillAsync(text);
        // Wait for debounce (300ms) + rendering
        await _page.WaitForTimeoutAsync(500);
    }
}
