using System.Web.Mvc;

namespace BroadcastComposerWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "CMW Broadcast";

            return View();
        }
    }
}
