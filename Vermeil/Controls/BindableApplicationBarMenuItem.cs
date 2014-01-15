#region

using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Shell;
using Vermeil.Core;

#endregion

namespace Vermeil.Controls
{
    public class BindableApplicationBarMenuItem : FrameworkElement, IApplicationBarMenuItem
    {
        public static readonly DependencyProperty CommandProperty = VermeilExtensions.Register<ICommand, BindableApplicationBarMenuItem>("Command", null, CommandChanged);
        public static readonly DependencyProperty CommandParameterProperty = VermeilExtensions.Register<object, BindableApplicationBarMenuItem>("CommandParameter", null, CommandParameterChanged);
        public static readonly DependencyProperty TextProperty = VermeilExtensions.Register<string, BindableApplicationBarMenuItem>("Text", null, OnTextChanged);
        public static readonly DependencyProperty IsEnabledProperty = VermeilExtensions.Register<bool, BindableApplicationBarMenuItem>("IsEnabled", true, OnEnabledChanged);
        public static readonly DependencyProperty IsVisibleProperty = VermeilExtensions.Register<bool, BindableApplicationBarMenuItem>("IsVisible", true, (x, y) => x.OnVisibleChanged(x, y));

        public BindableApplicationBarMenuItem()
        {
            MenuItem = new ApplicationBarMenuItem {Text = "-"};
            MenuItem.Click += ApplicationBarMenuItemClick;
        }

        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public ApplicationBarMenuItem MenuItem { get; set; }

        public bool IsVisible
        {
            get { return (bool) GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public bool IsEnabled
        {
            get { return (bool) GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        event EventHandler IApplicationBarMenuItem.Click
        {
            add { }
            remove { }
        }

        internal event EventHandler IsVisibleChanged;

        private void OnVisibleChanged(BindableApplicationBarMenuItem d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
            {
                return;
            }
            if (IsVisibleChanged != null)
            {
                IsVisibleChanged(d, new EventArgs());
            }
        }

        private static void CommandChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var menuItem = (BindableApplicationBarMenuItem) source;
            var command = e.NewValue as ICommand;
            if (command == null)
            {
                return;
            }
            menuItem.IsEnabled = command.CanExecute(menuItem.CommandParameter);
        }

        private static void CommandParameterChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var menuItem = (BindableApplicationBarMenuItem) source;
            if (menuItem.Command == null)
            {
                return;
            }
            menuItem.IsEnabled = menuItem.Command.CanExecute(e.NewValue);
        }

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((BindableApplicationBarMenuItem) d).MenuItem.IsEnabled = (bool) e.NewValue;
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((BindableApplicationBarMenuItem) d).MenuItem.Text = e.NewValue.ToString();
            }
        }

        private void ApplicationBarMenuItemClick(object sender, EventArgs e)
        {
            if (Command != null)
            {
                Command.Execute(CommandParameter);
            }
        }
    }
}
