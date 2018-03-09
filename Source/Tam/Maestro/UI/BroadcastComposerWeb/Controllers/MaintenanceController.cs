using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
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
                    var service = _ApplicationServiceFactory.GetApplicationService<IAffidavitService>();
                    AffidavitSaveRequest request = new AffidavitSaveRequest();
                    var json = service.JSONifyFile(file.InputStream, fileName,out request);

                    ViewBag.Id = service.SaveAffidavit(request, "uma", DateTime.Now);
                    ViewBag.Message = json;

                }
            }

            return View();
        }
    }
}