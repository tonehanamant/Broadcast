using System.Web.Mvc;
using Services.Broadcast.ApplicationServices;

namespace BroadcastComposerWeb.Controllers
{
    public class HomeController : ViewControllerBase
    {
        public ActionResult Index()
        {
            return Redirect("/broadcastreact/inventory");
        }

        public ActionResult TrackerPrePosting()
        {
            return View("Index");
        }
    }
}
