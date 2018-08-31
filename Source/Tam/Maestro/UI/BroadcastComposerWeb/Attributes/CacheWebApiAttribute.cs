using System;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace BroadcastComposerWeb.Attributes
{
    public class CacheWebApiAttribute : ActionFilterAttribute
    {
        public int Duration { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext filterContext)
        {
            if (filterContext.Response?.Headers != null)
            {
                filterContext.Response.Headers.CacheControl = new CacheControlHeaderValue()
                {
                    MaxAge = TimeSpan.FromMinutes(Duration),
                    MustRevalidate = true,
                    Private = true
                };
            }
        }
    }
}
