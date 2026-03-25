using CodeClash.E2E.Tests.Infrastructure;
using CodeClash.E2E.Tests.PageObjects;
using CodeClash.E2E.Tests.TestData;

namespace CodeClash.E2E.Tests.Tests;

public class EditCourseTests : BaseE2ETest
{
    public EditCourseTests(E2EFixture fixture) : base(fixture) { }

    [Fact]
    public async Task EditCourse_LoadsExistingData()
    {
        var courseId = await CreateCourseViaUiAsync("Edit Load Test Course");

        var editPage = new EditCoursePage(Page, Fixture.WebBaseUrl);
        await editPage.NavigateAsync(courseId);
        await editPage.WaitForLoadedAsync();

        (await editPage.PageHeading.IsVisibleAsync()).ShouldBeTrue();

        var titleValue = await editPage.TitleInput.InputValueAsync();
        titleValue.ShouldContain("Edit Load Test Course");
    }

    [Fact]
    public async Task EditCourse_UpdateTitle_SavesSuccessfully()
    {
        var courseId = await CreateCourseViaUiAsync("Original Title");

        var editPage = new EditCoursePage(Page, Fixture.WebBaseUrl);
        await editPage.NavigateAsync(courseId);
        await editPage.WaitForLoadedAsync();

        // Update title
        await editPage.TitleInput.ClearAsync();
        await editPage.TitleInput.FillAsync("Updated Title");

        await editPage.SaveButton.ClickAsync();

        await WaitForSnackbarAsync("saved");

        // Verify we're redirected
        await Page.WaitForURLAsync($"**/courses/{courseId}",
            new PageWaitForURLOptions { Timeout = 10_000 });
    }

    [Fact]
    public async Task EditCourse_CancelButton_NavigatesToDetail()
    {
        var courseId = await CreateCourseViaUiAsync("Cancel Test Course");

        var editPage = new EditCoursePage(Page, Fixture.WebBaseUrl);
        await editPage.NavigateAsync(courseId);
        await editPage.WaitForLoadedAsync();

        await editPage.CancelButton.ClickAsync();
        await Page.WaitForURLAsync($"**/courses/{courseId}",
            new PageWaitForURLOptions { Timeout = 10_000 });
    }

    [Fact]
    public async Task EditCourse_ShowsPreviewPanel()
    {
        var courseId = await CreateCourseViaUiAsync("Preview Test Course");

        var editPage = new EditCoursePage(Page, Fixture.WebBaseUrl);
        await editPage.NavigateAsync(courseId);
        await editPage.WaitForLoadedAsync();

        var previewHeading = Page.GetByText("Preview");
        (await previewHeading.IsVisibleAsync()).ShouldBeTrue();
    }

    private async Task<string> CreateCourseViaUiAsync(string title)
    {
        var createPage = new CreateCoursePage(Page, Fixture.WebBaseUrl);
        await createPage.NavigateAsync();

        var data = TestCourseData.Default(TestCourseData.UniqueTitle(title));
        await createPage.FillBasicInfoAsync(data.Title, data.Description);
        await createPage.GoToNextStepAsync();

        // Step 1: Select at least one technology (required by API)
        foreach (var tech in data.Technologies)
            await createPage.SelectTechAsync(tech);
        await createPage.GoToNextStepAsync();
        await createPage.GoToNextStepAsync();
        await createPage.SubmitAsync();

        await Page.WaitForURLAsync(
            url => url.Contains("/courses/") && !url.TrimEnd('/').EndsWith("/courses/new"),
            new PageWaitForURLOptions { Timeout = 15_000 });

        var url = Page.Url;
        return url.Split("/courses/").Last().Split('?').First().Split('/').First();
    }
}
