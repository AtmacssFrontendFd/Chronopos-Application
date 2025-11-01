using System;
using System.Globalization;
using System.Windows.Data;
using ChronoPos.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChronoPos.Desktop.Converters;

/// <summary>
/// Converter that formats prices with active currency symbol and adds a negative sign
/// Used for discount amounts
/// </summary>
public class NegativeCurrencyPriceConverter : IValueConverter
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
                return $"-${decimalValue:N2}";
            return value.ToString() ?? string.Empty;
        }

        try
        {
            // Convert the value to decimal
            decimal priceValue = value switch
            {
                decimal d => d,
                double db => (decimal)db,
                float f => (decimal)f,
                int i => i,
                long l => l,
                _ => decimal.TryParse(value.ToString(), out var parsed) ? parsed : 0m
            };

            // Convert from base currency to active currency
            var convertedPrice = _currencyService.ConvertFromBaseCurrency(priceValue);
            
            // Format with currency symbol and add negative sign
            var formattedPrice = _currencyService.FormatPrice(convertedPrice);
            return $"-{formattedPrice}";
        }
        catch (Exception)
        {
            // Fallback on error
            return value.ToString() ?? string.Empty;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("NegativeCurrencyPriceConverter does not support ConvertBack");
    }
}
