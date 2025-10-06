using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChronoPos.Desktop.Converters;

/// <summary>
/// Converter to map module names to colors
/// </summary>
public class ModuleToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string module)
            return new SolidColorBrush(Colors.Gray);

        return module.ToLowerInvariant() switch
        {
            "products" => new SolidColorBrush(Color.FromRgb(52, 152, 219)),   // Blue
            "customers" => new SolidColorBrush(Color.FromRgb(46, 204, 113)),  // Green
            "sales" => new SolidColorBrush(Color.FromRgb(155, 89, 182)),      // Purple
            "stock" => new SolidColorBrush(Color.FromRgb(241, 196, 15)),      // Yellow
            "brands" => new SolidColorBrush(Color.FromRgb(230, 126, 34)),     // Orange
            "categories" => new SolidColorBrush(Color.FromRgb(231, 76, 60)),  // Red
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to map module names to icons
/// </summary>
public class ModuleToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string module)
            return "📦";

        return module.ToLowerInvariant() switch
        {
            "products" => "📦",
            "customers" => "👥",
            "sales" => "💳",
            "stock" => "📊",
            "brands" => "🏷️",
            "categories" => "📁",
            _ => "📄"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to check if a value is null and return appropriate visibility
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}