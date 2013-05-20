#region

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Vermeil.Cache;
using Vermeil.Core.IoC;
using Vermeil.Core.Logging;
using Vermeil.Core.Messaging;
using Vermeil.Core.Settings;
using Vermeil.MVVM;
using Vermeil.Navigation;
using Vermeil.State;

#endregion

namespace Vermeil
{
    public abstract class Bootstrapper : IApplicationService
    {
        private bool _clearHistory;
        private bool _isFastResume;

        protected Bootstrapper()
        {
            Container = new IocContainer();
            ViewModelMap = new ViewModelMap();
            InitRootFrame();
            InitContainer();
            InitPhoneServices();
            InitMvvm();
            Current = this;
        }

        #region Init

        private void InitRootFrame()
        {
            Container.RegisterInstance(CreateRootFrame());
        }

        private void InitContainer()
        {
            Container.RegisterInstance<PhoneApplicationService>();
            Container.RegisterInstance<ILogger, NullLogger>();
            Container.RegisterInstance<IMessagePublisher, MessagePublisher>();
            Container.RegisterInstance<INavigationManager, NavigationManager>();
            Container.RegisterInstance<ISettingsManager, SettingsManager>();
            Container.RegisterInstance<IStateManager, StateManager>();
            Container.RegisterInstance<IImageCache, ImageCache>();
            Container.RegisterInstance<TombstoneManager>();
        }

        private void InitPhoneServices()
        {
            var phoneApplicationService = Container.Resolve<PhoneApplicationService>();
            phoneApplicationService.Activated += PhoneApplicationServiceActivated;
            phoneApplicationService.Deactivated += PhoneApplicationServiceDeactivated;
            Application.Current.ApplicationLifetimeObjects.Add(phoneApplicationService);
            Application.Current.UnhandledException += (s, e) =>
                {
                    var logger = Container.TryResolve<ILogger>();
                    if (logger != null)
                    {
                        logger.Fatal("Application Unhandled Exception", e.ExceptionObject);
                    }
                    OnApplicationUnhandledException(e);
                };
        }

        private void InitMvvm()
        {
            var frame = Container.Resolve<PhoneApplicationFrame>();
            frame.OrientationChanged += OnOrientationChanged;
        }

        #endregion

        #region IApplicationService Implementation

        public void StartService(ApplicationServiceContext context)
        {
            var frame = Container.Resolve<PhoneApplicationFrame>();
            Application.Current.RootVisual = frame;

            frame.Navigating += FrameNavigating;
            frame.Navigated += FrameNavigated;

            Init();
        }

        public void StopService()
        {
        }

        #endregion

        #region Phone events

        private void PhoneApplicationServiceActivated(object sender, ActivatedEventArgs e)
        {
            OnApplicationActivated();
        }

        private void PhoneApplicationServiceDeactivated(object sender, DeactivatedEventArgs e)
        {
            OnApplicationDeactivated();
        }

        private void FrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (_isFastResume)
            {
                _isFastResume = false;
                var args = new FastResumeArgs(e.Uri);
                OnFastResume(args);

                if (args.ClearHistory)
                {
                    _clearHistory = true;
                }
                else if (args.PreserveLastOpenedPage)
                {
                    e.Cancel = true;
                }
                return;
            }
            if (DesignerProperties.IsInDesignTool || e.NavigationMode == NavigationMode.Back)
            {
                return;
            }

            var model = GetCurrentViewModel();
            if (model == null)
            {
                return;
            }
            var tombstoneManager = Container.Resolve<TombstoneManager>();
            tombstoneManager.SaveState(model);
        }

