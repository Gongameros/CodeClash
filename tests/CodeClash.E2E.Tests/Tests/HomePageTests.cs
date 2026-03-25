using CodeClash.E2E.Tests.Infrastructure;
using CodeClash.E2E.Tests.PageObjects;

namespace CodeClash.E2E.Tests.Tests;

public class HomePageTests : BaseE2ETest
{
    public HomePageTests(E2EFixture fixture) : base(fixture) { }

    [Fact]
    public async Task HomePage_DisplaysHeroSection()
    {
        var home = new HomePage(Page, Fixture.WebBaseUrl);
        await home.NavigateAsync();
        await home.WaitForLoadedAsync();

        (await home.HeroTitle.IsVisibleAsync()).ShouldBeTrue();
        (await home.HeroSubtitle.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task HomePage_DisplaysBrowseCoursesButton()
    {
        var home = new HomePage(Page, Fixture.WebBaseUrl);
        await home.NavigateAsync();
        await home.WaitForLoadedAsync();

        (await home.BrowseCoursesButton.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task HomePage_DisplaysFeaturedCoursesSection()
    {
        var home = new HomePage(Page, Fixture.WebBaseUrl);
        await home.NavigateAsync();
        await home.WaitForLoadedAsync();

        (await home.FeaturedCoursesHeading.IsVisibleAsync()).ShouldBeTrue();
        (await home.ViewAllButton.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task HomePage_ShowsStatsSection()
    {
        var home = new HomePage(Page, Fixture.WebBaseUrl);
        await home.NavigateAsync();
        await home.WaitForLoadedAsync();

        (await home.CoursesAvailableStat.IsVisibleAsync()).ShouldBeTrue();
        (await home.LearnersEnrolledStat.IsVisibleAsync()).ShouldBeTrue();
        (await home.TotalXpStat.IsVisibleAsync()).ShouldBeTrue();
    }

    [Fact]
    public async Task BrowseCoursesButton_NavigatesToCourseList()
    {
        var home = new HomePage(Page, Fixture.WebBaseUrl);
        await home.NavigateAsync();
        await home.WaitForLoadedAsync();

        await home.BrowseCoursesButton.ClickAsync();
        await Page.WaitForURLAsync("**/courses", new PageWaitForURLOptions { Timeout = 10_000 });

        Page.Url.ShouldContain("/courses");
    }
}
