using System;
using System.Globalization;
using System.Windows.Data;

namespace ChronoPos.Desktop.Converters;

/// <summary>
/// Converts screen name constants to their display-friendly names
/// </summary>
public class ScreenNameToDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string screenName)
        {
            return ChronoPos.Application.Constants.ScreenNames.GetDisplayName(screenName);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // For two-way binding, we need to find the screen name constant that matches the display name
        if (value is string displayName)
        {
            var allScreenNames = ChronoPos.Application.Constants.ScreenNames.GetAllScreenNamesWithAllOption();
            foreach (var screenName in allScreenNames)
            {
                if (ChronoPos.Application.Constants.ScreenNames.GetDisplayName(screenName) == displayName)
                {
                    return screenName;
                }
            }
        }
        return value;
    }
}
