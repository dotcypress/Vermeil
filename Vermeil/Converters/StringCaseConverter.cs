#region

using System;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace Vermeil.Converters
{
    public class StringCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is string)
            {
                var valueString = value.ToString();
                return parameter != null && parameter.ToString() == "lower"
                           ? valueString.ToLower()
                           : valueString.ToUpper();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}