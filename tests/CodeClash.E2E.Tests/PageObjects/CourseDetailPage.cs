namespace CodeClash.E2E.Tests.PageObjects;

public class CourseDetailPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public CourseDetailPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    // Header
    public ILocator CourseTitle => _page.Locator(".mud-typography-h4").Filter(new LocatorFilterOptions { HasNotText = "Course Content" });
    public ILocator CourseDescription => _page.Locator(".mud-typography-body1").First;
    public ILocator DraftChip => _page.Locator(".mud-chip").Filter(new LocatorFilterOptions { HasText = "Draft" });
    public ILocator NotFoundAlert => _page.GetByText("Course not found");
    public ILocator LoadingSkeletons => _page.Locator(".mud-skeleton");

    // Owner actions
    public ILocator EditCourseButton => _page.GetByRole(AriaRole.Link, new() { Name = "Edit Course" });
    public ILocator DeleteCourseButton => _page.GetByRole(AriaRole.Button, new() { Name = "Delete Course" });

    // Stats
    public ILocator Rating => _page.Locator("text=/\\d+\\.\\d+ \\(/");
    public ILocator EnrolledCount => _page.Locator("text=/\\d+ enrolled/");

    // Modules
    public ILocator CourseContentHeading => _page.GetByText("Course Content");
    public ILocator AddModuleButton => _page.GetByRole(AriaRole.Button, new() { Name = "Add Module" });
    public ILocator AddFirstModuleButton => _page.GetByRole(AriaRole.Button, new() { Name = "Add First Module" });
    public ILocator NoModulesYet => _page.GetByText("No modules yet");
    public ILocator ModulePanels => _page.Locator(".mud-expand-panel");

    // Module dialog
    public ILocator ModuleTitleInput => _page.Locator(".mud-dialog input").First;
    public ILocator ModuleDescriptionInput => _page.Locator(".mud-dialog textarea").First;
    public ILocator ModuleOrderInput => _page.Locator(".mud-dialog .mud-input-numeric input").First;
    public ILocator ModuleXpInput => _page.Locator(".mud-dialog .mud-input-numeric input").Last;
    public ILocator SaveModuleButton => _page.Locator(".mud-dialog").GetByRole(AriaRole.Button, new() { Name = "Add Module" });
    public ILocator SaveModuleChangesButton => _page.Locator(".mud-dialog").GetByRole(AriaRole.Button, new() { Name = "Save Changes" });
    public ILocator CancelDialogButton => _page.Locator(".mud-dialog").GetByRole(AriaRole.Button, new() { Name = "Cancel" });

    // Lesson dialog
    public ILocator LessonTitleInput => _page.Locator(".mud-dialog input").First;
    public ILocator SaveLessonButton => _page.Locator(".mud-dialog").GetByRole(AriaRole.Button, new() { Name = "Add Lesson" });

    public ILocator AddLessonButton(int moduleIndex) =>
        ModulePanels.Nth(moduleIndex).GetByRole(AriaRole.Button, new() { Name = "Add Lesson" });

    public async Task NavigateAsync(string courseId)
    {
        await _page.GotoAsync($"{_baseUrl}/courses/{courseId}",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }

    public async Task WaitForLoadedAsync()
    {
        await _page.WaitForFunctionAsync(
            "() => document.querySelectorAll('.mud-skeleton').length === 0",
            null, new PageWaitForFunctionOptions { Timeout = 15_000 });
    }

    public async Task ExpandModuleAsync(int index)
    {
        await ModulePanels.Nth(index).Locator(".mud-expand-panel-header").ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task FillModuleDialogAsync(string title, string description, int order = 0, int xp = 100)
    {
        await ModuleTitleInput.FillAsync(title);
        await ModuleDescriptionInput.FillAsync(description);
        await ModuleOrderInput.FillAsync(order.ToString());
        await ModuleXpInput.FillAsync(xp.ToString());
    }
}
