using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VATRP.Core.Enums;

namespace com.vtcsecure.ace.windows.Converters
{
    public class BoolToBubbleAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageDirection && (MessageDirection)value == MessageDirection.Outgoing)
            {
                return HorizontalAlignment.Right;
            }
            return HorizontalAlignment.Left;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }
}

