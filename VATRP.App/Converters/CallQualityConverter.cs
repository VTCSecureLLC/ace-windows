using System;
using System.Globalization;
using System.Windows.Data;
using VATRP.Core.Model;
using VATRP.Linphone.VideoWrapper;

namespace com.vtcsecure.ace.windows.Converters
{
    public class CallQualityConverter : IValueConverter
    {
        public CallQualityConverter()
        {
            
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is QualityIndicator)
            {
                switch ((QualityIndicator) value)
                {
                    case QualityIndicator.ToBad:
                        return "Too bad";
                    case QualityIndicator.VeryPoor:
                        return "Very poor";
                    case QualityIndicator.Poor:
                        return "Poor";
                    case QualityIndicator.Medium:
                        return "Average";
                    case QualityIndicator.Good:
                        return "Good";
                }
               
            }

            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}