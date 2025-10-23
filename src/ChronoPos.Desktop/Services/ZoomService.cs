using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Service for managing application zoom functionality
/// </summary>
public class ZoomService : IZoomService
{
    private ZoomLevel _currentZoomLevel = ZoomLevel.Zoom100;

    /// <summary>
    /// Event raised when zoom level changes
    /// </summary>
    public event Action<ZoomLevel>? ZoomChanged;

    /// <summary>
    /// Gets the current zoom level
    /// </summary>
    public ZoomLevel CurrentZoomLevel => _currentZoomLevel;

    /// <summary>
    /// Gets the current zoom percentage (50-150)
    /// </summary>
    public int CurrentZoomPercentage => (int)_currentZoomLevel;

    /// <summary>
    /// Gets the current zoom scale factor (0.5-1.5)
    /// </summary>
    public double CurrentZoomScale => (int)_currentZoomLevel / 100.0;

    /// <summary>
    /// Gets all available zoom levels with display names
    /// </summary>
    /// <returns>List of available zoom levels with display names</returns>
    public List<KeyValuePair<ZoomLevel, string>> GetAvailableZoomLevels()
    {
        return new List<KeyValuePair<ZoomLevel, string>>
        {
            new(ZoomLevel.Zoom50, "50% - Very Small"),
            new(ZoomLevel.Zoom60, "60% - Small"),
            new(ZoomLevel.Zoom70, "70% - Smaller"),
            new(ZoomLevel.Zoom80, "80% - Small Normal"),
            new(ZoomLevel.Zoom90, "90% - Almost Normal"),
            new(ZoomLevel.Zoom100, "100% - Normal (Default)"),
            new(ZoomLevel.Zoom110, "110% - Slightly Large"),
            new(ZoomLevel.Zoom120, "120% - Large"),
            new(ZoomLevel.Zoom130, "130% - Larger"),
            new(ZoomLevel.Zoom140, "140% - Very Large"),
            new(ZoomLevel.Zoom150, "150% - Maximum")
        };
    }

    /// <summary>
    /// Changes the zoom level
    /// </summary>
    /// <param name="zoomLevel">The new zoom level to apply</param>
    public void ChangeZoomLevel(ZoomLevel zoomLevel)
    {
        var oldLevel = _currentZoomLevel;
        if (_currentZoomLevel != zoomLevel)
        {
            _currentZoomLevel = zoomLevel;
            
            Console.WriteLine($"Zoom level changing from {oldLevel} to {_currentZoomLevel} ({CurrentZoomPercentage}%)");
            
            // Apply zoom and refresh UI
            RefreshApplicationUI();
            
            // Save to settings
            SaveZoomToSettings();
            
            // Raise the event
            ZoomChanged?.Invoke(zoomLevel);
            
            Console.WriteLine($"Zoom level successfully changed to {_currentZoomLevel} ({CurrentZoomPercentage}%)");
        }
    }

    /// <summary>
    /// Changes the zoom level by percentage
    /// </summary>
    /// <param name="percentage">The zoom percentage (50-150)</param>
    public void ChangeZoomLevel(int percentage)
    {
        // Find the closest valid zoom level
        var validPercentages = Enum.GetValues<ZoomLevel>().Cast<int>().ToList();
        var closestPercentage = validPercentages.OrderBy(p => Math.Abs(p - percentage)).First();
        
        if (Enum.IsDefined(typeof(ZoomLevel), closestPercentage))
        {
            ChangeZoomLevel((ZoomLevel)closestPercentage);
        }
    }

    /// <summary>
    /// Increases zoom level by one step
    /// </summary>
    public void ZoomIn()
    {
        var currentIndex = GetCurrentZoomIndex();
        var zoomLevels = Enum.GetValues<ZoomLevel>().ToList();
        
        if (currentIndex < zoomLevels.Count - 1)
        {
            ChangeZoomLevel(zoomLevels[currentIndex + 1]);
        }
    }

    /// <summary>
    /// Decreases zoom level by one step
    /// </summary>
    public void ZoomOut()
    {
        var currentIndex = GetCurrentZoomIndex();
        
        if (currentIndex > 0)
        {
            var zoomLevels = Enum.GetValues<ZoomLevel>().ToList();
            ChangeZoomLevel(zoomLevels[currentIndex - 1]);
        }
    }

    /// <summary>
    /// Resets zoom to 100%
    /// </summary>
    public void ResetZoom()
    {
        ChangeZoomLevel(ZoomLevel.Zoom100);
    }

