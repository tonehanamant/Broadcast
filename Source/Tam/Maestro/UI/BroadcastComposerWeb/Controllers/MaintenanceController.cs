using Common.Services;
using Newtonsoft.Json;
using Services.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Helpers;
using Services.Broadcast.ApplicationServices.Maintenance;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using System;
using System.IO;
using System.Linq;
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

        protected string _GetCurrentUserFullName() =>
            _ApplicationServiceFactory.GetApplicationService<IUserService>().GetCurrentUserFullName();

        // GET
        public ActionResult Index()
        {
            if (TempData["Message"] != null) {
                ViewBag.Message = TempData["Message"];
            }
            if (TempData["TabId"] != null) {
                ViewBag.TabId = TempData["TabId"];
            }

            if (TempData["Id"] != null) {
                ViewBag.Id = TempData["Id"];
            }
            if (TempData["TextareaMessage "] != null) {
                ViewBag.TextareaMessage = TempData["TextareaMessage"];
            }
            return View("Index");
        }

        [HttpPost]
        public ActionResult TestEmail(string test_email)
        {
            var quickEmailer = _ApplicationServiceFactory.GetApplicationService<IEmailerService>();
            quickEmailer.QuickSend(true, "<b>test</b><br/> This is only a test", "Test Email from Broadcast",
                MailPriority.Normal, new string[] {"test_email@test.com"});
            TempData["Message"] = "Test email sent.";
            TempData["TabId"] = "settings";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                if (!fileName.EndsWith(".csv"))
                {
                    TempData["Message"] = "Only CSV files supported";
                }
                else
                {
                    var service = _ApplicationServiceFactory.GetApplicationService<IAffidavitService>();
                    InboundFileSaveRequest request = new InboundFileSaveRequest();
                    var json = service.JSONifyFile(file.InputStream, fileName,out request);

                    TempData["Id"] = service.SaveAffidavit(request, "uma", DateTime.Now);
                    TempData["TextareaMessage"] = json;

                }
            }

            TempData["TabId"] = "posting";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("ImportStationMappings")]
        public ActionResult ImportStationMappings(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                if (!fileName.EndsWith(".xlsx"))
                {
                    TempData["Message"] = "Only Excel (.xlsx) files supported";
                }
                else
                {
                    try
                    {
                        var service = _ApplicationServiceFactory.GetApplicationService<IStationMappingService>();
                        service.LoadStationMappings(file.InputStream, fileName, "maintenance controller", DateTime.Now);
                        TempData["Message"] = "Station mappings file uploaded and processed successfully !";
                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = ex.Message;
                    }
                }
            }

            TempData["TabId"] = "reference_data";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("ImportInventoryProgramsResults")]
        public ActionResult ImportInventoryProgramsResults(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                if (!fileName.EndsWith(".csv"))
                {
                    TempData["Message"] = "Only CSV (.csv) files supported";
                }
                else
                {
                    try
                    {
                        var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
                        var result = service.ImportInventoryProgramsResults(file.InputStream, fileName);
                        TempData["TextareaMessage"] = result;
                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = ex.Message;
                    }
                }
            }

            TempData["TabId"] = "reference_data";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public ActionResult ClearMediaMonthCrunchCache()
        {
            var service = _ApplicationServiceFactory.GetApplicationService<IRatingForecastService>();
            service.ClearMediaMonthCrunchCache();

            TempData["Message"] = "Media Month Crunch Cache cleared, good luck!";
            TempData["TabId"] = "settings";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult TestFtpAccess()
        {
            FtpService srv = new FtpService();
            WWTVFtpHelper helper = new WWTVFtpHelper(srv);
            NetworkCredential creds = helper.GetClientCredentials();
            var site = "ftp://" + helper.Host;
            var list = srv.GetFileList(creds, site);
        
            TempData["Message"] = "Get file worked w/o error!\r\n";
            TempData["TabId"] = "settings";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult AlignProposalDaypartsToZeroSeconds()
        {
            var proposalService = _ApplicationServiceFactory.GetApplicationService<IProposalService>();

            var result = proposalService.AlignProposalDaypartsToZeroSeconds();

            TempData["Message"] = result;
            TempData["TabId"] = "inventory";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RequeueInventoryRatingsJob(int jobId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
                service.RequeueInventoryFileRatingsJob(jobId);
                TempData["Message"] = $"Job '{jobId}' queued.";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Processing Job: " + e.Message;
            }
            TempData["TabId"] = "inventory";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult QueueInventoryProgramsFileJob(int fileId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
                var result = service.QueueProcessInventoryProgramsByFileJob(fileId, _GetCurrentUserFullName());

                TempData["Message"] = $"Job queued as Job {result.Job.Id}. \r\n{result}";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Processing Job: " + e.Message;
            }

            TempData["TabId"] = "program_guide";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult QueInventoryProgramsJobByFileName(string fileName)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
                var result = service.QueueProcessInventoryProgramsByFileJobByFileName(fileName, _GetCurrentUserFullName());

                TempData["Message"] = $"Job queued as Job {result.Job.Id}. \r\n{result}";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Processing Job: " + e.Message;
            }

            TempData["TabId"] = "program_guide";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RequeueInventoryProgramsByFileJob(int jobId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
                var result = service.ReQueueProcessInventoryProgramsByFileJob(jobId, _GetCurrentUserFullName());

                TempData["Message"] = $"Job '{jobId}' requeued as new Job {result.Job.Id}. \r\n{result}";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Processing Job: " + e.Message;
            }
            TempData["TabId"] = "program_guide";
            return RedirectToAction("Index");
        }

        public DateTime ParseDateTimeString(string dateTimeString, string fieldName)
        {
            if (DateTime.TryParse(dateTimeString, out var result))
            {
                return result;
            }
            throw new InvalidOperationException($"'{fieldName}' was not in correct format.  Try 'yyyy/MM/dd'.");
        }

        [HttpPost]
        public ActionResult QueueInventoryProgramsSourceJob(int inventorySourceId, string startDateString, string endDateString)
        {
            try
            {
                var startDate = ParseDateTimeString(startDateString, "StartDate");
                var endDate = ParseDateTimeString(endDateString, "EndDate");

                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
                var result = service.QueueProcessInventoryProgramsBySourceJob(inventorySourceId, startDate, endDate,_GetCurrentUserFullName());

                TempData["Message"] = $"Job queued as Job {result.Jobs.First().Id}. \r\n{result}";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Processing Job: " + e.Message;
            }

            TempData["TabId"] = "program_guide";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult QueueProcessInventoryProgramsBySourceForWeeks(string orientByDateString)
        {
            try
            {
                var orientByDate = ParseDateTimeString(orientByDateString, "OrientByDate");

                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
                var result = service.QueueProcessInventoryProgramsBySourceForWeeksFromDate(orientByDate, _GetCurrentUserFullName());

                TempData["Message"] = $"Job queued as Job {result.Jobs.First().Id}. \r\n{result}";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Processing Job: " + e.Message;
            }

            TempData["TabId"] = "program_guide";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RequeueInventoryProgramsSourceJob(int jobId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
                var result = service.ReQueueProcessInventoryProgramsBySourceJob(jobId, _GetCurrentUserFullName());

                TempData["Message"] = $"Job '{jobId}' requeued as new Job {result.Jobs.First().Id}.";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Processing Job: " + e.Message;
            }

            TempData["TabId"] = "program_guide";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult QueueProcessInventoryProgramsBySourceJobUnprocessedBetweenDates(int inventorySourceId, string startDateString, string endDateString)
        {
            try
            {
                var startDate = ParseDateTimeString(startDateString, "StartDate");
                var endDate = ParseDateTimeString(endDateString, "EndDate");

                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
                var result = service.QueueProcessInventoryProgramsBySourceJobUnprocessed(inventorySourceId, startDate, endDate, _GetCurrentUserFullName());

                TempData["Message"] = $"Job queued as Job {result.Jobs.First().Id}. \r\n{result}";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Processing Job: " + e.Message;
            }

            TempData["TabId"] = "program_guide";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RequeueScxGenerationJob(int jobId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IScxGenerationService>();
                service.RequeueScxGenerationJob(jobId);
                TempData["Message"] = $"Job '{jobId}' queued.";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Processing Job: " + e.Message;
            }

            TempData["TabId"] = "inventory";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ResetRatingsJobStatus(int jobId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
                service.ResetJobStatusToQueued(jobId);
                TempData["Message"] = "Job Status Set to Queued";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Resetting Job Status: " + e.Message;
            }

            TempData["TabId"] = "inventory";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ResetScxGenerationJobStatus(int jobId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IScxGenerationService>();
                service.ResetJobStatusToQueued(jobId);
                TempData["Message"] = "Job Status Set to Queued";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Resetting Job Status: " + e.Message;
            }

            TempData["TabId"] = "inventory";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult QueueInventorySourceAggregation(int inventorySourceId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>();
                service.QueueAggregateInventorySummaryDataJob(inventorySourceId);

                TempData["Message"] = $"Job queued for inventory source '{inventorySourceId}'.";
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Processing Job: " + e.Message;
            }


            TempData["TabId"] = "inventory";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ForceCompletePlanPricingJob(int jobId)
        {
            try
            {
                var userName = _ApplicationServiceFactory.GetApplicationService<IUserService>()
                    .GetCurrentUserFullName();
                var service = _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
                var result = service.ForceCompletePlanPricingJob(jobId, userName);

                TempData["Message"] = result;
            }
            catch (Exception e)
            {
                TempData["Message"] = "Error Processing Job: " + e.Message;
            }

            TempData["TabId"] = "planning";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SwitchProcessRatingsAutomatically()
        {
            TemporalApplicationSettings.ProcessRatingsAutomatically = !TemporalApplicationSettings.ProcessRatingsAutomatically;
            TempData["Message"] = $"ProcessRatingsAutomatically is set to {TemporalApplicationSettings.ProcessRatingsAutomatically}";
            TempData["TabId"] = "settings";
            return RedirectToAction("Index");
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
                    TempData["Message"] = "Only Excel (.xlsx) files supported";
                }
                else
                {
                    try
                    {
                        var service = _ApplicationServiceFactory.GetApplicationService<IMarketService>();
                        service.LoadCoverages(file.InputStream, fileName, "maintenance controller", DateTime.Now);
                        TempData["Message"] = "Market coverages file uploaded and processed successfully !";
                    }
                    catch(Exception ex)
                    {
                        TempData["Message"] = ex.Message;
                    }                                      
                }
            }

            TempData["TabId"] = "reference_data";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("UploadNTIUniversesFile")]
        public ActionResult UploadNTIUniversesFile(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                if (!Path.GetFileName(file.FileName).EndsWith(".xlsx"))
                {
                    TempData["Message"] = "Only Excel (.xlsx) files supported";
                }
                else
                {
                    try
                    {
                        var userName = _ApplicationServiceFactory
                            .GetApplicationService<IUserService>()
                            .GetCurrentUserFullName();

                        _ApplicationServiceFactory
                            .GetApplicationService<INtiUniverseService>()
                            .LoadUniverses(file.InputStream, userName, DateTime.Now);

                        TempData["Message"] = "NTI universes file uploaded and processed successfully !";
                    }
                    catch (Exception ex)
                    {
                        TempData["Message"] = ex.Message;
                    }
                }
            }

            TempData["TabId"] = "reference_data";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CleanupDaypartsFilterErroneousDayparts()
        {
            var service = _ApplicationServiceFactory.GetApplicationService<IDaypartCleanupService>();
            var results = service.FilterErroneousDayparts();
            TempData["TextareaMessage"] = JsonConvert.SerializeObject(results, Formatting.Indented);
            TempData["TabId"] = "reference_data";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CleanupDaypartsRepairErroneousDayparts()
        {
            var service = _ApplicationServiceFactory.GetApplicationService<IDaypartCleanupService>();
            var results = service.RepairErroneousDayparts();
            TempData["TextareaMessage"] = JsonConvert.SerializeObject(results, Formatting.Indented);
            TempData["TabId"] = "reference_data";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RerunPlanPricingJob(int jobId)
        {
            var service = _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
            var results = service.ReRunPricingJob(jobId);
            TempData["Message"] = $"Reprocessed job {jobId} as job {results}";
            TempData["TabId"] = "planning";
            return RedirectToAction("Index");
        }
    }
}