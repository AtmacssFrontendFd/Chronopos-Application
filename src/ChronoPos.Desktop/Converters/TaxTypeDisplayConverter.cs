using System;
using System.Globalization;
using System.Windows.Data;
using ChronoPos.Application.DTOs;

namespace ChronoPos.Desktop.Converters
{
    public class TaxTypeDisplayConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TaxTypeDto t)
            {
                // Show as: Name (Value% or Value)
                var val = t.Value;
                if (t.IsPercentage)
                    return $"{t.Name} ({val:0.####}%)";
                return $"{t.Name} ({val:0.####})";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
