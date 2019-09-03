using System;
using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.DatabaseDtos;
using Tam.Maestro.Services.Clients;

namespace Tam.Maestro.Services.Cable.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RestrictedAccessAttribute : AuthorizeAttribute
    {
        public RoleType RequiredRole { get; set; }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (ConfigurationManager.AppSettings["DisableSecurity"] == "true")
            {
                return true;
            }

            //Block Anonomouse Authentication.
            if (HttpContext.Current.Request.IsAuthenticated == false)
            {
                return false;
            }

            var ssid = HttpContext.Current.Request.LogonUserIdentity.User.Value;
            AuthenticationEmployee employee = SMSClient.Handler.GetEmployee(ssid, false);
            if (employee != null)
            {
                if (employee.Roles.Contains((int)RequiredRole))
                {
                    return true;
                }
            }
            return false;
        }
    }
}