using Collections.Models;

namespace Collections.Services
{
    public interface ILocalesOptions
    {
        public string[] GetLocaleNames();

        public List<Locale> GetLocales();
    }
}
