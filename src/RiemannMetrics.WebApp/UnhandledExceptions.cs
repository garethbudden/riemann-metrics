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
            var exception = HttpContext.Current.Server.GetLastError();
            if (exception == null)
                return;

            SendEvent("ok",             // state
                "Unhandled exceptions", // description
                1);                     // metric
        }
    }
}