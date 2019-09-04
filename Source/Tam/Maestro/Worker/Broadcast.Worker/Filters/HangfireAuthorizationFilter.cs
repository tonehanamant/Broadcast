using System;
using System.Configuration;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.Owin;

namespace Broadcast.Worker.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var owinContext = new OwinContext(context.GetOwinEnvironment());

            var header = owinContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(header))
            {
                SetChallengeResponse(owinContext);
                return false;
            }

            var authValues = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(header);

            if (!"Basic".Equals(authValues.Scheme, StringComparison.InvariantCultureIgnoreCase))
            {
                SetChallengeResponse(owinContext);
                return false;
            }

            var parameter = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));
            var parts = parameter.Split(':');

            if (parts.Length < 2)
            {
                SetChallengeResponse(owinContext);
                return false;
            }

            var username = parts[0];
            var password = parts[1];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                SetChallengeResponse(owinContext);
                return false;
            }

            if (username == ConfigurationManager.AppSettings["HangfireAdminUsername"] && password == ConfigurationManager.AppSettings["HangfireAdminPassword"])
            {
                return true;
            }

            SetChallengeResponse(owinContext);
            return false;
        }

        private void SetChallengeResponse(OwinContext httpContext)
        {
            httpContext.Response.StatusCode = 401;
            httpContext.Response.Headers.Add("WWW-Authenticate", new[] { "Basic realm=\"Hangfire Dashboard\"" });
            httpContext.Response.Write("Authentication is required.");
        }
    }
}
