using System;
using System.Globalization;
using System.Windows.Data;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.Converters
{
    public class CallStateToStringConverter : IValueConverter
    {
        public CallStateToStringConverter()
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
                    case VATRPCallState.Trying:
                        return "Trying";
                    case VATRPCallState.Ringing:
                        return "Ringing";
                    case VATRPCallState.InProgress:
                        return "Ringing";
                    case VATRPCallState.StreamsRunning:
                    case VATRPCallState.Connected:
                        return "Connected";
                    case VATRPCallState.RemotePaused:
                    case VATRPCallState.LocalPaused:
                        return "Held";
                    case VATRPCallState.LocalResumed:
                        return "Resumed";
                    case VATRPCallState.Closed:
                        return "Terminated";
                    case VATRPCallState.Error:
                        return string.Empty;
                    case VATRPCallState.Declined:
                        return "Call declined";
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}