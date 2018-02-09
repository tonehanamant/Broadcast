using System.IO;
using System.Web;
using System.Web.Mvc;
using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public MaintenanceController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        // GET
        public ActionResult Index()
        {
            ViewBag.Message = "Choose File from above and hit submit";
            return View("Index");
        }
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                if (!fileName.EndsWith(".csv"))
                {
                    ViewBag.Message = "Only CSV files supported";
                }
                else
                {
                    var json = _ApplicationServiceFactory.GetApplicationService<IAffidavitService>().JSONifyFile(file.InputStream, fileName);
                    ViewBag.Message = json;
                }
            }

            return View();
        }
    }
}