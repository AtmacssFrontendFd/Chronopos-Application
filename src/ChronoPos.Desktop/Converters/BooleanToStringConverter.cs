using System;
using System.Globalization;
using System.Windows.Data;

namespace ChronoPos.Desktop.Converters
{
    /// <summary>
    /// Converter that converts boolean values to strings based on parameters
    /// </summary>
    public class BooleanToStringConverter : IValueConverter
    {
        public static BooleanToStringConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
                return string.Empty;

            if (parameter is not string parameterString)
                return boolValue.ToString();

            // Split the parameter by semicolon
            // Format: "TrueString;FalseString"
            var parts = parameterString.Split(';');
            
            if (parts.Length == 2)
            {
                return boolValue ? parts[0] : parts[1];
            }
            
            // Fallback to the whole parameter as true value
            return boolValue ? parameterString : string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("BooleanToStringConverter does not support two-way binding.");
        }
    }
}