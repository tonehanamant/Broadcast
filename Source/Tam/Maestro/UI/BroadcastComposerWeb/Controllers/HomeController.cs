using System.Web.Mvc;
using Services.Broadcast.ApplicationServices;

namespace BroadcastComposerWeb.Controllers
{
    public class HomeController : ViewControllerBase
    {
        public HomeController(BroadcastApplicationServiceFactory applicationServiceFactory)
            :base(applicationServiceFactory)
        {
        }

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
