using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Converters
{
    public class ContactFavoriteToBrushConverter : IValueConverter
    {
        public ContactFavoriteToBrushConverter()
        {
            
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                return new SolidColorBrush(Color.FromArgb(0xff, 0xe7, 0x3e, 0x0a));
            }

            return new SolidColorBrush(Color.FromArgb(255, 32, 32, 32));

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}