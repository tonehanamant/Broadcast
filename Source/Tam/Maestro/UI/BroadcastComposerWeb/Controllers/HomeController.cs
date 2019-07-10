using System.Web.Mvc;

namespace BroadcastComposerWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "broadcastreact/inventory");
        }

        public ActionResult TrackerPrePosting()
        {
            return View("Index");
        }
    }
}
