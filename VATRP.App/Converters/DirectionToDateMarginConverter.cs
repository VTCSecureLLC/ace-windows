using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VATRP.Core.Enums;

namespace com.vtcsecure.ace.windows.Converters
{
    public class DirectionToDateMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int extraHeight = 15;
            if (value is bool && (bool)value)
            {
                extraHeight += 5;
            }
            return new Thickness(0, extraHeight, 0, extraHeight);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

