using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VATRP.Core.Enums;

namespace com.vtcsecure.ace.windows.Converters
{
    public class BoolToBubbleCornerConverter : IValueConverter
    {
        public int Row { get; set; }
        public double Radius { get; set; }

        public BoolToBubbleCornerConverter()
        {
            Radius = 6.0;
            Row = 0;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageDirection)
            {
                if (Row == 0)
                {
                    return new CornerRadius(Radius, Radius, 0, 0);
                }
                else if (Row == 1)
                {
                    if ((MessageDirection)value == MessageDirection.Outgoing)
                    {
                        return new CornerRadius(0, 0, Radius * 2.5, Radius);
                    }
                    return new CornerRadius(0, 0, Radius, Radius * 2.5);
                }
            }
            return new CornerRadius(0, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }
}

