using Riemann;
using System;
using System.Diagnostics;
using System.Web;

namespace RiemannMetrics.WebApp
{
    public class RequestTimes : IHttpModule
    {
        const string ItemKey = "riemann-requestTime";

        public void Init(HttpApplication context)
        {
            if (!Riemann.ShouldMonitor())
                return;

            context.BeginRequest += context_BeginRequest;
            context.EndRequest += context_EndRequest;
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            var stopwatch = new Stopwatch();
            HttpContext.Current.Items[ItemKey] = stopwatch;
            stopwatch.Start();
        }

        void context_EndRequest(object sender, EventArgs e)
        {
            using (var client = new Client(Riemann.GetHost(), Riemann.GetPort()))
            {
                var stopwatch = ((Stopwatch)HttpContext.Current.Items[ItemKey]);
                stopwatch.Stop();
                var metric = (float)stopwatch.Elapsed.TotalMilliseconds / 1000;

                var evnt = new Event(Riemann.GetService(), GetState(metric),
                                    "Time taken for the server to process the request (does not include queue time, wiretime etc...)",
                                    metric);
                evnt.Tags.Add("request-time");
                evnt.Tags.Add(Environment.MachineName);
                evnt.Tags.Add(HttpContext.Current.Request.Path);

                client.SendEvents(new[] { evnt });
            }
        }

        public void Dispose()
        {
        }

        private string GetState(float metric)
        {
            if (metric <= 0.3)
                return "ok";

            if (metric <= 0.6)
                return "warning";

            return "critical";
        }
    }
}