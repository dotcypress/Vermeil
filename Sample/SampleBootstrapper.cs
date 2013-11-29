#region

using System.Windows;
using Vermeil;
using Vermeil.Core;
using Vermeil.Core.Logging;
using Vermeil.Notify;

#endregion

namespace Sample
{
    public class SampleBootstrapper : Bootstrapper
    {
        protected override void Init()
        {
            Container.Register<ILogger, NullLogger>("silent");
            Container.Register<ILogger, DebugLogger>();
            Container.Resolve<ILogger>().Debug("Init complete");
            Container.RegisterInstance((IProgressIndicatorService)Application.Current.Resources["IndicatorService"]);
            ShowAlignmentGrid = true;
        }
    }
}
