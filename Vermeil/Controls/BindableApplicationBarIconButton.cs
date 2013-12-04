#region

using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Phone.Shell;
using Vermeil.Core;

#endregion

namespace Vermeil.Controls
{
    public class BindableApplicationBarIconButton : FrameworkElement, IApplicationBarIconButton
    {
        public static readonly DependencyProperty CommandProperty = VermeilExtensions.Register<ICommand, BindableApplicationBarIconButton>("Command", null, CommandChanged);
        public static readonly DependencyProperty CommandParameterProperty = VermeilExtensions.Register<object, BindableApplicationBarIconButton>("CommandParameter", null, CommandParameterChanged);
        public static readonly DependencyProperty CommandParameterValueProperty = VermeilExtensions.RegisterAttached<object, BindableApplicationBarIconButton>("CommandParameterValue");
        public static readonly DependencyProperty IsEnabledProperty = VermeilExtensions.Register<bool, BindableApplicationBarIconButton>("IsEnabled", true, OnEnabledChanged);
        public static readonly DependencyProperty IsVisibleProperty = VermeilExtensions.Register<bool, BindableApplicationBarIconButton>("IsVisible", true, (x, y) => x.OnVisibleChanged(x, y));
        public static readonly DependencyProperty TextProperty = VermeilExtensions.Register<string, BindableApplicationBarIconButton>("Text", null, OnTextChanged);
        public static readonly DependencyProperty IconUriProperty = VermeilExtensions.Register<Uri, BindableApplicationBarIconButton>("IconUri", null, OnIconUriChanged);

        public BindableApplicationBarIconButton()
        {
            Button = new ApplicationBarIconButton {Text = "-", IconUri = new Uri("/holder.png", UriKind.Relative)};
            Button.Click += ApplicationBarIconButtonClick;
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

        public object CommandParameterValue
        {
            get { return GetValue(CommandParameterValueProperty); }
            set { SetValue(CommandParameterValueProperty, value); }
        }

        public ApplicationBarIconButton Button { get; set; }

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

        public Uri IconUri
        {
            get { return (Uri) GetValue(IconUriProperty); }
            set { SetValue(IconUriProperty, value); }
        }

        internal event EventHandler IsVisibleChanged;

        private static void CommandChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var barIconButton = (BindableApplicationBarIconButton) source;
            var command = e.NewValue as ICommand;
            if (command == null)
            {
                return;
            }
            barIconButton.IsEnabled = command.CanExecute(barIconButton.CommandParameter);
        }

        private static void CommandParameterChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var barIconButton = (BindableApplicationBarIconButton) source;
            if (barIconButton.Command == null)
            {
                return;
            }
            barIconButton.IsEnabled = barIconButton.Command.CanExecute(e.NewValue);
        }

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((BindableApplicationBarIconButton) d).Button.IsEnabled = (bool) e.NewValue;
            }
        }

        private void OnVisibleChanged(BindableApplicationBarIconButton d, DependencyPropertyChangedEventArgs e)
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

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((BindableApplicationBarIconButton) d).Button.Text = e.NewValue.ToString();
            }
        }

        private static void OnIconUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var newValue = (Uri) e.NewValue;
                ((BindableApplicationBarIconButton) d).Button.IconUri = new Uri(newValue.OriginalString, UriKind.Relative);
            }
        }


        private void ApplicationBarIconButtonClick(object sender, EventArgs e)
        {
            if (Command != null && CommandParameter != null)
            {
                Command.Execute(CommandParameter);
            }
            else if (Command != null)
            {
                Command.Execute(CommandParameterValue);
            }
        }
    }
}
