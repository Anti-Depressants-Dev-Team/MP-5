using MP5.Core.Interfaces;

namespace MP5.App.Services;

public class ThemeService : IThemeService
{
    public void SetAccentColor(string hexColor)
    {
        if (Application.Current != null && Color.TryParse(hexColor, out var color))
        {
            Application.Current.Resources["AccentPrimary"] = color;
            Application.Current.Resources["TextAccent"] = color;
            Application.Current.Resources["BorderAccent"] = color;
            Application.Current.Resources["ProgressFill"] = color;
        }
    }
}
