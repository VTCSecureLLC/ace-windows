using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VATRP.Core.Enums;
using Color = System.Windows.Media.Color;

namespace com.vtcsecure.ace.windows.Converters
{
    public class BoolToBubbleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageDirection && (MessageDirection)value == MessageDirection.Outgoing)
            {
                return new SolidColorBrush(Color.FromArgb(0xff, 0xac, 0xd9, 0xf1));
            }
            return new SolidColorBrush(Color.FromArgb(0xff, 0xef, 0xe2, 0xff));
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }
}

