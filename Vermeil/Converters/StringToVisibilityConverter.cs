#region

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#endregion

namespace Vermeil.Converters
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value as string;
            if (stringValue != null)
            {
                var inverse = parameter != null && parameter.ToString() == "inverse";
                var isNullOrEmpty = string.IsNullOrWhiteSpace(stringValue);
                return inverse
                    ? (isNullOrEmpty ? Visibility.Visible : Visibility.Collapsed)
                    : (isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible);
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
