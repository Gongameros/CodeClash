namespace CodeClash.E2E.Tests.PageObjects.Components;

public class NavBarComponent
{
    private readonly IPage _page;

    public NavBarComponent(IPage page) => _page = page;

    public ILocator LoginButton => _page.GetByRole(AriaRole.Link, new() { Name = "Login" });
    public ILocator LogoutButton => _page.GetByRole(AriaRole.Link, new() { Name = "Logout" });
    public ILocator MyCoursesLink => _page.GetByRole(AriaRole.Link, new() { Name = "My Courses" });
    public ILocator CoursesLink => _page.GetByRole(AriaRole.Link, new() { Name = "Courses" });
    public ILocator HomeLink => _page.Locator("a[href='/']").First;

    public async Task<bool> IsLoggedInAsync()
    {
        return await LogoutButton.IsVisibleAsync();
    }
}
