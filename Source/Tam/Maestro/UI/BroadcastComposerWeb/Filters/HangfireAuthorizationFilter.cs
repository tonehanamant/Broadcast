using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace BroadcastComposerWeb.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = HttpContext.Current;

            return httpContext.User.Identity.IsAuthenticated;
        }

    }
}