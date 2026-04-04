using CodeClash.E2E.Tests.Infrastructure;
using CodeClash.E2E.Tests.PageObjects;
using CodeClash.E2E.Tests.TestData;

namespace CodeClash.E2E.Tests.Tests;

public class LessonManagementTests : BaseE2ETest
{
    public LessonManagementTests(E2EFixture fixture) : base(fixture) { }

    [Fact]
    public async Task AddLesson_ValidData_AppearsInModule()
    {
        var courseId = await CreateCourseWithModuleAsync();

        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        // Expand the module
        await detailPage.ExpandModuleAsync(0);

        // Click Add Lesson
        await detailPage.AddLessonButton(0).ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Fill lesson dialog
        var lessonData = TestCourseData.DefaultLesson();
        await detailPage.LessonTitleInput.FillAsync(lessonData.Title);
        await detailPage.SaveLessonButton.ClickAsync();

        await WaitForSnackbarAsync("Lesson");
        await Page.WaitForTimeoutAsync(500);

        // Refresh and verify
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();
        await detailPage.ExpandModuleAsync(0);

        // Lesson should be visible in the expanded module
        var lessonText = Page.GetByText(lessonData.Title);
        (await lessonText.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task EditLesson_NavigatesToEditPage()
    {
        var courseId = await CreateCourseWithModuleAndLessonAsync();

        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        // Expand the module
        await detailPage.ExpandModuleAsync(0);

        // Click edit on the lesson
        var lessonEditButton = detailPage.ModulePanels.First
            .Locator(".mud-list-item").First
            .Locator(".mud-icon-button").First;

        await lessonEditButton.ClickAsync();

        // Should navigate to edit lesson page
        await Page.WaitForURLAsync("**/lessons/*/edit",
            new PageWaitForURLOptions { Timeout = 10_000 });
        Page.Url.ShouldContain("/lessons/");
        Page.Url.ShouldContain("/edit");
    }

    [Fact]
    public async Task DeleteLesson_RemovesFromModule()
    {
        var courseId = await CreateCourseWithModuleAndLessonAsync();

        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        // Expand the module
        await detailPage.ExpandModuleAsync(0);

        // Count lessons before
        var lessonsBefore = await detailPage.ModulePanels.First
            .Locator(".mud-list-item").CountAsync();

        // Click delete on the lesson (last icon button in the lesson row)
        var lessonDeleteButton = detailPage.ModulePanels.First
            .Locator(".mud-list-item").First
            .Locator(".mud-icon-button").Last;

        await lessonDeleteButton.ClickAsync();

        // Wait for MudBlazor confirmation dialog to fully render
        var confirmDialog = Page.Locator(".mud-message-box");
        await confirmDialog.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        await Page.WaitForTimeoutAsync(500); // Wait for dialog animation

        // Click "Delete" button inside the message box dialog
        var confirmButton = confirmDialog.GetByRole(AriaRole.Button, new() { Name = "Delete" });
        await confirmButton.ClickAsync();

        await Page.WaitForTimeoutAsync(1000);

        // Refresh and verify
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();
        await detailPage.ExpandModuleAsync(0);

        var lessonsAfter = await detailPage.ModulePanels.First
            .Locator(".mud-list-item").CountAsync();

        lessonsAfter.ShouldBeLessThan(lessonsBefore);
    }

    [Fact]
    public async Task EditLessonPage_LoadsExistingData()
    {
        var courseId = await CreateCourseWithModuleAndLessonAsync();

        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        // Expand the module and navigate to edit lesson
        await detailPage.ExpandModuleAsync(0);

        var lessonEditButton = detailPage.ModulePanels.First
            .Locator(".mud-list-item").First
            .Locator(".mud-icon-button").First;

        await lessonEditButton.ClickAsync();
        await Page.WaitForURLAsync("**/lessons/*/edit",
            new PageWaitForURLOptions { Timeout = 10_000 });

        var editLessonPage = new EditLessonPage(Page, Fixture.WebBaseUrl);
        await editLessonPage.WaitForLoadedAsync();

        // Title should be pre-filled
        var titleValue = await editLessonPage.TitleInput.InputValueAsync();
        titleValue.ShouldNotBeEmpty();
    }

    private async Task<string> CreateCourseViaUiAsync()
    {
        var createPage = new CreateCoursePage(Page, Fixture.WebBaseUrl);
        await createPage.NavigateAsync();

        var data = TestCourseData.Default();
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

        return Page.Url.Split("/courses/").Last().Split('?').First().Split('/').First();
    }

    private async Task<string> CreateCourseWithModuleAsync()
    {
        var courseId = await CreateCourseViaUiAsync();

        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        // Add module
        await detailPage.AddFirstModuleButton.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        var moduleData = TestCourseData.DefaultModule();
        await detailPage.FillModuleDialogAsync(
            moduleData.Title, moduleData.Description, moduleData.Order, moduleData.XpReward);
        await detailPage.SaveModuleButton.ClickAsync();
        await Page.WaitForTimeoutAsync(1000);

        return courseId;
    }

    private async Task<string> CreateCourseWithModuleAndLessonAsync()
    {
        var courseId = await CreateCourseWithModuleAsync();

        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        // Expand module and add lesson
        await detailPage.ExpandModuleAsync(0);
        await detailPage.AddLessonButton(0).ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        var lessonData = TestCourseData.DefaultLesson();
        await detailPage.LessonTitleInput.FillAsync(lessonData.Title);
        await detailPage.SaveLessonButton.ClickAsync();
        await Page.WaitForTimeoutAsync(1000);

        return courseId;
    }
}
