using CodeClash.E2E.Tests.Infrastructure;
using CodeClash.E2E.Tests.PageObjects;
using CodeClash.E2E.Tests.TestData;

namespace CodeClash.E2E.Tests.Tests;

public class ModuleManagementTests : BaseE2ETest
{
    public ModuleManagementTests(E2EFixture fixture) : base(fixture) { }

    [Fact]
    public async Task AddModule_ValidData_AppearsInModulesList()
    {
        var courseId = await CreateCourseViaUiAsync();

        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        // Click Add Module
        await detailPage.AddFirstModuleButton.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Fill module dialog
        var moduleData = TestCourseData.DefaultModule();
        await detailPage.FillModuleDialogAsync(
            moduleData.Title, moduleData.Description, moduleData.Order, moduleData.XpReward);

        await detailPage.SaveModuleButton.ClickAsync();
        await WaitForSnackbarAsync("Module");

        // Module should appear in the list
        await Page.WaitForTimeoutAsync(500);
        (await detailPage.ModulePanels.CountAsync()).ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task AddModule_OwnerSeesAddModuleButton()
    {
        var courseId = await CreateCourseViaUiAsync();

        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        // Owner should see add module buttons
        var addButton = detailPage.AddModuleButton.Or(detailPage.AddFirstModuleButton);
        (await addButton.First.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task ExpandModule_ShowsLessonsAndAddLessonButton()
    {
        var courseId = await CreateCourseViaUiAsync();
        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);

        // Add a module first
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        await detailPage.AddFirstModuleButton.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        var moduleData = TestCourseData.DefaultModule();
        await detailPage.FillModuleDialogAsync(
            moduleData.Title, moduleData.Description, moduleData.Order, moduleData.XpReward);
        await detailPage.SaveModuleButton.ClickAsync();
        await Page.WaitForTimeoutAsync(1000);

        // Refresh page to see the module
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        // Expand the module
        await detailPage.ExpandModuleAsync(0);

        // Should see Add Lesson button
        (await detailPage.AddLessonButton(0).IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteModule_RemovesFromList()
    {
        var courseId = await CreateCourseViaUiAsync();
        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);

        // Add a module
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        await detailPage.AddFirstModuleButton.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        var moduleData = TestCourseData.DefaultModule();
        await detailPage.FillModuleDialogAsync(
            moduleData.Title, moduleData.Description, moduleData.Order, moduleData.XpReward);
        await detailPage.SaveModuleButton.ClickAsync();
        await Page.WaitForTimeoutAsync(1000);

        // Refresh and verify module exists
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();
        (await detailPage.ModulePanels.CountAsync()).ShouldBe(1);

        // Delete the module — click the delete icon button (Color.Error) in the module header
        var moduleHeader = detailPage.ModulePanels.First.Locator(".mud-expand-panel-header");
        var moduleDeleteBtn = moduleHeader.Locator("button.mud-icon-button").Last;
        await moduleDeleteBtn.ClickAsync();

        // Wait for MudBlazor confirmation dialog to fully render
        var confirmDialog = Page.Locator(".mud-message-box");
        await confirmDialog.WaitForAsync(new LocatorWaitForOptions { Timeout = 5_000 });
        await Page.WaitForTimeoutAsync(500); // Wait for dialog animation

        // Click "Delete" button inside the message box dialog
        var confirmButton = confirmDialog.GetByRole(AriaRole.Button, new() { Name = "Delete" });
        await confirmButton.ClickAsync();

        await Page.WaitForTimeoutAsync(1000);

        // Refresh and verify module is gone
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();
        (await detailPage.NoModulesYet.IsVisibleAsync()).ShouldBeTrue();
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

        var url = Page.Url;
        return url.Split("/courses/").Last().Split('?').First().Split('/').First();
    }
}
