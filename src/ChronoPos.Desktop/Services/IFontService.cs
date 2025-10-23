namespace ChronoPos.Desktop.Services;

/// <summary>
/// Font size options available in the application
/// </summary>
public enum FontSize
{
    VerySmall,
    Small,
    Medium,
    Large
}

/// <summary>
/// Interface for managing application font settings
/// </summary>
public interface IFontService
{
    /// <summary>
    /// Gets the current font size
    /// </summary>
    FontSize CurrentFontSize { get; }
    
    /// <summary>
    /// Event raised when the font size changes
    /// </summary>
    event Action<FontSize>? FontChanged;
    
    /// <summary>
    /// Changes the application font size
    /// </summary>
    /// <param name="fontSize">The new font size to apply</param>
    void ChangeFontSize(FontSize fontSize);
    
    /// <summary>
    /// Loads the font size from application settings
    /// </summary>
    void LoadFontFromSettings();
}
