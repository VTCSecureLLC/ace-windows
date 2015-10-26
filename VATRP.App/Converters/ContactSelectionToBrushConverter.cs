using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VATRP.Core.Model;

namespace VATRP.App.Converters
{
    public class ContactSelectionToBrushConverter : IValueConverter
    {
        public ContactSelectionToBrushConverter()
        {
            
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && (bool)value)
            {
                return new SolidColorBrush(Color.FromArgb(255, 175, 238, 238));
            }

            return new SolidColorBrush(Color.FromArgb(255, 240, 248, 255));

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}