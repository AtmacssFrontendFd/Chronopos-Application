using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChronoPos.Desktop.Converters
{
    public class FlowDirectionToTextAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowDirection flowDirection)
            {
                return flowDirection == FlowDirection.RightToLeft ? TextAlignment.Right : TextAlignment.Left;
            }
            return TextAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FlowDirectionToHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowDirection flowDirection)
            {
                return flowDirection == FlowDirection.RightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FlowDirectionToReverseHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowDirection flowDirection)
            {
                // Inverted logic: LTR -> Right, RTL -> Left (for side panels)
                return flowDirection == FlowDirection.RightToLeft ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            }
            return HorizontalAlignment.Right;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}