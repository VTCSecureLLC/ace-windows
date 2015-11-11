using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VATRP.Core.Model;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows.Converters
{
    public class RegistrationStateToBrushConverter : IValueConverter
    {
        public RegistrationStateToBrushConverter()
        {
            
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LinphoneRegistrationState)
            {
                var state = (LinphoneRegistrationState)value;

                switch (state)
                {
                    case LinphoneRegistrationState.LinphoneRegistrationOk:
                        return new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                    case LinphoneRegistrationState.LinphoneRegistrationFailed:
                        return new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    default:
                        return new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
                }
            }

            return new SolidColorBrush(Color.FromArgb(255, 0x0d, 0x6e, 0x0f));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}