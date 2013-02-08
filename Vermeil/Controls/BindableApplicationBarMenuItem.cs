#region

using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Shell;

#endregion

namespace Vermeil.Controls
{
    public class BindableApplicationBarMenuItem : FrameworkElement, IApplicationBarMenuItem
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command",
                                                                                                        typeof (ICommand),
                                                                                                        typeof (BindableApplicationBarMenuItem),
                                                                                                        new PropertyMetadata(CommandChanged));


        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached("CommandParameter",
                                                                                                                 typeof (object),
                                                                                                                 typeof (BindableApplicationBarMenuItem),
                                                                                                                 new PropertyMetadata(CommandParameterChanged));


        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text",
                                                                                                     typeof (string),
                                                                                                     typeof (BindableApplicationBarMenuItem),
                                                                                                     new PropertyMetadata(OnTextChanged));


        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled",
                                                                                                          typeof (bool),
                                                                                                          typeof (BindableApplicationBarMenuItem),
                                                                                                          new PropertyMetadata(true, OnEnabledChanged));


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

        public ApplicationBarMenuItem MenuItem { get; set; }

        public BindableApplicationBarMenuItem()
        {
            MenuItem = new ApplicationBarMenuItem {Text = "-"};
            MenuItem.Click += ApplicationBarMenuItemClick;
        }

        private void ApplicationBarMenuItemClick(object sender, EventArgs e)
        {
            if (Command != null)
            {
                Command.Execute(CommandParameter);
            }
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
            add {}
            remove {  }
        }
    }
}