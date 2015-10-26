using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VATRP.Core.Enums;

namespace VATRP.App.Converters
{
    public class BoolToBubbleCornerConverter : IValueConverter
    {
        private double radius = 5.0;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageDirection && (MessageDirection)value == MessageDirection.Outgoing)
            {
                return new CornerRadius(radius, radius, 0, radius);
            }
            return new CornerRadius(0, radius, radius, radius);
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }
}

