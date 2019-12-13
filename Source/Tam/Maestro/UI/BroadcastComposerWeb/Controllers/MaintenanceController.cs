using Common.Services;
using Services.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.Entities;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Security;

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
                    InboundFileSaveRequest request = new InboundFileSaveRequest();
                    var json = service.JSONifyFile(file.InputStream, fileName,out request);

                    ViewBag.Id = service.SaveAffidavit(request, "uma", DateTime.Now);
                    ViewBag.Message = json;

                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult ClearMediaMonthCrunchCache()
        {
            var service = _ApplicationServiceFactory.GetApplicationService<IRatingForecastService>();
            service.ClearMediaMonthCrunchCache();

            ViewBag.Message = "Media Month Crunch Cache cleared, good luck!";
            return View("Index");
        }

        [HttpGet]
        public ActionResult TestFtpAccess()
        {
            FtpService srv = new FtpService();
            WWTVFtpHelper helper = new WWTVFtpHelper(srv);
            NetworkCredential creds = helper.GetClientCredentials();
            var site = "ftp://" + helper.Host;
            var list = srv.GetFileList(creds, site);
        
            ViewBag.Message = "Get file worked w/o error!\r\n";
            return View("Index");
        }

        [HttpGet]
        public ActionResult AlignProposalDaypartsToZeroSeconds()
        {
            var proposalService = _ApplicationServiceFactory.GetApplicationService<IProposalService>();

            var result = proposalService.AlignProposalDaypartsToZeroSeconds();

            ViewBag.Message = result;
            return View("Index");
        }

        [HttpPost]
        public ActionResult RequeueInventoryRatingsJob(int jobId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
                service.RequeueInventoryFileRatingsJob(jobId);
                ViewBag.Message = $"Job '{jobId}' queued.";
            }
            catch (Exception e)
            {
                ViewBag.Message = "Error Processing Job: " + e.Message;
            }

            return View("Index");
        }

        [HttpPost]
        public ActionResult RequeueScxGenerationJob(int jobId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IScxGenerationService>();
                service.RequeueScxGenerationJob(jobId);
                ViewBag.Message = $"Job '{jobId}' queued.";
            }
            catch (Exception e)
            {
                ViewBag.Message = "Error Processing Job: " + e.Message;
            }

            return View("Index");
        }

        [HttpPost]
        public ActionResult ResetRatingsJobStatus(int jobId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
                service.ResetJobStatusToQueued(jobId);
                ViewBag.Message = "Job Status Set to Queued";
            }
            catch (Exception e)
            {
                ViewBag.Message = "Error Resetting Job Status: " + e.Message;
            }

            return View("Index");
        }

        [HttpPost]
        public ActionResult ResetScxGenerationJobStatus(int jobId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IScxGenerationService>();
                service.ResetJobStatusToQueued(jobId);
                ViewBag.Message = "Job Status Set to Queued";
            }
            catch (Exception e)
            {
                ViewBag.Message = "Error Resetting Job Status: " + e.Message;
            }

            return View("Index");
        }

        [HttpPost]
        public ActionResult QueueInventorySourceAggregation(int inventorySourceId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>();
                service.QueueAggregateInventorySummaryDataJob(inventorySourceId);

                ViewBag.Message = $"Job queued for inventory source '{inventorySourceId}'.";
            }
            catch (Exception e)
            {
                ViewBag.Message = "Error Processing Job: " + e.Message;
            }

            return View("Index");
        }

        [HttpPost]
        public ActionResult SwitchProcessRatingsAutomatically()
        {
            TemporalApplicationSettings.ProcessRatingsAutomatically = !TemporalApplicationSettings.ProcessRatingsAutomatically;
            ViewBag.Message = $"ProcessRatingsAutomatically is set to {TemporalApplicationSettings.ProcessRatingsAutomatically}";
            return View("Index");
        }

        [HttpPost]
        [Route("UploadMarketCoverageFile")]
        public ActionResult UploadMarketCoverageFile(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                if (!fileName.EndsWith(".xlsx"))
                {
                    ViewBag.Message = "Only Excel (.xlsx) files supported";
                }
                else
                {
                    try
                    {
                        var service = _ApplicationServiceFactory.GetApplicationService<IMarketService>();
                        service.LoadCoverages(file.InputStream, fileName, "maintenance controller", DateTime.Now);
                        ViewBag.Message = "Market coverages file uploaded and processed successfully !";
                    }
                    catch(Exception ex)
                    {
                        ViewBag.Message = ex.Message;
                    }                                      
                }
            }

            return View("Index");
        }
    }
}