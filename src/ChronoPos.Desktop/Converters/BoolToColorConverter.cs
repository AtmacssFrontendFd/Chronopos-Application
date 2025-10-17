using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChronoPos.Desktop.Converters
{
    /// <summary>
    /// Converts boolean values to colors (true = green, false = gray) or text (true = "Yes", false = "No")
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If parameter is "Text", return text instead of color
                if (parameter?.ToString() == "Text")
                {
                    return boolValue ? "Yes" : "No";
                }
                
                // Otherwise return color brush
                return boolValue ? new SolidColorBrush(Color.FromRgb(40, 167, 69)) : // Green for primary
                                  new SolidColorBrush(Color.FromRgb(224, 224, 224)); // Light gray for non-primary
            }
            
            // Default
            if (parameter?.ToString() == "Text")
            {
                return "No";
            }
            return new SolidColorBrush(Color.FromRgb(224, 224, 224)); // Default gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
