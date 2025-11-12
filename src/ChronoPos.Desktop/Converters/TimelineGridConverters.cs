using System.Globalization;
using System.Windows.Data;

namespace ChronoPos.Desktop.Converters;

/// <summary>
/// Converts column index to left position in pixels for timeline grid
/// </summary>
public class ColumnIndexToLeftConverter : IValueConverter
{
    private const double ColumnWidth = 100; // Match time slot width

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int columnIndex)
        {
            return columnIndex * ColumnWidth;
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts column span to width in pixels for reservation blocks
/// </summary>
public class ColumnSpanToWidthConverter : IValueConverter
{
    private const double ColumnWidth = 100; // Match time slot width

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int columnSpan)
        {
            return columnSpan * ColumnWidth - 4; // Subtract margin
        }
        return ColumnWidth - 4;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts equality check for radio button binding
/// </summary>
public class EqualityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == parameter?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked)
        {
            return parameter;
        }
        return Binding.DoNothing;
    }
}
