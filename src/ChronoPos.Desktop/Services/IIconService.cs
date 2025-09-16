using System.Windows;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Service for providing application icons using Iconify icon system
/// </summary>
public interface IIconService
{
    /// <summary>
    /// Get icon geometry data for the specified icon name
    /// </summary>
    /// <param name="iconName">Name of the icon (e.g., "home", "refresh", "add", "back")</param>
    /// <returns>Geometry data for the icon</returns>
    string GetIconGeometry(string iconName);
    
    /// <summary>
    /// Get icon resource key for use in XAML
    /// </summary>
    /// <param name="iconName">Name of the icon</param>
    /// <returns>Resource key for the icon geometry</returns>
    string GetIconResourceKey(string iconName);
    
    /// <summary>
    /// Register icon geometries with application resources
    /// </summary>
    void RegisterIconResources();
}