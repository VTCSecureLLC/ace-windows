using System;
using System.Globalization;
using System.Windows.Data;

namespace com.vtcsecure.ace.windows.Converters
{

    public class MessageDateConverter : IValueConverter
    {

        public MessageDateConverter()
        {
            
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime)
            {
                string dateFormat = "dd.MM.yyyy";
                var diffTime = DateTime.Now - (DateTime)value;
                if (diffTime.Days == 0)
                    dateFormat = "h:mm tt";
                else if (diffTime.Days < 8)
                    dateFormat = "dddd";

                var dateString = ((DateTime)value).ToString(dateFormat);

                return dateString;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotImplementedException();
        }

        #endregion
    }
}
