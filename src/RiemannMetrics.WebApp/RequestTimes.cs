using System;
using System.Diagnostics;
using System.Web;

namespace RiemannMetrics.WebApp
{
    public class RequestTimes : BaseMetrics
    {
        const string ItemKey = "riemann-requestTime";

        protected override void Register(HttpApplication context)
        {
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
            var context = HttpContext.Current;

            if (context == null)
                return;

            var stopwatch = ((Stopwatch)context.Items[ItemKey]);
            stopwatch.Stop();

            var metric = (float)stopwatch.Elapsed.TotalMilliseconds / 1000;

            SendEvent("request-times",
                    GetState(metric),
                    "Time taken for the server to process the request (does not include queue time, wiretime etc...)",
                    metric,
                    HttpContext.Current.Request.Path);
        }
        
        private static string GetState(float metric)
        {
            if (metric <= 0.3)
                return "ok";

            return metric <= 0.6 ? "warning" : "critical";
        }
    }
}