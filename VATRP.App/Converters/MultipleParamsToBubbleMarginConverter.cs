using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VATRP.Core.Enums;

namespace com.vtcsecure.ace.windows.Converters
{
    public class MultipleParamsToBubbleMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int paddingHeigth = 5;
            if (values.Length > 1)
            {
                var isRtt = values[1] is bool && (bool)values[1];
                //if (isRtt)
                //    paddingHeigth += 5;
            }
            if (values[0] is MessageDirection && (MessageDirection)values[0] == MessageDirection.Outgoing)
            {
                
                return new Thickness(20, paddingHeigth, 5, paddingHeigth);
            }
            return new Thickness(5, paddingHeigth, 20, paddingHeigth);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

