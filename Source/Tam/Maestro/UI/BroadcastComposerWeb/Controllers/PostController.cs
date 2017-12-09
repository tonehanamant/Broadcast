using System.Web.Mvc;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Security;

namespace BroadcastComposerWeb.Controllers
{
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class LegacyPostController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}