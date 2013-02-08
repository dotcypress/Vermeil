#region

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Vermeil.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool) value;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool) value;
            }
            return null;
        }
    }
}