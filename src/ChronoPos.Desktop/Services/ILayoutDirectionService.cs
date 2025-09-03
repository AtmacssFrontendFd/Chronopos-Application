using System.Windows;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Layout direction options
/// </summary>
public enum LayoutDirection
{
    LeftToRight,
    RightToLeft
}

/// <summary>
/// Interface for managing application layout direction (RTL/LTR)
/// </summary>
public interface ILayoutDirectionService
{
    /// <summary>
    /// Gets the current layout direction
    /// </summary>
    LayoutDirection CurrentDirection { get; }
    
    /// <summary>
    /// Event raised when layout direction changes
    /// </summary>
    event Action<LayoutDirection>? DirectionChanged;
    
    /// <summary>
    /// Changes the layout direction
    /// </summary>
    /// <param name="direction">The new layout direction</param>
    void ChangeDirection(LayoutDirection direction);
    
    /// <summary>
    /// Loads layout direction from application settings
    /// </summary>
    void LoadDirectionFromSettings();
    
    /// <summary>
    /// Gets all available layout directions
    /// </summary>
    List<KeyValuePair<LayoutDirection, string>> GetAvailableDirections();
}
