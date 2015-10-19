using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VATRP.App.Converters
{
    public class IntToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// FalseEquivalent (default : Visibility.Collapsed => see Constructor)
        /// </summary>
        public Visibility FalseEquivalent { get; set; }
        /// <summary>
        /// Define whether the opposite boolean value is crucial (default : false)
        /// </summary>
        public bool OppositeBooleanValue { get; set; }

        public IntToVisibilityConverter()
        {
            this.FalseEquivalent = Visibility.Collapsed;
            this.OppositeBooleanValue = false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo language)
        {
            if (value is int && targetType == typeof(Visibility))
            {
                bool booleanValue = (int)value == 0;

                if (OppositeBooleanValue)
                {
                    booleanValue = !booleanValue;
                }

                return booleanValue ? Visibility.Visible : FalseEquivalent;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo language)
        {
            throw new NotImplementedException();
        }
    }
}