    /// <summary>
    /// Loads zoom level from settings
    /// </summary>
    public void LoadZoomFromSettings()
    {
        try
        {
            var savedZoom = Properties.Settings.Default.ZoomLevel;
            if (savedZoom >= 50 && savedZoom <= 150)
            {
                ChangeZoomLevel(savedZoom);
            }
            else
            {
                // Reset to default if invalid value
                ChangeZoomLevel(ZoomLevel.Zoom100);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading zoom from settings: {ex.Message}");
            ChangeZoomLevel(ZoomLevel.Zoom100);
        }
    }

    /// <summary>
    /// Saves current zoom level to settings
    /// </summary>
    public void SaveZoomToSettings()
    {
        try
        {
            Properties.Settings.Default.ZoomLevel = CurrentZoomPercentage;
            Properties.Settings.Default.Save();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving zoom to settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the index of the current zoom level in the enum
    /// </summary>
    /// <returns>Index of current zoom level</returns>
    private int GetCurrentZoomIndex()
    {
        var zoomLevels = Enum.GetValues<ZoomLevel>().ToList();
        return zoomLevels.IndexOf(_currentZoomLevel);
    }

    /// <summary>
    /// Applies the current zoom level to the application UI
    /// </summary>
    private void ApplyZoomToApplication()
    {
        try
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Update zoom-related resources that will be used throughout the application
                var resources = System.Windows.Application.Current.Resources;
                
                // Set zoom scale factor
                resources["CurrentZoomScale"] = CurrentZoomScale;
                resources["CurrentZoomPercentage"] = CurrentZoomPercentage;
                resources["CurrentZoomLevel"] = _currentZoomLevel.ToString();

                // Update font sizes based on zoom
                UpdateFontSizesWithZoom(resources);
                
                // Update spacing and sizing based on zoom
                UpdateLayoutResourcesWithZoom(resources);

                Console.WriteLine($"Zoom applied: {CurrentZoomPercentage}% (Scale: {CurrentZoomScale})");
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying zoom to application: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates font sizes based on current zoom level
    /// </summary>
    private void UpdateFontSizesWithZoom(System.Windows.ResourceDictionary resources)
    {
        // Base font sizes (updated to match theme files)
        var baseFontSizes = new Dictionary<string, double>
        {
            ["FontSizeVerySmall"] = 13,
            ["FontSizeSmall"] = 15,
            ["FontSizeMedium"] = 17,
            ["FontSizeLarge"] = 19,
            ["FontSizeXLarge"] = 22,
            ["FontSizeXXLarge"] = 26
        };

        // Apply zoom to font sizes
        foreach (var fontSize in baseFontSizes)
        {
            var zoomedSize = fontSize.Value * CurrentZoomScale;
            resources[fontSize.Key] = zoomedSize;
        }
    }

    /// <summary>
    /// Updates layout resources based on current zoom level
    /// </summary>
    private void UpdateLayoutResourcesWithZoom(System.Windows.ResourceDictionary resources)
    {
        // Base spacing and sizing values (Double values) - updated to match theme files
        var baseValues = new Dictionary<string, double>
        {
            ["DefaultMargin"] = 10 * CurrentZoomScale,
            ["DefaultPadding"] = 15 * CurrentZoomScale,
            ["ButtonHeight"] = 45 * CurrentZoomScale,
            ["ButtonMinWidth"] = 80 * CurrentZoomScale,
            ["CardPadding"] = 20 * CurrentZoomScale,
            ["BorderRadius"] = 8 * CurrentZoomScale,
            ["IconSize"] = 16 * CurrentZoomScale
        };

        // Apply zoom to layout values (these are Double resources)
        foreach (var value in baseValues)
        {
            resources[value.Key] = value.Value;
        }

        // Create Thickness values for properties that need Thickness objects
        var defaultMarginThickness = new System.Windows.Thickness(10 * CurrentZoomScale);
        var defaultPaddingThickness = new System.Windows.Thickness(15 * CurrentZoomScale);
        var cardPaddingThickness = new System.Windows.Thickness(20 * CurrentZoomScale);
        
        resources["DefaultMarginThickness"] = defaultMarginThickness;
        resources["DefaultPaddingThickness"] = defaultPaddingThickness;
        resources["CardPaddingThickness"] = cardPaddingThickness;
        
        // Create CornerRadius values for properties that need CornerRadius objects
        var borderCornerRadius = new System.Windows.CornerRadius(8 * CurrentZoomScale);
        var cardCornerRadius = new System.Windows.CornerRadius(8 * CurrentZoomScale);
        
        resources["BorderCornerRadius"] = borderCornerRadius;
        resources["CardCornerRadius"] = cardCornerRadius;
    }

    /// <summary>
    /// Forces a refresh of the application UI by updating all bound resources
    /// </summary>
    public void RefreshApplicationUI()
    {
        try
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Force re-evaluation of dynamic resources by clearing and re-applying them
                ApplyZoomToApplication();
                
                // Force update of main window if available
                var mainWindow = System.Windows.Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.InvalidateVisual();
                    mainWindow.UpdateLayout();
                }
                
                Console.WriteLine($"UI refresh completed for zoom level {CurrentZoomPercentage}%");
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing application UI: {ex.Message}");
        }
    }
}
