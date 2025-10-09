using System;
using System.Globalization;
using System.Windows.Data;

namespace ChronoPos.Desktop.Converters;

public class StatusToIsActiveConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.Equals("Active", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? "Active" : "Inactive";
        }
        return "Inactive";
    }
}
