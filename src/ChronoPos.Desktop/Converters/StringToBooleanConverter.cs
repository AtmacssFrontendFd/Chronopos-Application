using System;
using System.Globalization;
using System.Windows.Data;

namespace ChronoPos.Desktop.Converters;

public class StringToBooleanConverter : IValueConverter
{
    public string TrueValue { get; set; } = "Active";
    public string FalseValue { get; set; } = "Inactive";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return stringValue == TrueValue;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? TrueValue : FalseValue;
        }
        return FalseValue;
    }
}