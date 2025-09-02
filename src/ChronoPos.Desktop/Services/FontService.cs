using System.Windows;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Service for managing application fonts
/// </summary>
public class FontService : IFontService
{
    private FontSize _currentFontSize = FontSize.Medium;

    public FontSize CurrentFontSize => _currentFontSize;

    public event Action<FontSize>? FontChanged;

    public void ChangeFontSize(FontSize fontSize)
    {
        if (_currentFontSize == fontSize)
            return;

        _currentFontSize = fontSize;

        // Update font size resources
        var fontSizes = GetFontSizeValues(fontSize);
        
        foreach (var fontSizeResource in fontSizes)
        {
            System.Windows.Application.Current.Resources[fontSizeResource.Key] = fontSizeResource.Value;
        }

        // Save font size preference
        SaveFontSizeToSettings(fontSize);

        // Notify about font size change
        FontChanged?.Invoke(fontSize);
    }

    public void LoadFontFromSettings()
    {
        try
        {
            var savedFontSize = Properties.Settings.Default.FontSize;
            if (Enum.TryParse<FontSize>(savedFontSize, out var fontSize))
            {
                ChangeFontSize(fontSize);
            }
            else
            {
                // Default to medium font size if no valid setting found
                ChangeFontSize(FontSize.Medium);
            }
        }
        catch
        {
            // If there's any error, default to medium font size
            ChangeFontSize(FontSize.Medium);
        }
    }

    private Dictionary<string, double> GetFontSizeValues(FontSize fontSize)
    {
        return fontSize switch
        {
            FontSize.VerySmall => new Dictionary<string, double>
            {
                ["FontSizeVerySmall"] = 8,
                ["FontSizeSmall"] = 10,
                ["FontSizeMedium"] = 12,
                ["FontSizeLarge"] = 14
            },
            FontSize.Small => new Dictionary<string, double>
            {
                ["FontSizeVerySmall"] = 10,
                ["FontSizeSmall"] = 12,
                ["FontSizeMedium"] = 14,
                ["FontSizeLarge"] = 16
            },
            FontSize.Medium => new Dictionary<string, double>
            {
                ["FontSizeVerySmall"] = 12,
                ["FontSizeSmall"] = 14,
                ["FontSizeMedium"] = 16,
                ["FontSizeLarge"] = 18
            },
            FontSize.Large => new Dictionary<string, double>
            {
                ["FontSizeVerySmall"] = 14,
                ["FontSizeSmall"] = 16,
                ["FontSizeMedium"] = 18,
                ["FontSizeLarge"] = 20
            },
            _ => new Dictionary<string, double>
            {
                ["FontSizeVerySmall"] = 12,
                ["FontSizeSmall"] = 14,
                ["FontSizeMedium"] = 16,
                ["FontSizeLarge"] = 18
            }
        };
    }

    private void SaveFontSizeToSettings(FontSize fontSize)
    {
        try
        {
            Properties.Settings.Default.FontSize = fontSize.ToString();
            Properties.Settings.Default.Save();
        }
        catch
        {
            // Ignore save errors
        }
    }
}
