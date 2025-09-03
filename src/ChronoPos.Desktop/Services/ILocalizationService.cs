using System.Globalization;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Supported languages in the application
/// </summary>
public enum SupportedLanguage
{
    English,
    Urdu
}

/// <summary>
/// Interface for managing application localization
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets the current language
    /// </summary>
    SupportedLanguage CurrentLanguage { get; }
    
    /// <summary>
    /// Gets the current culture info
    /// </summary>
    CultureInfo CurrentCulture { get; }
    
    /// <summary>
    /// Event raised when the language changes
    /// </summary>
    event Action<SupportedLanguage>? LanguageChanged;
    
    /// <summary>
    /// Changes the application language
    /// </summary>
    /// <param name="language">The new language to apply</param>
    void ChangeLanguage(SupportedLanguage language);
    
    /// <summary>
    /// Loads the language from application settings
    /// </summary>
    void LoadLanguageFromSettings();
    
    /// <summary>
    /// Gets a localized string by key
    /// </summary>
    /// <param name="key">The localization key</param>
    /// <returns>The localized string</returns>
    string GetString(string key);
    
    /// <summary>
    /// Gets all available languages for UI display
    /// </summary>
    Dictionary<SupportedLanguage, string> GetAvailableLanguages();
}
