#region

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

#endregion

namespace Vermeil.Converters
{
    public class ComplementColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is Color)
            {
                var color = (Color) value;
                return new Color
                           {
                               R = (byte) (255 - color.R),
                               G = (byte) (255 - color.G),
                               B = (byte) (255 - color.B),
                               A = color.A
                           };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}