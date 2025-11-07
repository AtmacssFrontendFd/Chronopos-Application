using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChronoPos.Desktop.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status.ToLower() switch
                {
                    "active" => new SolidColorBrush(Color.FromRgb(34, 197, 94)),    // Green
                    "posted" => new SolidColorBrush(Color.FromRgb(34, 197, 94)),    // Green
                    "settled" => new SolidColorBrush(Color.FromRgb(34, 197, 94)),   // Green
                    "billed" => new SolidColorBrush(Color.FromRgb(59, 130, 246)),   // Blue
                    "inactive" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),  // Red
                    "cancelled" => new SolidColorBrush(Color.FromRgb(239, 68, 68)), // Red
                    "voided" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),    // Red
                    "pending" => new SolidColorBrush(Color.FromRgb(245, 158, 11)),  // Orange
                    "refunded" => new SolidColorBrush(Color.FromRgb(245, 158, 11)), // Orange
                    "pendingpayment" => new SolidColorBrush(Color.FromRgb(245, 158, 11)), // Orange
                    "partialpayment" => new SolidColorBrush(Color.FromRgb(251, 191, 36)), // Yellow
                    "hold" => new SolidColorBrush(Color.FromRgb(156, 163, 175)),    // Gray-400
                    "draft" => new SolidColorBrush(Color.FromRgb(107, 114, 128)),   // Gray
                    _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))          // Default Gray
                };
            }
            return new SolidColorBrush(Color.FromRgb(107, 114, 128)); // Default Gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}