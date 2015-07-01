using Riemann;
using System;
using System.Web;

namespace RiemannMetrics.WebApp
{
    public abstract class BaseMetrics : IHttpModule
    {
        private string _serviceName;

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

        protected void SendEvent(string appendServiceString, string state, string description, float metric, params string[] tags)
        {
            if (_serviceName == null)
                _serviceName = String.Concat(Riemann.GetService(), " ", appendServiceString);

            var evnt = new Event(_serviceName, state, description, metric);
            evnt.Tags.Add(Environment.MachineName);
            evnt.Tags.Add(GetType().Name);

            Client.SendEvents(new []{ evnt });
        }

        protected abstract void Register(HttpApplication context);
    }
}