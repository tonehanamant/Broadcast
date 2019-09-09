using System.Web.Mvc;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Security;

namespace BroadcastComposerWeb.Controllers
{
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class TrafficController : ViewControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}