using System;
using System.Globalization;
using System.Windows.Data;
using ChronoPos.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChronoPos.Desktop.Converters;

/// <summary>
/// Converter that translates resource keys to localized strings
/// based on the current active language
/// </summary>
public class LocalizationConverter : IValueConverter
{
    private static IDatabaseLocalizationService? _localizationService;

    /// <summary>
    /// Initialize the localization service (called from App.xaml.cs)
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        _localizationService = serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null && parameter == null)
            return string.Empty;

        // Use parameter as the key if value is null
        var key = (value ?? parameter)?.ToString();
        
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        if (_localizationService == null)
        {
            // Fallback if service not initialized - return the key itself
            return key;
        }

        try
        {
            // Get translation asynchronously
            // Note: This is a synchronous converter, so we need to handle async carefully
            var task = _localizationService.GetTranslationAsync(key);
            task.Wait(TimeSpan.FromMilliseconds(500)); // Short timeout to prevent UI freeze
            
            return task.IsCompleted ? task.Result : key;
        }
        catch (Exception)
        {
            // Fallback on error - return the key
            return key;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("LocalizationConverter does not support ConvertBack");
    }
}
