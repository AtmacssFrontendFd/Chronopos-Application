using System;
using System.Globalization;
using System.Windows.Data;
using ChronoPos.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChronoPos.Desktop.Converters;

/// <summary>
/// Converter that formats prices with active currency symbol and conversion
/// Converts from base currency (AED) to active currency
/// </summary>
public class CurrencyPriceConverter : IValueConverter
{
    private static IActiveCurrencyService? _currencyService;

    /// <summary>
    /// Initialize the currency service (called from App.xaml.cs)
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _currencyService = serviceProvider.GetRequiredService<IActiveCurrencyService>();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;

        if (_currencyService == null)
        {
            // Fallback if service not initialized
            if (value is decimal decimalValue)
                return $"${decimalValue:N2}";
            return value.ToString() ?? string.Empty;
        }

        try
        {
            // Convert the value to decimal
            decimal priceInBaseCurrency = value switch
            {
                decimal d => d,
                double db => (decimal)db,
                float f => (decimal)f,
                int i => i,
                long l => l,
                _ => decimal.TryParse(value.ToString(), out var parsed) ? parsed : 0m
            };

            // Convert from base currency (AED) to active currency
            var convertedPrice = _currencyService.ConvertFromBaseCurrency(priceInBaseCurrency);
            
            // Format with currency symbol
            return _currencyService.FormatPrice(convertedPrice);
        }
        catch (Exception)
        {
            // Fallback on error
            return value.ToString() ?? string.Empty;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("CurrencyPriceConverter does not support ConvertBack");
    }
}
