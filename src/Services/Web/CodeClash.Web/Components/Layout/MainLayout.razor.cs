using MudBlazor;

namespace CodeClash.Web.Components.Layout;

public partial class MainLayout
{
    private bool _drawerOpen = false;
    private bool _isDarkMode = true;

    private readonly MudTheme _theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#6C63FF",
            Secondary = "#FF6584",
            AppbarBackground = "#1A1A2E",
            AppbarText = "#FFFFFF",
            Background = "#F4F6F8",
            Surface = "#FFFFFF",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#6C63FF",
            Secondary = "#FF6584",
            AppbarBackground = "#0D0D1A",
            Background = "#0F0F1A",
            Surface = "#1A1A2E",
            DrawerBackground = "#16213E",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Inter", "sans-serif"] }
        }
    };

    private void ToggleDrawer() => _drawerOpen = !_drawerOpen;
    private void ToggleDarkMode() => _isDarkMode = !_isDarkMode;
}
