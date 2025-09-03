using System;
using System.Collections.Generic;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Zoom levels available in the application
/// </summary>
public enum ZoomLevel
{
    Zoom50 = 50,
    Zoom60 = 60,
    Zoom70 = 70,
    Zoom80 = 80,
    Zoom90 = 90,
    Zoom100 = 100,  // Default
    Zoom110 = 110,
    Zoom120 = 120,
    Zoom130 = 130,
    Zoom140 = 140,
    Zoom150 = 150
}

/// <summary>
/// Service interface for managing application zoom functionality
/// </summary>
public interface IZoomService
{
    /// <summary>
    /// Gets the current zoom level
    /// </summary>
    ZoomLevel CurrentZoomLevel { get; }

    /// <summary>
    /// Gets the current zoom percentage (50-150)
    /// </summary>
    int CurrentZoomPercentage { get; }

    /// <summary>
    /// Gets the current zoom scale factor (0.5-1.5)
    /// </summary>
    double CurrentZoomScale { get; }

    /// <summary>
    /// Gets all available zoom levels
    /// </summary>
    /// <returns>List of available zoom levels with display names</returns>
    List<KeyValuePair<ZoomLevel, string>> GetAvailableZoomLevels();

    /// <summary>
    /// Changes the zoom level
    /// </summary>
    /// <param name="zoomLevel">The new zoom level to apply</param>
    void ChangeZoomLevel(ZoomLevel zoomLevel);

    /// <summary>
    /// Changes the zoom level by percentage
    /// </summary>
    /// <param name="percentage">The zoom percentage (50-150)</param>
    void ChangeZoomLevel(int percentage);

    /// <summary>
    /// Increases zoom level by one step
    /// </summary>
    void ZoomIn();

    /// <summary>
    /// Decreases zoom level by one step
    /// </summary>
    void ZoomOut();

    /// <summary>
    /// Resets zoom to 100%
    /// </summary>
    void ResetZoom();

    /// <summary>
    /// Loads zoom level from settings
    /// </summary>
    void LoadZoomFromSettings();

    /// <summary>
    /// Saves current zoom level to settings
    /// </summary>
    void SaveZoomToSettings();

    /// <summary>
    /// Event raised when zoom level changes
    /// </summary>
    event Action<ZoomLevel> ZoomChanged;
}
