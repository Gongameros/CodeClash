namespace CodeClash.Web.Components.Auth;

public partial class RedirectToHome
{
    protected override void OnInitialized()
    {
        Navigation.NavigateTo("/", replace: true);
    }
}
