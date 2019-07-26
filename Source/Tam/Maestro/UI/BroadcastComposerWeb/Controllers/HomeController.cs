using System.Web.Mvc;

namespace BroadcastComposerWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("/broadcastreact/campaigns");
        }

        public ActionResult TrackerPrePosting()
        {
            return View("Index");
        }
    }
}
