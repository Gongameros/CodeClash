using CodeClash.E2E.Tests.Auth;
using CodeClash.E2E.Tests.Infrastructure;
using CodeClash.E2E.Tests.PageObjects;
using CodeClash.E2E.Tests.TestData;

namespace CodeClash.E2E.Tests.Tests;

public class MyCoursesTests : BaseE2ETest
{
    public MyCoursesTests(E2EFixture fixture) : base(fixture) { }

    [Fact]
    public async Task MyCourses_PageLoadsSuccessfully()
    {
        var myCoursesPage = new MyCoursesPage(Page, Fixture.WebBaseUrl);
        await myCoursesPage.NavigateAsync();
        await myCoursesPage.WaitForLoadedAsync();

        (await myCoursesPage.PageHeading.IsVisibleAsync()).ShouldBeTrue();
        (await myCoursesPage.SubHeading.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task MyCourses_ShowsCreateCourseButton()
    {
        var myCoursesPage = new MyCoursesPage(Page, Fixture.WebBaseUrl);
        await myCoursesPage.NavigateAsync();
        await myCoursesPage.WaitForLoadedAsync();

        (await myCoursesPage.CreateCourseButton.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task MyCourses_AfterCreatingCourse_ShowsCourseInList()
    {
        // Create a course first
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

        // Navigate to My Courses
        var myCoursesPage = new MyCoursesPage(Page, Fixture.WebBaseUrl);
        await myCoursesPage.NavigateAsync();
        await myCoursesPage.WaitForLoadedAsync();

        // Should show at least one course
        (await myCoursesPage.CourseRows.CountAsync()).ShouldBeGreaterThanOrEqualTo(1);

        // Should show stats
        (await myCoursesPage.TotalCoursesCount.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task MyCourses_RequiresAuthentication()
    {
        // Create unauthenticated context
        var context = await Fixture.CreateContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{Fixture.WebBaseUrl}/my-courses",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Should be redirected to Keycloak login
        var usernameField = page.Locator("#username");
        await usernameField.WaitForAsync(new LocatorWaitForOptions { Timeout = 30_000 });
        (await usernameField.IsVisibleAsync()).ShouldBeTrue();

        await context.CloseAsync();
    }
}
