using System;
using System.Globalization;
using System.Windows.Data;
using VATRP.Core.Model;

namespace VATRP.App.Converters
{
    public class CallQualityConverter : IValueConverter
    {
        public CallQualityConverter()
        {
            
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float)
            {
                var rating = (float)value;
                if (rating >= 4.0)
                    return "Good";
                if (rating >= 3.0)
                    return "Average";
                if (rating >= 2.0)
                    return "Poor";
                if (rating >= 1.0)
                    return "Very poor";
                if (rating >= 0)
                    return "Too bad";
            }

            return "Unavailable";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}