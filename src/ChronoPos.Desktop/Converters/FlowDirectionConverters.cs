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

    public class FlowDirectionToGridColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowDirection flowDirection && parameter is string columnParam)
            {
                // Parameter can be "start", "end", "center"
                // For 2-column grids: start=0/1, end=1/0
                // For 3-column grids: start=0/2, center=1, end=2/0
                switch (columnParam.ToLower())
                {
                    case "start":
                        return flowDirection == FlowDirection.RightToLeft ? 1 : 0;
                    case "end":
                        return flowDirection == FlowDirection.RightToLeft ? 0 : 1;
                    case "center":
                        return 1; // Center is always column 1 (for 3-column grids)
                    case "start3": // For 3-column grids
                        return flowDirection == FlowDirection.RightToLeft ? 2 : 0;
                    case "end3": // For 3-column grids
                        return flowDirection == FlowDirection.RightToLeft ? 0 : 2;
                    default:
                        return 0;
                }
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}