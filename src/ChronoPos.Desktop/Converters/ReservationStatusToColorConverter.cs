using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChronoPos.Desktop.Converters;

/// <summary>
/// Converts reservation status to color brush for visual display
/// </summary>
public class ReservationStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = value?.ToString()?.ToLower();

        return status switch
        {
            "confirmed" => new SolidColorBrush(Color.FromRgb(255, 215, 0)),      // Yellow (#FFD700)
            "checked_in" => new SolidColorBrush(Color.FromRgb(255, 215, 0)),     // Yellow (#FFD700)
            "waiting" => new SolidColorBrush(Color.FromRgb(204, 204, 204)),      // Grey (#CCCCCC)
            "cancelled" => new SolidColorBrush(Color.FromRgb(255, 107, 107)),    // Red (#FF6B6B)
            "completed" => new SolidColorBrush(Color.FromRgb(78, 205, 196)),     // Green/Teal (#4ECDC4)
            _ => Brushes.LightGray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack is not supported for ReservationStatusToColorConverter");
    }
}

/// <summary>
/// Converts reservation status to text color (for contrast)
/// </summary>
public class ReservationStatusToTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = value?.ToString()?.ToLower();

        return status switch
        {
            "confirmed" => Brushes.Black,       // Black text on yellow
            "checked_in" => Brushes.Black,      // Black text on yellow
            "waiting" => Brushes.Black,         // Black text on grey
            "cancelled" => Brushes.White,       // White text on red
            "completed" => Brushes.White,       // White text on teal
            _ => Brushes.Black
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack is not supported for ReservationStatusToTextColorConverter");
    }
}

/// <summary>
/// Converts reservation status to badge text
/// </summary>
public class ReservationStatusToBadgeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = value?.ToString()?.ToLower();

        return status switch
        {
            "confirmed" => "✓ Confirmed",
            "checked_in" => "✓ Checked In",
            "waiting" => "⏱ Waiting",
            "cancelled" => "✗ Cancelled",
            "completed" => "✓ Completed",
            _ => status ?? "Unknown"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ConvertBack is not supported for ReservationStatusToBadgeConverter");
    }
}
