#region

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

#endregion

namespace Vermeil.Controls
{
    [ContentProperty("Buttons")]
    public class BindableApplicationBar : ItemsControl, IApplicationBar
    {
        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.RegisterAttached("IsVisible",
            typeof (bool),
            typeof (BindableApplicationBar),
            new PropertyMetadata(true, OnVisibleChanged));


        public static readonly DependencyProperty IsMenuEnabledProperty = DependencyProperty.RegisterAttached("IsMenuEnabled",
            typeof (bool),
            typeof (BindableApplicationBar),
            new PropertyMetadata(true, OnEnabledChanged));

        private readonly ApplicationBar _applicationBar;

        public BindableApplicationBar()
        {
            _applicationBar = new ApplicationBar();
            Loaded += ApplicationBarLoaded;
        }

        public double BarOpacity
        {
            get { return _applicationBar.Opacity; }
            set { _applicationBar.Opacity = value; }
        }

        public bool IsVisible
        {
            get { return (bool) GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public bool IsMenuEnabled
        {
            get { return (bool) GetValue(IsMenuEnabledProperty); }
            set { SetValue(IsMenuEnabledProperty, value); }
        }

        public Color BackgroundColor
        {
            get { return _applicationBar.BackgroundColor; }
            set { _applicationBar.BackgroundColor = value; }
        }

        public Color ForegroundColor
        {
            get { return _applicationBar.ForegroundColor; }
            set { _applicationBar.ForegroundColor = value; }
        }

        public ApplicationBarMode Mode
        {
            get { return _applicationBar.Mode; }
            set { _applicationBar.Mode = value; }
        }

        public double DefaultSize
        {
            get { return _applicationBar.DefaultSize; }
        }

        public double MiniSize
        {
            get { return _applicationBar.MiniSize; }
        }

        public IList Buttons
        {
            get { return Items; }
        }

        public IList MenuItems
        {
            get { return Items; }
        }

        event EventHandler<ApplicationBarStateChangedEventArgs> IApplicationBar.StateChanged
        {
            add { }
            remove { }
        }

        private void ApplicationBarLoaded(object sender, RoutedEventArgs e)
        {
            var page = UIExtensions.FindAncestor(this, typeof (PhoneApplicationPage)) as PhoneApplicationPage;
            if (page != null)
            {
                page.ApplicationBar = _applicationBar;
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            _applicationBar.Buttons.Clear();
            _applicationBar.MenuItems.Clear();
            foreach (BindableApplicationBarIconButton button in Items.Where(c => c is BindableApplicationBarIconButton))
            {
                _applicationBar.Buttons.Add(button.Button);
            }
            foreach (BindableApplicationBarMenuItem button in Items.Where(c => c is BindableApplicationBarMenuItem))
            {
                _applicationBar.MenuItems.Add(button.MenuItem);
            }
        }

        private static void OnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((BindableApplicationBar) d)._applicationBar.IsVisible = (bool) e.NewValue;
            }
        }

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((BindableApplicationBar) d)._applicationBar.IsMenuEnabled = (bool) e.NewValue;
            }
        }
    }
}
