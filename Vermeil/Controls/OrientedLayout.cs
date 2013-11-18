#region

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Phone.Controls;
using Vermeil.Core;

#endregion

namespace Vermeil.Controls
{
    public class OrientedLayout : Control
    {
        public static readonly DependencyProperty OrientationProperty = VermeilExtensions.Register<PageOrientation, OrientedLayout>("Orientation");
        
        public OrientedLayout()
        {
            DefaultStyleKey = typeof (OrientedLayout);
        }

        public object PortraitLayout { get; set; }

        public object LandscapeLayout { get; set; }

        public PageOrientation Orientation
        {
            get { return (PageOrientation) GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
    }

    public class OrientationToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PageOrientation)
            {
                var orientation = (PageOrientation) value;
                var inverse = parameter != null && parameter.ToString() == "Landscape";
                var isPortrait = orientation == PageOrientation.Portrait
                    || orientation == PageOrientation.PortraitDown
                    || orientation == PageOrientation.PortraitUp
                    || orientation == PageOrientation.None;
                if (inverse)
                {
                    isPortrait = !isPortrait;
                }
                return isPortrait ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
