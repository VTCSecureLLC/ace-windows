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
                return new SolidColorBrush(Color.FromArgb(0xff, 0xB2, 0xD0, 0xEE));
            }
            return new SolidColorBrush(Color.FromArgb(0xff, 0xDE, 0xB2, 0xEE));
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }
}

