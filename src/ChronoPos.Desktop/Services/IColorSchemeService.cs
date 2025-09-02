using System.Windows.Media;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Represents a color scheme option
/// </summary>
public class ColorOption
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string HexValue { get; set; } = string.Empty;
    public Color Color => (Color)ColorConverter.ConvertFromString(HexValue);
}

/// <summary>
/// Available primary color schemes
/// </summary>
public static class PrimaryColors
{
    public static List<ColorOption> GetAll() => new()
    {
        new ColorOption { Name = "Golden", DisplayName = "Golden", HexValue = "#FFD700" },
        new ColorOption { Name = "Blue", DisplayName = "Blue", HexValue = "#2196F3" },
        new ColorOption { Name = "Green", DisplayName = "Green", HexValue = "#4CAF50" },
        new ColorOption { Name = "Purple", DisplayName = "Purple", HexValue = "#9C27B0" }
    };
}

/// <summary>
/// Available background color schemes
/// </summary>
public static class BackgroundColors
{
    public static List<ColorOption> GetAll() => new()
    {
        new ColorOption { Name = "Default", DisplayName = "Default", HexValue = "#FFFFFF" },
        new ColorOption { Name = "Warm", DisplayName = "Warm White", HexValue = "#FFF8F0" },
        new ColorOption { Name = "Cool", DisplayName = "Cool White", HexValue = "#F0F8FF" },
        new ColorOption { Name = "Neutral", DisplayName = "Neutral Gray", HexValue = "#F5F5F5" }
    };
}

/// <summary>
/// Interface for managing application color schemes
/// </summary>
public interface IColorSchemeService
{
    /// <summary>
    /// Gets the current primary color
    /// </summary>
    ColorOption CurrentPrimaryColor { get; }
    
    /// <summary>
    /// Gets the current background color
    /// </summary>
    ColorOption CurrentBackgroundColor { get; }
    
    /// <summary>
    /// Event raised when primary color changes
    /// </summary>
    event Action<ColorOption>? PrimaryColorChanged;
    
    /// <summary>
    /// Event raised when background color changes
    /// </summary>
    event Action<ColorOption>? BackgroundColorChanged;
    
    /// <summary>
    /// Changes the primary color scheme
    /// </summary>
    /// <param name="colorOption">The new primary color</param>
    void ChangePrimaryColor(ColorOption colorOption);
    
    /// <summary>
    /// Changes the background color scheme
    /// </summary>
    /// <param name="colorOption">The new background color</param>
    void ChangeBackgroundColor(ColorOption colorOption);
    
    /// <summary>
    /// Loads color schemes from application settings
    /// </summary>
    void LoadColorsFromSettings();
    
    /// <summary>
    /// Gets all available primary colors
    /// </summary>
    List<ColorOption> GetAvailablePrimaryColors();
    
    /// <summary>
    /// Gets all available background colors
    /// </summary>
    List<ColorOption> GetAvailableBackgroundColors();
}
