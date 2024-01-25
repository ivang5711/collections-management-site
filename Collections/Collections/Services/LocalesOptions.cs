using Collections.Models;

namespace Collections.Services;

public class LocalesOptions(IConfiguration configuration) : ILocalesOptions
{
    public List<Locale> Locales { get; set; } = configuration
        .GetSection("Locales").Get<List<Locale>>() ?? [];

    public string[] GetLocaleNames()
    {
        string[] supportedCultures = new string[Locales.Count];
        int i = 0;
        foreach (Locale item in Locales)
        {
            supportedCultures[i++] = item.Name!;
        }

        return supportedCultures;
    }

    public List<Locale> GetLocales()
    {
        return Locales;
    }
}