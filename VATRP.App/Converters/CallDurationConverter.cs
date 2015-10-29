using System;
using System.Globalization;
using System.Windows.Data;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Converters
{
    public class CallDurationConverter : IValueConverter
    {
        public CallDurationConverter()
        {
            
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int)
            {
                var duration = (int)value;
                return string.Format("{0:D2}:{1:D2}:{2:D2}", duration/3600, (duration/60)%60, duration%60);
            }

            return "00:00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}