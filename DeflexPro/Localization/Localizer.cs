using System.Globalization;
using System.Windows;

namespace DeflexPro.Localization;

public static class Localizer
{
    public static bool IsHungarian =>
        CultureInfo.InstalledUICulture.TwoLetterISOLanguageName.Equals("hu", StringComparison.OrdinalIgnoreCase);

    public static string Get(string key, string fallback)
    {
        return Application.Current?.TryFindResource(key) as string ?? fallback;
    }
}
