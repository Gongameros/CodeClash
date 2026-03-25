namespace CodeClash.E2E.Tests.PageObjects;

public class MyCoursesPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public MyCoursesPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public ILocator PageHeading => _page.GetByText("My Courses").First;
    public ILocator SubHeading => _page.GetByText("Manage the courses you've created");
    public ILocator CreateCourseButton => _page.GetByRole(AriaRole.Link, new() { Name = "Create Course" });
    public ILocator CreateFirstCourseButton => _page.GetByRole(AriaRole.Link, new() { Name = "Create First Course" });
    public ILocator EmptyState => _page.GetByText("No courses yet");
    public ILocator CourseRows => _page.Locator(".mud-paper").Filter(new LocatorFilterOptions { HasText = "View" });
    public ILocator LoadingSkeletons => _page.Locator(".mud-skeleton");

    // Stats
    public ILocator TotalCoursesCount => _page.GetByText("Total Courses").Locator("..");
    public ILocator PublishedCount => _page.GetByText("Published").Locator("..");
    public ILocator DraftsCount => _page.GetByText("Drafts").Locator("..");

    public ILocator ViewButton(int rowIndex) =>
        CourseRows.Nth(rowIndex).GetByRole(AriaRole.Link, new() { Name = "View" });

    public ILocator EditButton(int rowIndex) =>
        CourseRows.Nth(rowIndex).GetByRole(AriaRole.Link, new() { Name = "Edit" });

    public ILocator PublishButton(int rowIndex) =>
        CourseRows.Nth(rowIndex).GetByRole(AriaRole.Button, new() { Name = "Publish" });

    public ILocator UnpublishButton(int rowIndex) =>
        CourseRows.Nth(rowIndex).GetByRole(AriaRole.Button, new() { Name = "Unpublish" });

    public async Task NavigateAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/my-courses",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }

    public async Task WaitForLoadedAsync()
    {
        await _page.WaitForFunctionAsync(
            "() => document.querySelectorAll('.mud-skeleton').length === 0",
            null, new PageWaitForFunctionOptions { Timeout = 15_000 });
    }
}
