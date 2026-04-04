namespace CodeClash.E2E.Tests.PageObjects;

public class CreateCoursePage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public CreateCoursePage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    // Step indicators
    public ILocator StepIndicator(string stepName) =>
        _page.GetByText(stepName, new PageGetByTextOptions { Exact = false });

    // Step 0: Basic Info
    public ILocator TitleInput => _page.Locator("input").Nth(0);
    public ILocator DescriptionInput => _page.Locator("textarea").First;
    public ILocator DifficultySelect => _page.Locator(".mud-select").First;
    public ILocator ThumbnailInput => _page.GetByPlaceholder("https://...");

    // Scope selectors to the main form area (exclude Live Preview panel)
    private ILocator FormArea => _page.Locator(".mud-grid-item-md-8");

    // Step 1: Technologies
    public ILocator TechChip(string techName) =>
        FormArea.Locator(".mud-chip").Filter(new LocatorFilterOptions { HasText = techName });

    public ILocator SelectedTechCount => FormArea.Locator("text=/\\d+ selected:/");

    // Step 2: Tags
    public ILocator TagInput => _page.GetByPlaceholder("e.g. algorithms, data-structures");
    public ILocator AddTagButton => _page.GetByRole(AriaRole.Button, new() { Name = "Add" });
    public ILocator TagChips => FormArea.Locator(".mud-chip").Filter(new LocatorFilterOptions { HasText = "#" });

    // Step 3: Review
    public ILocator PublishToggle => _page.Locator(".mud-switch");
    public ILocator CreateCourseButton => _page.GetByRole(AriaRole.Button, new() { Name = "Create Course" });

    // Navigation
    public ILocator NextButton => _page.GetByRole(AriaRole.Button, new() { Name = "Next" });
    public ILocator PreviousButton => _page.GetByRole(AriaRole.Button, new() { Name = "Previous" });

    // Live preview
    public ILocator PreviewTitle => _page.Locator("text=Live Preview").Locator("..").Locator("..").Locator(".mud-typography-h6");

    public async Task NavigateAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/courses/new",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }

    public async Task FillBasicInfoAsync(string title, string description)
    {
        await TitleInput.FillAsync(title);
        await DescriptionInput.FillAsync(description);
        // Wait for Blazor Server SignalR round-trip to process bindings
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task SelectDifficultyAsync(string difficulty)
    {
        await DifficultySelect.ClickAsync();
        await _page.GetByRole(AriaRole.Option, new() { Name = difficulty }).ClickAsync();
    }

    public async Task SelectTechAsync(string techName)
    {
        await TechChip(techName).ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task AddTagAsync(string tag)
    {
        await TagInput.FillAsync(tag);
        // Wait for Blazor to enable the Add button
        await _page.WaitForTimeoutAsync(300);
        await AddTagButton.ClickAsync();
        // Wait for chip to render
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task GoToNextStepAsync()
    {
        await NextButton.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task GoToPreviousStepAsync()
    {
        await PreviousButton.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task SubmitAsync()
    {
        await CreateCourseButton.ClickAsync();
    }
}
