using Services.Broadcast.ApplicationServices;
using System.Web.Mvc;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace BroadcastComposerWeb.Controllers
{
    public class ViewControllerBase : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.DisplayCampaignLink = BroadcastServiceSystemParameter.DisplayCampaignLink;

            base.OnActionExecuting(filterContext);
        }
    }
}