#region

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using Vermeil.Cache;
using Vermeil.Core;

#endregion

namespace Vermeil
{
    public class Attached
    {
        #region Autoupdate Binding

        public static readonly DependencyProperty AutoUpdateBindingProperty = VermeilExtensions.RegisterAttached<bool, Attached>("AutoUpdateBinding", false, OnUpdateSourceTriggerChanged);

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

        public static readonly DependencyProperty CommandProperty = VermeilExtensions.RegisterAttached<ICommand, Attached>("Command", null, OnCommandChanged);

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

        public static readonly DependencyProperty ReturnCommandProperty = VermeilExtensions.RegisterAttached<ICommand, Attached>("ReturnCommand", null, OnReturnCommandChanged);

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

        public static readonly DependencyProperty UriSourceProperty = VermeilExtensions.RegisterAttached<Uri, Attached>("UriSource", null, OnUriSourceChanged);


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

        public static readonly DependencyProperty AutoFocusProperty = VermeilExtensions.RegisterAttached<bool, Attached>("AutoFocus", false, AutoFocusChanged);

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

        public static readonly DependencyProperty BoundPasswordProperty = VermeilExtensions.RegisterAttached<string, Attached>("BoundPassword", string.Empty, OnBoundPasswordChanged);

        public static readonly DependencyProperty BindPasswordProperty = VermeilExtensions.RegisterAttached<bool, Attached>("BindPassword", false, OnBindPasswordChanged);

        private static readonly DependencyProperty UpdatingPasswordProperty = VermeilExtensions.RegisterAttached<bool, Attached>("UpdatingPassword");

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

        public static readonly DependencyProperty WebLinkProperty = VermeilExtensions.RegisterAttached<Uri, Attached>("WebLink", null, OnWebLinkChanged);

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

        #region Animation Trigger

        public static readonly DependencyProperty AnimationTriggerProperty =
            VermeilExtensions.RegisterAttached<bool?, Attached>("AnimationTrigger", null, AnimationTriggerChanged);

        public static bool GetAnimationTrigger(DependencyObject target)
        {
            return (bool) target.GetValue(AnimationTriggerProperty);
        }

        public static void SetAnimationTrigger(DependencyObject target, bool value)
        {
            target.SetValue(AnimationTriggerProperty, value);
        }

        private static void AnimationTriggerChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var storyboard = target as Storyboard;
            if (storyboard == null)
            {
                return;
            }
            storyboard.Begin();
        }

        #endregion

        #region Animation Conroller

        public static readonly DependencyProperty AnimationControllerProperty =
            VermeilExtensions.RegisterAttached<bool, Attached>("AnimationController", false, AnimationControllerChanged);

        public static bool GetAnimationController(DependencyObject target)
        {
            return (bool) target.GetValue(AnimationControllerProperty);
        }

        public static void SetAnimationController(DependencyObject target, bool value)
        {
            target.SetValue(AnimationControllerProperty, value);
        }

        private static void AnimationControllerChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var storyboard = target as Storyboard;

            if (storyboard == null || !(e.NewValue is bool))
            {
                return;
            }

            if ((bool) e.NewValue)
            {
                storyboard.Stop();
                storyboard.AutoReverse = false;
                storyboard.Begin();
            }
            else
            {
                storyboard.AutoReverse = true;
                var time = storyboard.GetCurrentTime();
                storyboard.Stop();
                storyboard.Begin();
                storyboard.SeekAlignedToLastTick(time);
            }
        }

        #endregion

        #region LoadMoreCommand

        public static readonly DependencyProperty LoadMoreCommandProperty = VermeilExtensions.RegisterAttached<ICommand, Attached>("LoadMoreCommand", null, OnLoadMoreCommandChanged);

        public static ICommand GetLoadMoreCommand(DependencyObject dependencyObject)
        {
            return (ICommand) dependencyObject.GetValue(LoadMoreCommandProperty);
        }

        public static void SetLoadMoreCommand(DependencyObject dependencyObject, ICommand value)
        {
            dependencyObject.SetValue(LoadMoreCommandProperty, value);
        }

        private static void OnLoadMoreCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var command = e.NewValue as ICommand;
            if (command == null)
            {
                return;
            }
            if (!(dependencyObject is ListBox))
            {
                return;
            }
            var list = dependencyObject as ListBox;
            list.Loaded += (s, ev) =>
                {
                    var scroll = list.FindFirstChild<ScrollViewer>();
                    if (scroll == null)
                    {
                        return;
                    }
                    var property = VermeilExtensions.RegisterAttached<double, Attached>("VerticalOffsetListenAttached" + Guid.NewGuid(),
                        0,
                        (sender, ea) =>
                            {
                                if (scroll.ScrollableHeight - scroll.VerticalOffset < 0.1)
                                {
                                    command.Execute(null);
                                }
                            });
                    var binding = new Binding {Source = scroll, Path = new PropertyPath("VerticalOffset"), Mode = BindingMode.OneWay};
                    scroll.SetBinding(property, binding);
                };
        }

        #endregion
    }
}
