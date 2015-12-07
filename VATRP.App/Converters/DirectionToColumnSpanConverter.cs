using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VATRP.Core.Enums;

namespace com.vtcsecure.ace.windows.Converters
{
    public class DirectionToColumnSpanConverter : IValueConverter
    {
        public DirectionToColumnSpanConverter()
        {
            FalseEquivalent = 2;
            OppositeBooleanValue = false;
        }
        public bool OppositeBooleanValue { get; set; }

        public int FalseEquivalent { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageDirection )
            {
                bool booleanValue = (MessageDirection)value == MessageDirection.Incoming;

                if (OppositeBooleanValue)
                {
                    booleanValue = !booleanValue;
                }

                return booleanValue ? 3 : FalseEquivalent;
            }
            return FalseEquivalent;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
        
    }
}

