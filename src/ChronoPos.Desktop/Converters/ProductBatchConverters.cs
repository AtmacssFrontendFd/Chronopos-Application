using System;
using System.Globalization;
using System.Windows.Data;

namespace ChronoPos.Desktop.Converters
{
    public class DaysToExpiryConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length != 2)
                return "N/A";

            if (values[0] is int daysToExpiry && values[1] is bool isExpired)
            {
                if (isExpired)
                {
                    return Math.Abs(daysToExpiry) == 1 ? "Expired (1 day ago)" : $"Expired ({Math.Abs(daysToExpiry)} days ago)";
                }

                if (daysToExpiry == 0)
                {
                    return "Expires Today";
                }

                return daysToExpiry == 1 ? "1 day" : $"{daysToExpiry} days";
            }

            return "N/A";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BatchStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() == "Active")
            {
                return "#2E7D32"; // Green
            }
            return "#C62828"; // Red
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}