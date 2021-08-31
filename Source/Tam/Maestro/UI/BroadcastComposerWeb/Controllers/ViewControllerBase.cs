﻿using Services.Broadcast.ApplicationServices;
using System.Web.Mvc;

namespace BroadcastComposerWeb.Controllers
{
    public class ViewControllerBase : Controller
    {
        protected readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public ViewControllerBase(BroadcastApplicationServiceFactory applicationServiceFactory)
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var environmentInfo = _ApplicationServiceFactory.GetApplicationService<IEnvironmentService>().GetEnvironmentInfo();

            ViewBag.DisplayCampaignLink = environmentInfo.DisplayCampaignLink;
            ViewBag.DisplayBuyingLink = environmentInfo.DisplayBuyingLink;
            ViewBag.DisplayAABLink = environmentInfo.EnableAabNavigation;
            ViewBag.DisplayIsciMappingLink = environmentInfo.DisplayIsciMappingLink;

            base.OnActionExecuting(filterContext);
        }
    }
}