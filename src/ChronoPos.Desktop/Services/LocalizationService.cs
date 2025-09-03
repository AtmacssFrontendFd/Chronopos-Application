using System.Globalization;
using System.Windows;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Service for managing application localization and language switching
/// </summary>
public class LocalizationService : ILocalizationService
{
    private SupportedLanguage _currentLanguage = SupportedLanguage.English;
    private readonly Dictionary<SupportedLanguage, Dictionary<string, string>> _localizations;
    private readonly Dictionary<SupportedLanguage, CultureInfo> _cultures;
    private readonly Dictionary<SupportedLanguage, string> _languageNames;

    public SupportedLanguage CurrentLanguage => _currentLanguage;
    public CultureInfo CurrentCulture => _cultures[_currentLanguage];
    
    public event Action<SupportedLanguage>? LanguageChanged;

    public LocalizationService()
    {
        _cultures = new Dictionary<SupportedLanguage, CultureInfo>
        {
            { SupportedLanguage.English, new CultureInfo("en-US") },
            { SupportedLanguage.Urdu, new CultureInfo("ur-PK") }
        };

        _languageNames = new Dictionary<SupportedLanguage, string>
        {
            { SupportedLanguage.English, "English" },
            { SupportedLanguage.Urdu, "اردو" }
        };

        _localizations = InitializeLocalizations();
    }

    public void ChangeLanguage(SupportedLanguage language)
    {
        if (_currentLanguage == language)
            return;

        _currentLanguage = language;
        
        // Set thread culture
        var culture = _cultures[language];
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        // Update application resources
        UpdateApplicationResources();

        // Save language preference
        SaveLanguageToSettings(language);

        // Notify about language change
        LanguageChanged?.Invoke(language);
    }

    public void LoadLanguageFromSettings()
    {
        try
        {
            var savedLanguage = Properties.Settings.Default.Language;
            if (Enum.TryParse<SupportedLanguage>(savedLanguage, out var language))
            {
                ChangeLanguage(language);
            }
            else
            {
                ChangeLanguage(SupportedLanguage.English);
            }
        }
        catch
        {
            ChangeLanguage(SupportedLanguage.English);
        }
    }

    public string GetString(string key)
    {
        if (_localizations.TryGetValue(_currentLanguage, out var languageDict) &&
            languageDict.TryGetValue(key, out var value))
        {
            return value;
        }

        // Fallback to English if key not found in current language
        if (_currentLanguage != SupportedLanguage.English &&
            _localizations.TryGetValue(SupportedLanguage.English, out var englishDict) &&
            englishDict.TryGetValue(key, out var englishValue))
        {
            return englishValue;
        }

        return key; // Return key if no translation found
    }

    public Dictionary<SupportedLanguage, string> GetAvailableLanguages()
    {
        return new Dictionary<SupportedLanguage, string>(_languageNames);
    }

