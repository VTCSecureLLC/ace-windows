using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VATRP.Core.Enums;
using VATRP.LinphoneWrapper.Enums;
using Color = System.Windows.Media.Color;

namespace com.vtcsecure.ace.windows.Converters
{
    public class MessageDeliveryStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LinphoneChatMessageState)
            {
                switch ((LinphoneChatMessageState) value)
                {
                    case LinphoneChatMessageState.LinphoneChatMessageStateIdle:
                        return new SolidColorBrush(Color.FromArgb(255, 239, 244, 253));
                    case LinphoneChatMessageState.LinphoneChatMessageStateInProgress:
                        return new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
                    case LinphoneChatMessageState.LinphoneChatMessageStateDelivered:
                        return new SolidColorBrush(Color.FromArgb(255, 108, 175, 74));
                    case LinphoneChatMessageState.LinphoneChatMessageStateNotDelivered:
                        return new SolidColorBrush(Color.FromArgb(255, 235, 62, 66));
                }
            }            
            return new SolidColorBrush(Color.FromArgb(0xff, 237, 237, 237));
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }
}

