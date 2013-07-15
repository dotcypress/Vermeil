#region

using System;
using System.Linq;
using System.Xml.Linq;

#endregion

namespace Vermeil.Core
{
    public static class CurrentApp
    {
        public static Version GetAppVersion()
        {
            try
            {
                var doc = XDocument.Load("WMAppManifest.xml");
                var xAttribute = doc.Descendants("App").First().Attribute("Version");
                if (xAttribute != null)
                {
                    var version = xAttribute.Value;
                    if (!string.IsNullOrEmpty(version))
                    {
                        Version result;
                        if (Version.TryParse(version, out result))
                        {
                            return result;
                        }
                    }
                }
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
            }
            return default(Version);
        }
    }
}
