namespace CodeClash.E2E.Tests.PageObjects;

public class EditCoursePage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public EditCoursePage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public ILocator PageHeading => _page.GetByText("Edit Course");
    public ILocator TitleInput => _page.Locator("input").First;
    public ILocator DescriptionInput => _page.Locator("textarea").First;
    public ILocator DifficultySelect => _page.Locator(".mud-select").First;
    public ILocator ThumbnailInput => _page.GetByPlaceholder("https://...");
    public ILocator SaveButton => _page.GetByRole(AriaRole.Button, new() { Name = "Save Changes" });
    public ILocator CancelButton => _page.GetByRole(AriaRole.Link, new() { Name = "Cancel" });
    public ILocator PublishSwitch => _page.Locator(".mud-switch");
    public ILocator LoadingSkeletons => _page.Locator(".mud-skeleton");
    public ILocator NotFoundAlert => _page.GetByText("Course not found");

    // Tags
    public ILocator TagInput => _page.Locator("input").Filter(new LocatorFilterOptions { Has = _page.Locator("[label='Add tag']") });
    public ILocator AddTagButton => _page.GetByRole(AriaRole.Button, new() { Name = "Add" });
    public ILocator TagChips => _page.Locator(".mud-chip").Filter(new LocatorFilterOptions { HasText = "#" });

    // Tech chips
    public ILocator TechChip(string techName) =>
        _page.Locator(".mud-chip").Filter(new LocatorFilterOptions { HasText = techName });

    // Preview
    public ILocator PreviewTitle => _page.Locator("text=Preview").Locator("..").Locator("..").Locator(".mud-typography-h6");

    public async Task NavigateAsync(string courseId)
    {
        await _page.GotoAsync($"{_baseUrl}/courses/{courseId}/edit",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }

    public async Task WaitForLoadedAsync()
    {
        await _page.WaitForFunctionAsync(
            "() => document.querySelectorAll('.mud-skeleton').length === 0",
            null, new PageWaitForFunctionOptions { Timeout = 15_000 });
    }
}