    private Dictionary<SupportedLanguage, Dictionary<string, string>> InitializeLocalizations()
    {
        return new Dictionary<SupportedLanguage, Dictionary<string, string>>
        {
            {
                SupportedLanguage.English,
                new Dictionary<string, string>
                {
                    // Navigation
                    { "Dashboard", "Dashboard" },
                    { "Transactions", "Transactions" },
                    { "Management", "Management" },
                    { "Reservation", "Reservation" },
                    { "OrderTable", "Order Table" },
                    { "Reports", "Reports" },
                    { "Settings", "Settings" },
                    { "Logout", "Logout" },
                    
                    // Common Actions
                    { "Save", "Save" },
                    { "Cancel", "Cancel" },
                    { "Delete", "Delete" },
                    { "Edit", "Edit" },
                    { "Add", "Add" },
                    { "Search", "Search" },
                    { "Filter", "Filter" },
                    { "Export", "Export" },
                    { "Import", "Import" },
                    { "Print", "Print" },
                    
                    // Settings Categories
                    { "GeneralSettings", "General Settings" },
                    { "Appearance", "Appearance" },
                    { "Language", "Language" },
                    { "FontSize", "Font Size" },
                    { "ColorScheme", "Color Scheme" },
                    { "LayoutDirection", "Layout Direction" },
                    
                    // Theme
                    { "LightTheme", "Light" },
                    { "DarkTheme", "Dark" },
                    { "CurrentTheme", "Current Theme:" },
                    
                    // Font Sizes
                    { "VerySmall", "Very Small" },
                    { "Small", "Small" },
                    { "Medium", "Medium" },
                    { "Large", "Large" },
                    { "CurrentFontSize", "Current Font Size:" },
                    
                    // Colors
                    { "PrimaryColor", "Primary Color" },
                    { "BackgroundColor", "Background Color" },
                    { "CurrentPrimaryColor", "Current Primary:" },
                    { "CurrentBackgroundColor", "Current Background:" },
                    
                    // Layout Direction
                    { "LeftToRight", "Left to Right (LTR)" },
                    { "RightToLeft", "Right to Left (RTL)" },
                    { "CurrentDirection", "Current Direction:" },
                    
                    // Status Messages
                    { "SettingsSaved", "Settings saved successfully" },
                    { "LanguageChanged", "Language changed successfully" },
                    { "ThemeChanged", "Theme changed successfully" },
                    { "ColorChanged", "Color scheme changed successfully" },
                    { "DirectionChanged", "Layout direction changed successfully" },
                    
                    // Application Info
                    { "ApplicationName", "ChronoPos" },
                    { "ApplicationDescription", "Point of Sale System" },
                    { "Version", "Version" },
                    { "BuildDate", "Build Date" }
                }
            },
            {
                SupportedLanguage.Urdu,
                new Dictionary<string, string>
                {
                    // Navigation (Urdu translations)
                    { "Dashboard", "ڈیش بورڈ" },
                    { "Transactions", "لین دین" },
                    { "Management", "انتظام" },
                    { "Reservation", "بکنگ" },
                    { "OrderTable", "آرڈر ٹیبل" },
                    { "Reports", "رپورٹس" },
                    { "Settings", "ترتیبات" },
                    { "Logout", "لاگ آؤٹ" },
                    
                    // Common Actions (Urdu translations)
                    { "Save", "محفوظ کریں" },
                    { "Cancel", "منسوخ" },
                    { "Delete", "حذف کریں" },
                    { "Edit", "تبدیل کریں" },
                    { "Add", "شامل کریں" },
                    { "Search", "تلاش" },
                    { "Filter", "فلٹر" },
                    { "Export", "برآمد" },
                    { "Import", "درآمد" },
                    { "Print", "پرنٹ" },
                    
                    // Settings Categories (Urdu translations)
                    { "GeneralSettings", "عمومی ترتیبات" },
                    { "Appearance", "ظاہری شکل" },
                    { "Language", "زبان" },
                    { "FontSize", "فونٹ کا سائز" },
                    { "ColorScheme", "رنگ کی اسکیم" },
                    { "LayoutDirection", "لے آؤٹ کی سمت" },
                    
                    // Theme (Urdu translations)
                    { "LightTheme", "ہلکا" },
                    { "DarkTheme", "گہرا" },
                    { "CurrentTheme", "موجودہ تھیم:" },
                    
                    // Font Sizes (Urdu translations)
                    { "VerySmall", "بہت چھوٹا" },
                    { "Small", "چھوٹا" },
                    { "Medium", "درمیانہ" },
                    { "Large", "بڑا" },
                    { "CurrentFontSize", "موجودہ فونٹ سائز:" },
                    
                    // Colors (Urdu translations)
                    { "PrimaryColor", "بنیادی رنگ" },
                    { "BackgroundColor", "پس منظر کا رنگ" },
                    { "CurrentPrimaryColor", "موجودہ بنیادی:" },
                    { "CurrentBackgroundColor", "موجودہ پس منظر:" },
                    
                    // Layout Direction (Urdu translations)
                    { "LeftToRight", "بائیں سے دائیں" },
                    { "RightToLeft", "دائیں سے بائیں" },
                    { "CurrentDirection", "موجودہ سمت:" },
                    
                    // Status Messages (Urdu translations)
                    { "SettingsSaved", "ترتیبات کامیابی سے محفوظ ہوگئیں" },
                    { "LanguageChanged", "زبان کامیابی سے تبدیل ہوگئی" },
                    { "ThemeChanged", "تھیم کامیابی سے تبدیل ہوگیا" },
                    { "ColorChanged", "رنگ کی اسکیم کامیابی سے تبدیل ہوگئی" },
                    { "DirectionChanged", "لے آؤٹ کی سمت کامیابی سے تبدیل ہوگئی" },
                    
                    // Application Info (Urdu translations)
                    { "ApplicationName", "کرونو پوز" },
                    { "ApplicationDescription", "فروخت کا نظام" },
                    { "Version", "ورژن" },
                    { "BuildDate", "بلڈ کی تاریخ" }
                }
            }
        };
    }

    private void UpdateApplicationResources()
    {
        var resources = System.Windows.Application.Current?.Resources;
        if (resources == null) return;
        
        var currentDict = _localizations[_currentLanguage];
        
        foreach (var kvp in currentDict)
        {
            resources[$"Loc_{kvp.Key}"] = kvp.Value;
        }
    }

    private void SaveLanguageToSettings(SupportedLanguage language)
    {
        try
        {
            Properties.Settings.Default.Language = language.ToString();
            Properties.Settings.Default.Save();
        }
        catch
        {
            // Ignore save errors
        }
    }
}
