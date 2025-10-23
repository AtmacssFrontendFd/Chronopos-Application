using System.Windows;
using System.Windows.Media;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Service for managing application color schemes
/// </summary>
public class ColorSchemeService : IColorSchemeService
{
    private ColorOption _currentPrimaryColor;
    private ColorOption _currentBackgroundColor;

    public ColorOption CurrentPrimaryColor => _currentPrimaryColor;
    public ColorOption CurrentBackgroundColor => _currentBackgroundColor;
    
    public event Action<ColorOption>? PrimaryColorChanged;
    public event Action<ColorOption>? BackgroundColorChanged;

    public ColorSchemeService()
    {
        // Set default colors
        _currentPrimaryColor = PrimaryColors.GetAll().First(c => c.Name == "Golden");
        _currentBackgroundColor = BackgroundColors.GetAll().First(c => c.Name == "Default");
    }

    public void ChangePrimaryColor(ColorOption colorOption)
    {
        if (_currentPrimaryColor.Name == colorOption.Name)
            return;

        _currentPrimaryColor = colorOption;
        
        // Update application resources
        UpdatePrimaryColorResources(colorOption);
        
        // Save color preference
        SavePrimaryColorToSettings(colorOption);
        
        // Notify about color change
        PrimaryColorChanged?.Invoke(colorOption);
    }

    public void ChangeBackgroundColor(ColorOption colorOption)
    {
        if (_currentBackgroundColor.Name == colorOption.Name)
            return;

        _currentBackgroundColor = colorOption;
        
        // Update application resources
        UpdateBackgroundColorResources(colorOption);
        
        // Save color preference
        SaveBackgroundColorToSettings(colorOption);
        
        // Notify about color change
        BackgroundColorChanged?.Invoke(colorOption);
    }

    public void LoadColorsFromSettings()
    {
        try
        {
            // Load primary color
            var savedPrimaryColor = Properties.Settings.Default.PrimaryColor;
            var primaryColor = PrimaryColors.GetAll().FirstOrDefault(c => c.Name == savedPrimaryColor);
            if (primaryColor != null)
            {
                ChangePrimaryColor(primaryColor);
            }

            // Load background color
            var savedBackgroundColor = Properties.Settings.Default.BackgroundColor;
            var backgroundColor = BackgroundColors.GetAll().FirstOrDefault(c => c.Name == savedBackgroundColor);
            if (backgroundColor != null)
            {
                ChangeBackgroundColor(backgroundColor);
            }
        }
        catch
        {
            // Use defaults if loading fails
        }
    }

    public List<ColorOption> GetAvailablePrimaryColors()
    {
        return PrimaryColors.GetAll();
    }

    public List<ColorOption> GetAvailableBackgroundColors()
    {
        return BackgroundColors.GetAll();
    }

    private void UpdatePrimaryColorResources(ColorOption colorOption)
    {
        var resources = System.Windows.Application.Current?.Resources;
        if (resources == null) return;
        
        var color = colorOption.Color;
        
        // Update primary color
        resources["Primary"] = new SolidColorBrush(color);
        
        // Generate lighter variant for hover states
        var lighterColor = Color.FromRgb(
            (byte)Math.Min(255, color.R + 20),
            (byte)Math.Min(255, color.G + 20),
            (byte)Math.Min(255, color.B + 20)
        );
        resources["PrimaryAlt"] = new SolidColorBrush(lighterColor);
        
        // Generate darker variant for pressed states
        var darkerColor = Color.FromRgb(
            (byte)Math.Max(0, color.R - 20),
            (byte)Math.Max(0, color.G - 20),
            (byte)Math.Max(0, color.B - 20)
        );
        resources["PrimaryDark"] = new SolidColorBrush(darkerColor);
    }

    private void UpdateBackgroundColorResources(ColorOption colorOption)
    {
        var resources = System.Windows.Application.Current?.Resources;
        if (resources == null) return;
        
        var color = colorOption.Color;
        
        // Update main background color for light theme
        if (resources.MergedDictionaries.Count > 0)
        {
            foreach (var dictionary in resources.MergedDictionaries)
            {
                if (dictionary.Source?.ToString().Contains("LightTheme") == true)
                {
                    dictionary["MainBackground"] = new SolidColorBrush(color);
                    
                    // Generate slightly darker variant for cards
                    var cardColor = Color.FromRgb(
                        (byte)Math.Max(0, color.R - 10),
                        (byte)Math.Max(0, color.G - 10),
                        (byte)Math.Max(0, color.B - 10)
                    );
                    dictionary["CardBackground"] = new SolidColorBrush(cardColor);
                    break;
                }
            }
        }
    }

    private void SavePrimaryColorToSettings(ColorOption colorOption)
    {
        try
        {
            Properties.Settings.Default.PrimaryColor = colorOption.Name;
            Properties.Settings.Default.Save();
        }
        catch
        {
            // Ignore save errors
        }
    }

    private void SaveBackgroundColorToSettings(ColorOption colorOption)
    {
        try
        {
            Properties.Settings.Default.BackgroundColor = colorOption.Name;
            Properties.Settings.Default.Save();
        }
        catch
        {
            // Ignore save errors
        }
    }
}
