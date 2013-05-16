#region

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using Vermeil.Cache;

#endregion

namespace Vermeil.UI
{
    public class Attached
    {
        #region Autoupdate Binding

        public static readonly DependencyProperty AutoUpdateBindingProperty =
            DependencyProperty.RegisterAttached("AutoUpdateBinding",
                                                typeof (bool),
                                                typeof (Attached),
                                                new PropertyMetadata(OnUpdateSourceTriggerChanged));

        public static bool GetAutoUpdateBinding(DependencyObject target)
        {
            return (bool) target.GetValue(AutoUpdateBindingProperty);
        }

        public static void SetAutoUpdateBinding(DependencyObject target, bool value)
        {
            target.SetValue(AutoUpdateBindingProperty, value);
        }

        private static void OnUpdateSourceTriggerChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = target as PasswordBox;
            if (passwordBox != null)
            {
                if ((bool) e.OldValue)
                {
                    passwordBox.PasswordChanged -= PasswordBoxTextChanged;
                }
                if ((bool) e.NewValue)
                {
                    passwordBox.PasswordChanged += PasswordBoxTextChanged;
                }
            }
            var textBox = target as TextBox;
            if (textBox == null)
            {
                return;
            }
            if ((bool) e.OldValue)
            {
                textBox.TextChanged -= TextBoxTextChanged;
            }
            if ((bool) e.NewValue)
            {
                textBox.TextChanged += TextBoxTextChanged;
            }
        }

