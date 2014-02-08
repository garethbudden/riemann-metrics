using System;
using System.Configuration;
using System.Web.Hosting;

namespace RiemannMetrics.WebApp
{
    public static class Riemann
    {
        public static bool ShouldMonitor()
        {
            return !String.IsNullOrWhiteSpace(GetHost());
        }

        public static string GetHost()
        {
            return ConfigurationManager.AppSettings["riemann-host"];
        }

        public static int GetPort()
        {
            ushort port = 0;

            if (!UInt16.TryParse(ConfigurationManager.AppSettings["riemann-port"], out port))
                port = 5555;

            return port;
        }

        public static string GetService()
        {
            return String.Concat(HostingEnvironment.SiteName,
                                HostingEnvironment.ApplicationVirtualPath == "/"
                                        ? ""
                                        : HostingEnvironment.ApplicationVirtualPath);
        }
    }
}