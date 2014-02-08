using Riemann;
using System;
using System.Web;

namespace RiemannMetrics.WebApp
{
    public class UnhandledExceptions : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            if (!Riemann.ShouldMonitor())
                return;

            context.Error += context_Error;
        }

        void context_Error(object sender, EventArgs e)
        {
            var exception = HttpContext.Current.Server.GetLastError();
            if (exception == null)
                return;

            using (var client = new Client(Riemann.GetHost(), Riemann.GetPort()))
            {
                var evnt = new Event(Riemann.GetService(), "ok", exception.ToString(), 1);
                evnt.Tags.Add("exception");
                evnt.Tags.Add(Environment.MachineName);
                evnt.Tags.Add(exception.GetType().FullName);

                client.SendEvents(new[] { evnt });
            }
        }

        public void Dispose()
        {
        }
    }
}