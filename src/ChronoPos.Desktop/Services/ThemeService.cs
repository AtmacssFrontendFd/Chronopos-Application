using System.Windows;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Enumeration for available themes
/// </summary>
public enum Theme
{
    Light,
    Dark
}

/// <summary>
/// Interface for theme management service
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the current theme
    /// </summary>
    Theme CurrentTheme { get; }

    /// <summary>
    /// Changes the application theme
    /// </summary>
    /// <param name="theme">The theme to apply</param>
    void ChangeTheme(Theme theme);

    /// <summary>
    /// Loads the theme from settings
    /// </summary>
    void LoadThemeFromSettings();

    /// <summary>
    /// Event fired when theme changes
    /// </summary>
    event Action<Theme> ThemeChanged;
}

/// <summary>
/// Service for managing application themes
/// </summary>
public class ThemeService : IThemeService
{
    private Theme _currentTheme = Theme.Light;
    private const string THEME_SETTINGS_KEY = "AppTheme";

    public Theme CurrentTheme => _currentTheme;

    public event Action<Theme>? ThemeChanged;

    public void ChangeTheme(Theme theme)
    {
        if (_currentTheme == theme)
            return;

        _currentTheme = theme;

        // Remove existing theme resource dictionaries
        var existingTheme = System.Windows.Application.Current.Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString?.Contains("Themes/") == true);

        if (existingTheme != null)
        {
            System.Windows.Application.Current.Resources.MergedDictionaries.Remove(existingTheme);
        }

        // Add new theme resource dictionary
        var themeUri = theme switch
        {
            Theme.Dark => new Uri("pack://application:,,,/ChronoPos.Desktop;component/Themes/DarkTheme.xaml"),
            Theme.Light => new Uri("pack://application:,,,/ChronoPos.Desktop;component/Themes/LightTheme.xaml"),
            _ => new Uri("pack://application:,,,/ChronoPos.Desktop;component/Themes/LightTheme.xaml")
        };

        var newTheme = new ResourceDictionary { Source = themeUri };
        System.Windows.Application.Current.Resources.MergedDictionaries.Insert(0, newTheme);

        // Save theme preference
        SaveThemeToSettings(theme);

        // Notify about theme change
        ThemeChanged?.Invoke(theme);
    }

    public void LoadThemeFromSettings()
    {
        try
        {
            var savedTheme = Properties.Settings.Default.Theme;
            if (Enum.TryParse<Theme>(savedTheme, out var theme))
            {
                ChangeTheme(theme);
            }
            else
            {
                // Default to light theme if no valid setting found
                ChangeTheme(Theme.Light);
            }
        }
        catch
        {
            // If there's any error, default to light theme
            ChangeTheme(Theme.Light);
        }
    }

    private void SaveThemeToSettings(Theme theme)
    {
        try
        {
            Properties.Settings.Default.Theme = theme.ToString();
            Properties.Settings.Default.Save();
        }
        catch
        {
            // Ignore save errors
        }
    }
}
