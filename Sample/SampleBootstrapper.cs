#region

using Vermeil;
using Vermeil.Core.Logging;

#endregion

namespace Sample
{
    public class SampleBootstrapper : Bootstrapper
    {
        protected override void Init()
        {
            Container.Register<ILogger, DebugLogger>();
            Container.Resolve<ILogger>().Debug("Init complete");
        }
    }
}
