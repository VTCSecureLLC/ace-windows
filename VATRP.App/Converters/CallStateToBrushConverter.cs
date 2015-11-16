using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Converters
{
    public class CallStateToBrushConverter : IValueConverter
    {
        public CallStateToBrushConverter()
        {
            
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VATRPCallState)
            {
                var state = (VATRPCallState)value;

                switch (state)
                {
                    case VATRPCallState.Closed:
                    case VATRPCallState.Error:
                        return new SolidColorBrush(Color.FromArgb(255, 0x2B, 0x91, 0x2D));
                    default:
                        return new SolidColorBrush(Color.FromArgb(255, 255,0,0));
                }
            }

            return new SolidColorBrush(Color.FromArgb(255, 0x2B, 0x91, 0x2D));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}