using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VATRP.Core.Enums;

namespace VATRP.App.Converters
{
    public class BoolToBubbleAlignmentConverter : IValueConverter
    {
        public HorizontalAlignment FalseEquivalent { get; set; }

        public bool OppositeBooleanValue { get; set; }

        public BoolToBubbleAlignmentConverter()
        {
            this.FalseEquivalent = HorizontalAlignment.Right;
            this.OppositeBooleanValue = false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool booleanValue = false;
            if (value is MessageDirection && (MessageDirection) value == MessageDirection.Outgoing)
            {
                booleanValue = !OppositeBooleanValue;
            }

            return booleanValue ? HorizontalAlignment.Left : FalseEquivalent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
    }
}

