using System;
using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Web.Mvc;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class TrafficController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}