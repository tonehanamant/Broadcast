using System;
using System.Web.Mvc;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Security;

namespace BroadcastComposerWeb.Controllers
{
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    [Obsolete("The controller to retrieve the initial page for the Post Pre Posting section has been deprecated in favor of React")]
    public class PostPrePostingController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}