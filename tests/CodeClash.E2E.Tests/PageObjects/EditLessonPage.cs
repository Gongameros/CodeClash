namespace CodeClash.E2E.Tests.PageObjects;

public class EditLessonPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public EditLessonPage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public ILocator PageHeading => _page.GetByText("Edit Lesson");
    public ILocator TitleInput => _page.Locator("input").First;
    public ILocator ContentTextArea => _page.Locator("textarea").First;
    public ILocator SaveButton => _page.GetByRole(AriaRole.Button, new() { Name = "Save" });
    public ILocator LoadingSkeletons => _page.Locator(".mud-skeleton");
    public ILocator NotFoundAlert => _page.GetByText("Lesson not found");
    public ILocator PreviewTab => _page.GetByText("Preview");

    public async Task NavigateAsync(string courseId, string moduleId, string lessonId)
    {
        await _page.GotoAsync($"{_baseUrl}/courses/{courseId}/modules/{moduleId}/lessons/{lessonId}/edit",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }

    public async Task WaitForLoadedAsync()
    {
        await _page.WaitForFunctionAsync(
            "() => document.querySelectorAll('.mud-skeleton').length === 0",
            null, new PageWaitForFunctionOptions { Timeout = 15_000 });
    }
}
