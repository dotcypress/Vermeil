#region

using System.Windows;
using System.Windows.Input;
using Vermeil.Commands;
using Vermeil.Core;
using Vermeil.Core.IoC;
using Vermeil.Core.Logging;
using Vermeil.MVVM;
using Vermeil.State;

#endregion

namespace Sample.ViewModels
{
    public class MainPageViewModel : ViewModel
    {
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("MyProperty", typeof (string), typeof (MainPageViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty ShowButtonProperty =
            DependencyProperty.Register("ShowButton", typeof (bool), typeof (MainPageViewModel), new PropertyMetadata(true));


        [Tombstoned]
        public string MyProperty
        {
            get { return (string) GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

        public bool ShowButton
        {
            get { return (bool) GetValue(ShowButtonProperty); }
            set { SetValue(ShowButtonProperty, value); }
        }

        [Inject]
        public ILogger Logger { get; set; }

        [Inject]
        public IProgressIndicatorService ProgressIndicatorService { get; set; }

        [Inject("silent")]
        public ILogger SilentLogger { get; set; }

        public ICommand InfoCommand
        {
            get { return new RelayCommand<string>(x => Logger.Debug(x), x => !string.IsNullOrWhiteSpace(x)); }
        }

        public ICommand IncreaseCommand
        {
            get { return new RelayCommand<string>(x => Increase()); }
        }

        public ICommand AboutCommand
        {
            get { return new NavigationCommand("/Views/AboutPage.xaml"); }
        }

        protected override void OnCreate()
        {
            Logger.Debug("OnCreate");
            SilentLogger.Debug("Write log message to parallel universe :)");
        }

        protected override void OnLoad()
        {
            Logger.Debug("OnLoad");
        }

        protected override void OnOrientationChanged()
        {
            Logger.Debug("OnOrientationChanged");
        }

        private void Increase()
        {
            ShowButton = !ShowButton;
        }
    }
}
