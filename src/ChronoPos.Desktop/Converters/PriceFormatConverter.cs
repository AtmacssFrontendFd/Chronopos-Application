using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.Converters
{
    /// <summary>
    /// Converter that formats decimal prices using the active currency service.
    /// Expects the ConverterParameter to be the IActiveCurrencyService instance.
    /// </summary>
    public class PriceFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            if (value is decimal decimalValue)
            {
                // Try to get currency service from parameter (passed from binding)
                if (parameter is IActiveCurrencyService currencyService)
                {
                    return currencyService.FormatPrice(decimalValue);
                }
                
                // Fallback to simple formatting with $ symbol
                return $"${decimalValue:N2}";
            }

            return value.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
