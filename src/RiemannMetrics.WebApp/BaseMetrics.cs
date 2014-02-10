using Riemann;
using System;
using System.Web;

namespace RiemannMetrics.WebApp
{
    public abstract class BaseMetrics : IHttpModule
    {
        protected Client Client { get; private set; }

        public void Init(HttpApplication context)
        {
            if (!Riemann.ShouldMonitor())
                return;

            Client = new Client(Riemann.GetHost(), Riemann.GetPort());
            Register(context);
        }

        public void Dispose()
        {
            if (Client != null)
                Client.Dispose();
        }

        protected void SendEvent(string state, string description, float metric, params string[] tags)
        {
            var evnt = new Event(Riemann.GetService(), state, description, metric);
            evnt.Tags.Add(Environment.MachineName);
            evnt.Tags.Add(GetType().Name);

            Client.SendEvents(new []{ evnt });
        }

        protected abstract void Register(HttpApplication context);
    }
}