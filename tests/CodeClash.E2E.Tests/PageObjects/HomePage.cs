namespace CodeClash.E2E.Tests.PageObjects;

public class HomePage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public HomePage(IPage page, string baseUrl)
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    public ILocator HeroTitle => _page.Locator(".hero-section").GetByText("<CodeClash />");
    public ILocator HeroSubtitle => _page.GetByText("Master coding through interactive challenges");
    public ILocator BrowseCoursesButton => _page.GetByRole(AriaRole.Link, new() { Name = "Browse Courses" });
    public ILocator ViewAllButton => _page.GetByRole(AriaRole.Link, new() { Name = "View All" });

    // Stats
    public ILocator CoursesAvailableStat => _page.GetByText("Courses Available", new() { Exact = true });
    public ILocator LearnersEnrolledStat => _page.GetByText("Learners Enrolled");
    public ILocator TotalXpStat => _page.GetByText("Total XP to Earn");

    // Featured courses
    public ILocator FeaturedCoursesHeading => _page.GetByText("Featured Courses");
    public ILocator CourseCards => _page.Locator(".mud-card");
    public ILocator EmptyStateAlert => _page.GetByText("No courses available yet");
    public ILocator LoadingSkeletons => _page.Locator(".mud-skeleton");

    public async Task NavigateAsync()
    {
        await _page.GotoAsync(_baseUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    }

    public async Task WaitForLoadedAsync()
    {
        await _page.WaitForFunctionAsync(
            "() => document.querySelectorAll('.mud-skeleton').length === 0",
            null, new PageWaitForFunctionOptions { Timeout = 15_000 });
    }
}