        private static void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            var bindingExpression = ((TextBox) sender).GetBindingExpression(TextBox.TextProperty);
            if (bindingExpression != null)
            {
                bindingExpression.UpdateSource();
            }
        }

        private static void PasswordBoxTextChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            var bindingExpression = ((PasswordBox) sender).GetBindingExpression(PasswordBox.PasswordProperty);
            if (bindingExpression != null)
            {
                bindingExpression.UpdateSource();
            }
        }

        #endregion

        #region Command

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command",
                                                typeof (ICommand),
                                                typeof (Attached),
                                                new PropertyMetadata(OnCommandChanged));

        public static ICommand GetCommand(DependencyObject dependencyObject)
        {
            return (ICommand) dependencyObject.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject dependencyObject, ICommand value)
        {
            dependencyObject.SetValue(CommandProperty, value);
        }

        private static void OnCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var command = e.NewValue as ICommand;
            if (command == null)
            {
                return;
            }
            if (dependencyObject is ButtonBase)
            {
                var button = dependencyObject as ButtonBase;
                button.Click += (sender, arg) => command.Execute(button.DataContext);
            }
            else if (dependencyObject is Selector)
            {
                var selector = dependencyObject as Selector;
                selector.SelectionChanged += (sender, arg) =>
                                                 {
                                                     var list = (Selector) sender;
                                                     if (arg.AddedItems.Count > 0)
                                                     {
                                                         command.Execute(arg.AddedItems[0]);
                                                     }
                                                     list.SelectedItem = null;
                                                 };
            }
            else if (dependencyObject is FrameworkElement)
            {
                var element = dependencyObject as FrameworkElement;
                element.MouseLeftButtonUp += (sender, arg) => command.Execute(element.DataContext);
            }
        }

        #endregion

        #region TextBoxReturn command

        public static readonly DependencyProperty ReturnCommandProperty =
            DependencyProperty.RegisterAttached("ReturnCommand",
                                                typeof (ICommand),
                                                typeof (Attached),
                                                new PropertyMetadata(OnReturnCommandChanged));

        public static ICommand GetReturnCommand(DependencyObject dependencyObject)
        {
            return (ICommand) dependencyObject.GetValue(ReturnCommandProperty);
        }

        public static void SetReturnCommand(DependencyObject dependencyObject, ICommand value)
        {
            dependencyObject.SetValue(ReturnCommandProperty, value);
        }

        private static void OnReturnCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (!(dependencyObject is TextBox))
            {
                return;
            }
            var textBox = dependencyObject as TextBox;
            var command = e.NewValue as ICommand;
            if (command == null)
            {
                return;
            }
            textBox.KeyUp += (sender, arg) =>
                                 {
                                     if (arg.Key == Key.Enter)
                                     {
                                         command.Execute(textBox.DataContext);
                                     }
                                 };
        }

        #endregion

        #region Image UriSource

        public static readonly DependencyProperty UriSourceProperty =
            DependencyProperty.RegisterAttached("UriSource",
                                                typeof (Uri),
                                                typeof (Attached),
                                                new PropertyMetadata(OnUriSourceChanged));


        public static Uri GetUriSource(Image obj)
        {
            return (Uri) obj.GetValue(UriSourceProperty);
        }

        public static void SetUriSource(Image obj, Uri value)
        {
            obj.SetValue(UriSourceProperty, value);
        }

        private static void OnUriSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var image = dependencyObject as Image;
            var bootstrapper = Bootstrapper.Current;
            if (image == null || bootstrapper == null)
            {
                return;
            }
            var uri = (Uri) e.NewValue;
            var imageCache = bootstrapper.Container.TryResolve<IImageCache>();
            image.Source = DesignerProperties.IsInDesignTool || imageCache == null
                               ? new BitmapImage(uri)
                               : imageCache.Get(uri);
        }

        #endregion

        #region AutoFocus

        public static readonly DependencyProperty AutoFocusProperty =
            DependencyProperty.RegisterAttached("AutoFocus",
                                                typeof (bool),
                                                typeof (Attached),
                                                new PropertyMetadata(AutoFocusChanged));

        public static bool GetAutoFocus(DependencyObject target)
        {
            return (bool) target.GetValue(AutoFocusProperty);
        }

        public static void SetAutoFocus(DependencyObject target, bool value)
        {
            target.SetValue(AutoFocusProperty, value);
        }

        private static void AutoFocusChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (!(target is TextBox))
            {
                return;
            }
            var textBox = (TextBox) target;
            if ((bool) e.OldValue)
            {
                textBox.Loaded -= TextBoxLoaded;
                textBox.TextChanged -= TextChanged;
            }
            if ((bool) e.NewValue)
            {
                textBox.Loaded += TextBoxLoaded;
                textBox.TextChanged += TextChanged;
            }
        }

        private static void TextBoxLoaded(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox) sender;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Focus();
            }
            textBox.Loaded -= TextBoxLoaded;
        }

        private static void TextChanged(object sender, RoutedEventArgs e)
        {
            var textBox = (TextBox) sender;
            textBox.SelectionStart = textBox.Text.Length;
            textBox.TextChanged -= TextChanged;
        }

        #endregion

        #region PasswordBox

        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached("BoundPassword",
                                                typeof (string),
                                                typeof (Attached),
                                                new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached("BindPassword",
                                                typeof (bool),
                                                typeof (Attached),
                                                new PropertyMetadata(false, OnBindPasswordChanged));

        private static readonly DependencyProperty UpdatingPasswordProperty =
            DependencyProperty.RegisterAttached("UpdatingPassword",
                                                typeof (bool),
                                                typeof (Attached),
                                                new PropertyMetadata(false));

        private static void OnBoundPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs changedEventArgs)
        {
            var passwordBox = dependencyObject as PasswordBox;
            if (passwordBox == null || !GetBindPassword(dependencyObject))
            {
                return;
            }
            passwordBox.PasswordChanged -= HandlePasswordChanged;
            var newPassword = (string) changedEventArgs.NewValue;
            if (!GetUpdatingPassword(passwordBox))
            {
                passwordBox.Password = newPassword ?? string.Empty;
            }
            passwordBox.PasswordChanged += HandlePasswordChanged;
        }

        private static void OnBindPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs changedEventArgs)
        {
            var passwordBox = dependencyObject as PasswordBox;
            if (passwordBox == null)
            {
                return;
            }
            var wasBound = (bool) (changedEventArgs.OldValue);
            var needToBind = (bool) (changedEventArgs.NewValue);
            if (wasBound)
            {
                passwordBox.PasswordChanged -= HandlePasswordChanged;
            }
            if (needToBind)
            {
                passwordBox.PasswordChanged += HandlePasswordChanged;
            }
        }

        private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            SetUpdatingPassword(passwordBox, true);
            if (passwordBox == null)
            {
                return;
            }
            SetBoundPassword(passwordBox, passwordBox.Password);
            SetUpdatingPassword(passwordBox, false);
        }

        public static void SetBindPassword(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(BindPasswordProperty, value);
        }

        public static bool GetBindPassword(DependencyObject dependencyObject)
        {
            return (bool) dependencyObject.GetValue(BindPasswordProperty);
        }

        public static string GetBoundPassword(DependencyObject dependencyObject)
        {
            return (string) dependencyObject.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject dependencyObject, string value)
        {
            dependencyObject.SetValue(BoundPasswordProperty, value);
        }

        private static bool GetUpdatingPassword(DependencyObject dependencyObject)
        {
            return (bool) dependencyObject.GetValue(UpdatingPasswordProperty);
        }

        private static void SetUpdatingPassword(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(UpdatingPasswordProperty, value);
        }

        #endregion

        #region HyperlinkButton

        public static readonly DependencyProperty WebLinkProperty =
            DependencyProperty.RegisterAttached("WebLink",
                                                typeof (Uri),
                                                typeof (Attached),
                                                new PropertyMetadata(OnWebLinkChanged));

        public static Uri GetWebLink(DependencyObject dependencyObject)
        {
            return (Uri) dependencyObject.GetValue(WebLinkProperty);
        }

        public static void SetWebLink(DependencyObject dependencyObject, Uri value)
        {
            dependencyObject.SetValue(WebLinkProperty, value);
        }

        private static void OnWebLinkChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs changedEventArgs)
        {
            var hyperlinkButton = dependencyObject as HyperlinkButton;
            if (hyperlinkButton == null)
            {
                return;
            }

            var uri = changedEventArgs.NewValue as Uri;
            if (uri == null)
            {
                return;
            }
            hyperlinkButton.Click += (sender, arg) =>
                                         {
                                             var task = new WebBrowserTask {Uri = uri};
                                             task.Show();
                                         };
        }

        #endregion
    }
}