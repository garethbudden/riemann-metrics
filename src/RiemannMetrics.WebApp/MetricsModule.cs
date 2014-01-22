﻿using Riemann;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Web.Hosting;

namespace RiemannMetrics.WebApp
{
    public class MetricsModule : IHttpModule
    {
        public void Dispose()
        {
        }

        const string ItemKey = "riemann-requestTime";

        public void Init(HttpApplication context)
        {
            var host = ConfigurationManager.AppSettings["riemann-host"];
            ushort port = 0;

            if (!UInt16.TryParse(ConfigurationManager.AppSettings["riemann-port"], out port))
                port = 5555;

            if (String.IsNullOrWhiteSpace(host))
                return;

            context.BeginRequest += (s, e) =>
            {
                var stopwatch = new Stopwatch();
                context.Context.Items[ItemKey] = stopwatch;
                stopwatch.Start();
            };

            context.EndRequest += (s, e) =>
            {
                using (var client = new Client(host, port))
                {
                    var stopwatch = ((Stopwatch)context.Context.Items[ItemKey]);
                    stopwatch.Stop();
                    var metric = (float)stopwatch.Elapsed.TotalMilliseconds / 1000;

                    var service = String.Concat(HostingEnvironment.SiteName,
                                                HostingEnvironment.ApplicationVirtualPath == "/"
                                                        ? ""
                                                        : HostingEnvironment.ApplicationVirtualPath);

                    var evnt = new Event(service, GetState(metric),
                                        "Time taken for the server to process the request (does not include queue time, wiretime etc...)",
                                        metric);
                    evnt.Tags.Add("request-time");
                    evnt.Tags.Add(context.Request.Path);

                    client.SendEvents(new[] { evnt });
                }
            };
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