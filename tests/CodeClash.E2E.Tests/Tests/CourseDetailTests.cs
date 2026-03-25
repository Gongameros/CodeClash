using CodeClash.E2E.Tests.Infrastructure;
using CodeClash.E2E.Tests.PageObjects;
using CodeClash.E2E.Tests.TestData;

namespace CodeClash.E2E.Tests.Tests;

public class CourseDetailTests : BaseE2ETest
{
    public CourseDetailTests(E2EFixture fixture) : base(fixture) { }

    [Fact]
    public async Task CourseDetail_CreatedCourse_DisplaysAllInfo()
    {
        // First create a course via the UI
        var courseId = await CreateCourseViaUiAsync();

        // Navigate to detail page
        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        (await detailPage.CourseTitle.IsVisibleAsync()).ShouldBeTrue();
        (await detailPage.CourseContentHeading.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task CourseDetail_Owner_SeesEditAndDeleteButtons()
    {
        var courseId = await CreateCourseViaUiAsync();

        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        (await detailPage.EditCourseButton.IsVisibleAsync()).ShouldBeTrue();
        (await detailPage.DeleteCourseButton.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task CourseDetail_ShowsModulesSection()
    {
        var courseId = await CreateCourseViaUiAsync();

        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);
        await detailPage.NavigateAsync(courseId);
        await detailPage.WaitForLoadedAsync();

        (await detailPage.CourseContentHeading.IsVisibleAsync()).ShouldBeTrue();

        // New course should have no modules
        (await detailPage.NoModulesYet.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task CourseDetail_InvalidId_ShowsNotFound()
    {
        var detailPage = new CourseDetailPage(Page, Fixture.WebBaseUrl);
        await detailPage.NavigateAsync("000000000000000000000000");
        await detailPage.WaitForLoadedAsync();

        (await detailPage.NotFoundAlert.IsVisibleAsync()).ShouldBeTrue();
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

        // Extract course ID from URL
        var url = Page.Url;
        var courseId = url.Split("/courses/").Last().Split('?').First().Split('/').First();
        return courseId;
    }
}
