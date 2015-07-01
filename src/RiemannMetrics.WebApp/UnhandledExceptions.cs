using System;
using System.Web;

namespace RiemannMetrics.WebApp
{
    public class UnhandledExceptions : BaseMetrics
    {
        protected override void Register(HttpApplication context)
        {
            context.Error += context_Error;
        }

        void context_Error(object sender, EventArgs e)
        {
            var context = HttpContext.Current;

            if(context == null)
                return;

            var exception = context.Server.GetLastError();

            if (exception == null)
                return;

            SendEvent("unhandled-exceptions",   // appended to servicename => "{serviceName} unhandled-exceptions"
                      "ok",                     // state
                      "Unhandled exceptions",   // description
                      1);                       // metric
        }
    }
}