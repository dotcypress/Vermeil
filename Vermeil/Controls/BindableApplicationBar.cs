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
using Vermeil.Core;

#endregion

namespace Vermeil.Controls
{
    [ContentProperty("Buttons")]
    public class BindableApplicationBar : ItemsControl, IApplicationBar
    {
        public static readonly DependencyProperty IsVisibleProperty = VermeilExtensions.RegisterAttached<bool, BindableApplicationBar>("IsVisible", true, OnVisibleChanged);
        public static readonly DependencyProperty IsMenuEnabledProperty = VermeilExtensions.RegisterAttached<bool, BindableApplicationBar>("IsMenuEnabled", true, OnEnabledChanged);

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
            var page = this.FindAncestorWithType<PhoneApplicationPage>();
            if (page != null)
            {
                page.ApplicationBar = _applicationBar;
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            foreach (BindableApplicationBarIconButton button in Items.Where(c => c is BindableApplicationBarIconButton))
            {
                button.IsVisibleChanged -= RebuildAppBar;
                button.IsVisibleChanged += RebuildAppBar;
            }
            foreach (BindableApplicationBarMenuItem button in Items.Where(c => c is BindableApplicationBarMenuItem))
            {
                button.IsVisibleChanged -= RebuildAppBar;
                button.IsVisibleChanged += RebuildAppBar;
            }
            _applicationBar.Buttons.Clear();
            _applicationBar.MenuItems.Clear();
            foreach (var button in Items.Where(c => c is BindableApplicationBarIconButton).Cast<BindableApplicationBarIconButton>().Where(button => button.IsVisible))
            {
                _applicationBar.Buttons.Add(button.Button);
            }
            foreach (var button in Items.Where(c => c is BindableApplicationBarMenuItem).Cast<BindableApplicationBarMenuItem>().Where(button => button.IsVisible))
            {
                _applicationBar.MenuItems.Add(button.MenuItem);
            }
        }

        private void RebuildAppBar(object sender, EventArgs eventArgs)
        {
            var iconButton = sender as BindableApplicationBarIconButton;
            if (iconButton != null)
            {
                if (!iconButton.IsVisible)
                {
                    _applicationBar.Buttons.Remove(iconButton.Button);
                }
                else
                {
                    var index = Items.Where(c => c is BindableApplicationBarIconButton).ToList().IndexOf(iconButton);
                    _applicationBar.Buttons.Insert(index, iconButton.Button);
                }
                return;
            }
            var menuItem = sender as BindableApplicationBarMenuItem;
            if (menuItem == null)
            {
                return;
            }
            if (!menuItem.IsVisible)
            {
                _applicationBar.MenuItems.Remove(menuItem.MenuItem);
            }
            else
            {
                var index = Items.Where(c => c is BindableApplicationBarMenuItem).ToList().IndexOf(menuItem);
                _applicationBar.MenuItems.Insert(index, menuItem.MenuItem);
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
