#region

using Sample.ViewModels;
using Sample.Views;
using Vermeil;
using Vermeil.Logging;

#endregion

namespace Sample
{
    public class SampleBootstrapper : Bootstrapper
    {
        protected override void Init()
        {
            Container.Register<ILogger, DebugLogger>();

            ViewModelMap.Register<MainPage, MainPageViewModel>();
            Container.Resolve<ILogger>().Debug("Init complete");
        }
    }
}