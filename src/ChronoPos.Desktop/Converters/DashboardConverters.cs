using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChronoPos.Desktop.Converters;

/// <summary>
/// Converter for dashboard-related value conversions
/// </summary>
public class SalesToHeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // values[0] = sales (decimal)
        // values[1] = maxSalesValue (double from ViewModel)
        // parameter = maxHeight (string)
        
        if (values.Length >= 2 && 
            values[0] is decimal sales && 
            values[1] is double maxSalesValue &&
            parameter is string maxHeightStr &&
            double.TryParse(maxHeightStr, out double maxHeight))
        {
            // If sales is 0, return minimum visible height
            if (sales == 0)
                return 2.0;
            
            // If maxSalesValue is 0 or invalid, use the sales value itself
            if (maxSalesValue <= 0)
                maxSalesValue = (double)sales;
            
            // Calculate percentage based on dynamic max sales
            var percentage = Math.Min((double)sales / maxSalesValue, 1.0);
            
            // Ensure minimum visible height for any non-zero value
            var calculatedHeight = maxHeight * percentage;
            var finalHeight = Math.Max(calculatedHeight, 30.0); // Increased minimum for better visibility
            
            // Debug logging for non-zero sales
            if (sales > 0)
            {
                Console.WriteLine($"[SalesToHeightConverter] Sales={sales:C}, MaxSales={maxSalesValue:C}, Percentage={percentage:P2}, CalculatedHeight={calculatedHeight:F1}px, FinalHeight={finalHeight:F1}px");
            }
            
            return finalHeight;
        }
        
        var maxSalesDebug = values?.Length > 1 ? values[1]?.ToString() : "N/A";
        Console.WriteLine($"[SalesToHeightConverter] Invalid values - Length={values?.Length}, Sales={values?[0]}, MaxSales={maxSalesDebug}, Parameter={parameter}");
        return 2.0; // Minimum height for zero or invalid values
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to check if a number is positive
/// </summary>
public class IsPositiveConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
            return decimalValue > 0;
        if (value is int intValue)
            return intValue > 0;
        if (value is double doubleValue)
            return doubleValue > 0;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter to check if a number is negative
/// </summary>
public class IsNegativeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
            return decimalValue < 0;
        if (value is int intValue)
            return intValue < 0;
        if (value is double doubleValue)
            return doubleValue < 0;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converter for Boolean to Visibility
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}
