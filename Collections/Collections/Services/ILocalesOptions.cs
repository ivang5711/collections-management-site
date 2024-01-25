using Collections.Models;

namespace Collections.Services;

/// <summary>
/// Provides methods to get Locale data from settings
/// </summary>
public interface ILocalesOptions
{
    /// <summary>
    /// Gets string array of locales names
    /// </summary>
    /// <returns>string array of locales names</returns>
    public string[] GetLocaleNames();

    /// <summary>
    /// Gets List of Locale types
    /// </summary>
    /// <returns>list of Locale types</returns>
    public List<Locale> GetLocales();
}
