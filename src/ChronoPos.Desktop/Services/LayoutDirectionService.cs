using System.Windows;
using System.Windows.Controls;

namespace ChronoPos.Desktop.Services;

/// <summary>
/// Service for managing application layout direction (RTL/LTR)
/// </summary>
public class LayoutDirectionService : ILayoutDirectionService
{
    private LayoutDirection _currentDirection = LayoutDirection.LeftToRight;
    
    public LayoutDirection CurrentDirection => _currentDirection;
    
    public event Action<LayoutDirection>? DirectionChanged;

    public void ChangeDirection(LayoutDirection direction)
    {
        if (_currentDirection == direction)
            return;

        _currentDirection = direction;
        
        // Update application flow direction
        UpdateApplicationDirection(direction);
        
        // Save direction preference
        SaveDirectionToSettings(direction);
        
        // Notify about direction change
        DirectionChanged?.Invoke(direction);
    }

    public void LoadDirectionFromSettings()
    {
        try
        {
            var savedDirection = Properties.Settings.Default.LayoutDirection;
            if (Enum.TryParse<LayoutDirection>(savedDirection, out var direction))
            {
                ChangeDirection(direction);
            }
            else
            {
                ChangeDirection(LayoutDirection.LeftToRight);
            }
        }
        catch
        {
            ChangeDirection(LayoutDirection.LeftToRight);
        }
    }

    public List<KeyValuePair<LayoutDirection, string>> GetAvailableDirections()
    {
        return new List<KeyValuePair<LayoutDirection, string>>
        {
            new(LayoutDirection.LeftToRight, "Left to Right (LTR)"),
            new(LayoutDirection.RightToLeft, "Right to Left (RTL)")
        };
    }

    private void UpdateApplicationDirection(LayoutDirection direction)
    {
        var flowDirection = direction == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft 
            : FlowDirection.LeftToRight;

        // Update main window flow direction
        if (System.Windows.Application.Current?.MainWindow != null)
        {
            System.Windows.Application.Current.MainWindow.FlowDirection = flowDirection;
        }

        // Update application resources
        System.Windows.Application.Current?.Resources?.Remove("AppFlowDirection");
        System.Windows.Application.Current?.Resources?.Add("AppFlowDirection", flowDirection);
        
        // Update text alignment resources
        var textAlignment = direction == LayoutDirection.RightToLeft 
            ? TextAlignment.Right 
            : TextAlignment.Left;
        
        System.Windows.Application.Current?.Resources?.Remove("AppTextAlignment");
        System.Windows.Application.Current?.Resources?.Add("AppTextAlignment", textAlignment);
        
        // Update horizontal alignment for RTL support
        var horizontalAlignment = direction == LayoutDirection.RightToLeft 
            ? HorizontalAlignment.Right 
            : HorizontalAlignment.Left;
            
        System.Windows.Application.Current?.Resources?.Remove("AppHorizontalAlignment");
        System.Windows.Application.Current?.Resources?.Add("AppHorizontalAlignment", horizontalAlignment);
    }

    private void SaveDirectionToSettings(LayoutDirection direction)
    {
        try
        {
            Properties.Settings.Default.LayoutDirection = direction.ToString();
            Properties.Settings.Default.Save();
        }
        catch
        {
            // Ignore save errors
        }
    }
}
