using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Common.Services;
using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Tam.Maestro.Common.SystemComponentParameter;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;
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
        public ActionResult TestEmail(string test_email)
        {
            var quickEmailer = _ApplicationServiceFactory.GetApplicationService<IEmailerService>();
            quickEmailer.QuickSend(true, "<b>test</b><br/> This is only a test", "Test Email from Broadcast",
                MailPriority.Normal, "test@test.com", new string[] {"test_email@test.com"});
            ViewBag.Message = "Test email sent.";
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

        [HttpPost]
        public ActionResult ClearCache()
        {
            ClearSystemParameterCache();
            ViewBag.Message = "Cache cleared, good luck with that!";
            return View("Index");
        }
        [HttpPost]
        public ActionResult ClearMediaMonthCrunchCache()
        {
            var service = _ApplicationServiceFactory.GetApplicationService<IRatingForecastService>();
            service.ClearMediaMonthCrunchCache();

            ViewBag.Message = "Media Month Crunch Cache cleared, good luck!";
            return View("Index");
        }

        public void ClearSystemParameterCache()
        {
            SMSClient.Handler.ClearSystemComponentParameterCache(BroadcastServiceSystemParameterNames.ComponentID, null);
        }

    }
}