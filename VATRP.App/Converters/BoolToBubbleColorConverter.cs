using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VATRP.Core.Enums;
using Color = System.Windows.Media.Color;

namespace VATRP.App.Converters
{
    public class BoolToBubbleColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageDirection && (MessageDirection)value == MessageDirection.Outgoing)
            {
                return new SolidColorBrush(Color.FromArgb(0xff, 239, 226, 255));
            }            
            return new SolidColorBrush(Color.FromArgb(0xff, 237, 237, 237));
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }
}

