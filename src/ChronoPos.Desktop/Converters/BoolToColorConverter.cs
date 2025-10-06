using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChronoPos.Desktop.Converters
{
    /// <summary>
    /// Converts boolean values to colors (true = green, false = gray)
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? new SolidColorBrush(Color.FromRgb(40, 167, 69)) : // Green for primary
                                  new SolidColorBrush(Color.FromRgb(224, 224, 224)); // Light gray for non-primary
            }
            
            return new SolidColorBrush(Color.FromRgb(224, 224, 224)); // Default gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