        private void FrameNavigated(object sender, NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Reset)
            {
                _isFastResume = true;
                return;
            }
            if (DesignerProperties.IsInDesignTool || (e.NavigationMode == NavigationMode.Back && !e.IsNavigationInitiator))
            {
                return;
            }
            if (_clearHistory)
            {
                _clearHistory = false;
                var frame = Container.Resolve<PhoneApplicationFrame>();
                while (frame.RemoveBackEntry() != null)
                {
                }
            }
            var page = e.Content as PhoneApplicationPage;
            if (page == null)
            {
                return;
            }
            page.Loaded -= PageLoaded;
            page.Unloaded -= PageUnloaded;
            var currentContext = page.DataContext as ViewModel;
            if (currentContext != null)
            {
                var tombstoneManager = Container.Resolve<TombstoneManager>();
                tombstoneManager.Clear();
            }
            else
            {
                var viewModelType = ViewModelMap.Resolve(page.GetType());
                var viewModel = Container.TryResolve(viewModelType) as ViewModel;
                if (viewModel == null)
                {
                    return;
                }
                viewModel.RootElement = page;
                page.DataContext = viewModel;
            }
            page.Loaded += PageLoaded;
            page.Unloaded += PageUnloaded;
        }

        #endregion

        #region MVVM

        private ViewModel GetCurrentViewModel()
        {
            var frame = Container.Resolve<PhoneApplicationFrame>();

            var page = frame.Content as PhoneApplicationPage;
            if (page != null)
            {
                return page.DataContext as ViewModel;
            }
            return null;
        }

        private void PageLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var page = sender as PhoneApplicationPage;
            if (page == null)
            {
                return;
            }

            var model = page.DataContext as ViewModel;
            if (model == null)
            {
                return;
            }

            model.Orientation = Container.Resolve<PhoneApplicationFrame>().Orientation;
            BindNavigationParameters(model);

            if (!model.IsCreated)
            {
                model.IsCreated = true;
                model.FireOnCreate();
            }

            model.FireOnLoad();

            var tombstoneManager = Container.Resolve<TombstoneManager>();
            tombstoneManager.LoadState(model);
        }

        private void PageUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var page = sender as PhoneApplicationPage;
            if (page == null)
            {
                return;
            }

            var model = page.DataContext as ViewModel;
            if (model != null)
            {
                model.FireOnUnload();
            }

            page.Loaded -= PageLoaded;
            page.Unloaded -= PageUnloaded;
        }

        private void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            var model = GetCurrentViewModel();
            if (model != null)
            {
                model.Orientation = e.Orientation;
            }
        }

        private void BindNavigationParameters(ViewModel model)
        {
            var injectors = model.GetType().
                                  GetProperties().
                                  Select(x => new {Property = x, Attribute = (NavigationParamAttribute) x.GetCustomAttributes(typeof (NavigationParamAttribute), false).FirstOrDefault()}).
                                  Where(x => x.Attribute != null);
            foreach (var injector in injectors)
            {
                var propertyValue = GetParameter(injector.Property.PropertyType, injector.Attribute);
                injector.Property.SetValue(model, propertyValue, null);
            }
        }

        private object GetParameter(Type parameterType, NavigationParamAttribute attribute)
        {
            var navigationManager = Container.Resolve<INavigationManager>();
            var parameter = navigationManager.GetQueryParameter(attribute.Name);
            if (parameterType == typeof (int))
            {
                int number;
                if (!int.TryParse(parameter, out number) && attribute.IsMandatory)
                {
                    throw new Exception(string.Format("Navigation parameter '{0}' is empty", attribute.Name));
                }
                return number;
            }
            if (parameterType == typeof (Guid))
            {
                Guid guid;
                if (!Guid.TryParse(parameter, out guid) && attribute.IsMandatory)
                {
                    throw new Exception(string.Format("Navigation parameter '{0}' is empty", attribute.Name));
                }
                return guid;
            }
            if (parameterType == typeof (bool))
            {
                bool result;
                if (!bool.TryParse(parameter, out result) && attribute.IsMandatory)
                {
                    throw new Exception(string.Format("Navigation parameter '{0}' is empty", attribute.Name));
                }
                return result;
            }
            return parameter;
        }

        #endregion

        #region Properties

        public static Bootstrapper Current { get; private set; }

        public IocContainer Container { get; private set; }

        public ViewModelMap ViewModelMap { get; private set; }

        #endregion

        #region Virtual members

        protected virtual void Init()
        {
        }

        protected virtual void OnFastResume(FastResumeArgs args)
        {
        }

        protected virtual PhoneApplicationFrame CreateRootFrame()
        {
            return new PhoneApplicationFrame();
        }

        protected virtual void OnApplicationActivated()
        {
        }

        protected virtual void OnApplicationDeactivated()
        {
        }

        protected virtual void OnApplicationUnhandledException(ApplicationUnhandledExceptionEventArgs applicationUnhandledExceptionEventArgs)
        {
        }

        #endregion
    }
}
