using Common.Services;
using Common.Services.Repositories;
using Newtonsoft.Json;
using Services.Broadcast;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Maintenance;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Repositories;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Security;
using Unity;

namespace BroadcastComposerWeb.Controllers
{
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class MaintenanceController : Controller
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        
        public MaintenanceController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
        {
            _ApplicationServiceFactory = applicationServiceFactory;
            _BroadcastDataRepositoryFactory = BroadcastApplicationServiceFactory.Instance.Resolve<IDataRepositoryFactory>();
        }

        protected string _GetCurrentUserFullName() =>
            _ApplicationServiceFactory.GetApplicationService<IUserService>().GetCurrentUserFullName();

        // GET
        public ActionResult Index()
        {
            if (TempData["Message"] != null)
            {
                ViewBag.Message = TempData["Message"];
            }
            if (TempData["TabId"] != null)
            {
                ViewBag.TabId = TempData["TabId"];
            }

            if (TempData["Id"] != null)
            {
                ViewBag.Id = TempData["Id"];
            }
            if (TempData["TextareaMessage"] != null)
            {
                ViewBag.TextareaMessage = TempData["TextareaMessage"];
            }
            return View("Index");
        }

        [HttpPost]
        public ActionResult TestEmail(string test_email)
        {
            var quickEmailer = _ApplicationServiceFactory.GetApplicationService<IEmailerService>();
            quickEmailer.QuickSend(true, "<b>test</b><br/> This is only a test", "Test Email from Broadcast",
                MailPriority.Normal, new string[] { "test_email@test.com" });
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
                    var json = service.JSONifyFile(file.InputStream, fileName, out request);

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

        public ActionResult PerformImportStationsFromForecastDatabase()
        {
            try
            {
                var username = _GetCurrentUserFullName();
                var service = _ApplicationServiceFactory.GetApplicationService<IStationService>();
                service.ImportStationsFromForecastDatabaseJobEntryPoint(username);
                TempData["Message"] = "Stations Etl completed.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            TempData["TabId"] = "reference_data";
            return RedirectToAction("Index");
        }

        public ActionResult ResetLastStationsFromForecastDatabaseJob()
        {
            try
            {
                var username = _GetCurrentUserFullName();
                var service = _ApplicationServiceFactory.GetApplicationService<IStationService>();
                var deletedCount = service.ResetLastStationsFromForecastDatabaseJob(username);
                TempData["Message"] = $"Reset completed.  Deleted {deletedCount} records.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            TempData["TabId"] = "reference_data";
            return RedirectToAction("Index");
        }
        public ActionResult ExportMissingStations()
        {
            try
            {               
                var currentDate = DateTime.Now.ToString("yyyyMMdd");
                var service = _ApplicationServiceFactory.GetApplicationService<IStationService>();
                MemoryStream memoryStream= service.GetStationsMissingMarkets();
                TempData["Message"] = $"Export Completed.";              
                
                byte[] bytesInStream = memoryStream.ToArray(); 
                memoryStream.Close();

                Response.Clear();
                Response.ContentType = "application/force-download";
                Response.AddHeader("content-disposition", $"attachment;    filename=MissingStationsReport_{currentDate}.xlsx");
                Response.BinaryWrite(bytesInStream);
                Response.End();
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            TempData["TabId"] = "reference_data";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("ImportProgramMappings")]
        public ActionResult ImportProgramMappings(HttpPostedFileBase file)
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
                        var service = _ApplicationServiceFactory.GetApplicationService<IProgramMappingService>();
                        var jobId = service.LoadProgramMappings(file.InputStream, fileName, "maintenance controller", DateTime.Now);
                        TempData["Message"] = $"Program mappings file uploaded and a background job with id {jobId} was created!";
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
        [Route("ExportProgramMappings")]
        public ActionResult ExportProgramMappings()
        {
            try
            {
                var userName = _ApplicationServiceFactory.GetApplicationService<IUserService>().GetCurrentUserFullName();
                var programMappingService = _ApplicationServiceFactory.GetApplicationService<IProgramMappingService>();
                var exportedFile = programMappingService.ExportProgramMappingsFile(userName);

                TempData["Message"] = $"Downloaded file {exportedFile.Filename}";

                return File(exportedFile.Stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", exportedFile.Filename);
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            TempData["TabId"] = "reference_data";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("ExportUnmappedProgramNames")]
        public ActionResult ExportUnmappedProgramNames()
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IProgramMappingService>();
                var result = service.GenerateUnmappedProgramNameReport();
                return File(result.Stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    result.Filename);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Index");
            }
        }

        [HttpPost]
        [Route("ExportUnmappedPrograms")]
        public ActionResult ExportUnmappedPrograms()
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IProgramMappingService>();
                var result = service.ExportUnmappedPrograms();
                return File(result.Stream, "application/zip", result.Filename);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Index");
            }
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
                const bool waitToComplete = false;

                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
                service.RequeueInventoryFileRatingsJob(jobId, waitToComplete);
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
        public ActionResult RequeueInventoryRatingsJobAndWait(int jobId)
        {
            try
            {
                const bool waitToComplete = true;

                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryRatingsProcessingService>();
                service.RequeueInventoryFileRatingsJob(jobId, waitToComplete);
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
                var result = service.QueueProcessInventoryProgramsBySourceJob(inventorySourceId, startDate, endDate, _GetCurrentUserFullName());

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
        public ActionResult ComparePlanVersionForShouldPromotePricing(int planId, int beforeVersionId, int afterVersionId)
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IPlanService>();
                var result = service.ComparePlanVersionForShouldPromotePricing(planId, beforeVersionId, afterVersionId);

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
        public async Task<ActionResult> RerunPlanPricingJob(int jobId)
        {
            var service = _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
            var results = await service.ReRunPricingJobAsync(jobId);
            TempData["Message"] = $"Reprocessed job {jobId} as job {results}";
            TempData["TabId"] = "planning";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<ActionResult> RerunPlanBuyingJob(int jobId)
        {
            var service = _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();
            var results = await service.ReRunBuyingJobAsync(jobId);
            TempData["Message"] = $"Reprocessed job {jobId} as job {results}";
            TempData["TabId"] = "planning";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ForceCompletePlanBuyingJob(int jobId)
        {
            try
            {
                var userName = _ApplicationServiceFactory.GetApplicationService<IUserService>()
                    .GetCurrentUserFullName();
                var service = _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();
                var result = service.ForceCompletePlanBuyingJob(jobId, userName);

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
        public ActionResult GeneratePricingResultsReportQuality(int planId)
        {
            try
            {
                var appDataPath = HttpContext.Server.MapPath("~/App_Data");
                var service = _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
                var result = service.GeneratePricingResultsReport(planId, SpotAllocationModelMode.Quality, appDataPath);

                return File(result.Stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Filename);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Index");
            }
        }

        [HttpPost]
        public ActionResult GeneratePricingResultsReportEfficiency(int planId)
        {
            try
            {
                var appDataPath = HttpContext.Server.MapPath("~/App_Data");
                var service = _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
                var result = service.GeneratePricingResultsReport(planId,SpotAllocationModelMode.Efficiency ,appDataPath);

                return File(result.Stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Filename);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Index");
            }
        }

        [HttpPost]
        public ActionResult GeneratePricingResultsReportFloor(int planId)
        {
            try
            {
                var appDataPath = HttpContext.Server.MapPath("~/App_Data");
                var service = _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
                var result = service.GeneratePricingResultsReport(planId, SpotAllocationModelMode.Floor, appDataPath);

                return File(result.Stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Filename);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Index");
            }
        }

        [HttpPost]
        public ActionResult GenerateBuyingResultsReportEfficiency(int planId)
        {
            try
            {
                var appDataPath = HttpContext.Server.MapPath("~/App_Data");
                var service = _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();
                var result = service.GenerateBuyingResultsReport(planId, planVersionNumber: null, appDataPath, SpotAllocationModelMode.Efficiency, PostingTypeEnum.NSI);

                return File(result.Stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Filename);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Index");
            }
        }

        [HttpPost]
        public ActionResult GenerateBuyingResultsReportFloor(int planId)
        {
            try
            {
                var appDataPath = HttpContext.Server.MapPath("~/App_Data");
                var service = _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();
                var result = service.GenerateBuyingResultsReport(planId, planVersionNumber: null, appDataPath, SpotAllocationModelMode.Floor, PostingTypeEnum.NSI);

                return File(result.Stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.Filename);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View("Index");
            }
        }

        [HttpPost]
        public ActionResult UploadVpvhFile(HttpPostedFileBase file)
        {
            TempData["TabId"] = "reference_data";

            if (file == null || file.ContentLength <= 0)
                return RedirectToAction("Index");

            if (Path.GetExtension(file.FileName) != ".xlsx")
            {
                TempData["Message"] = "Only Excel (.xlsx) files supported";
                return RedirectToAction("Index");
            }

            try
            {
                var userName = _ApplicationServiceFactory
                    .GetApplicationService<IUserService>()
                    .GetCurrentUserFullName();

                _ApplicationServiceFactory
                    .GetApplicationService<IVpvhService>()
                    .LoadVpvhs(file.InputStream, file.FileName, userName, DateTime.Now);

                TempData["Message"] = "VPVH file uploaded and processed successfully !";
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }


            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RunIsciIngestJob()
        {
            var username = _GetCurrentUserFullName();
            var service = _ApplicationServiceFactory.GetApplicationService<IReelIsciIngestService>();
            service.Queue(username);
            return RedirectToAction("Index");
        }        

        [HttpGet]
        public ActionResult ExportVpvh()
        {
            try
            {
                var excelFile = _ApplicationServiceFactory
                        .GetApplicationService<IVpvhService>().Export();

                var contentDisposition = new System.Net.Mime.ContentDisposition
                {
                    FileName = "VpvhExport.xlsx",
                    Inline = false,
                };
                Response.AppendHeader("Content-Disposition", contentDisposition.ToString());
                return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult HTMLDecodeProgramNames(string protectionKey)
        {
            try
            {
                if (protectionKey == "q1w2e3r4")
                {
                    _BroadcastDataRepositoryFactory
                        .GetDataRepository<IDataMaintenanceRepository>()
                        .HtmlDecodeProgramNames();

                    ViewBag.Message = "Program names were successfully decoded";
                }
                else
                {
                    ViewBag.Message = "Invalid protection key";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View("Index");
        }

        [HttpPost]
        public ActionResult DeleteSavedBuyingData(string protectionKey)
        {            
            try
            {
                if (protectionKey == "r1f2m3p4")
                {
                    var service = _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>();
                    service.DeleteSavedBuyingData();

                    ViewBag.Message = "Buying Data was successfully deleted.";
                }
                else
                {
                    ViewBag.Message = "Invalid protection key";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View("Index");
        }

        [HttpPost]
        public ActionResult QueueRepairInventoryProgramsJob()
        {
            TempData["TabId"] = "program_guide";
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
                var jobId = service.QueueRepairInventoryProgramsJob();

                TempData["Message"] = $"Repair Inventory Programs processed queued with Id : '{jobId}'";
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CancelQueuedRepairInventoryProgramsJob(int hangfireJobId)
        {
            TempData["TabId"] = "program_guide";
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
                var resultMessage = service.CancelQueueRepairInventoryProgramsJob(hangfireJobId);

                TempData["Message"] = resultMessage;
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ExecuteRepairInventoryPrograms()
        {
            TempData["TabId"] = "program_guide";
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProgramsProcessingService>();
                service.PerformRepairInventoryPrograms(CancellationToken.None);

                TempData["Message"] = $"Repair Inventory Programs processed Completed.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CleanupOrphanedProgramNameMappings()
        {
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IProgramMappingService>();
                var result = service.CleanupOrphanedProgramNameMappings();

                ViewBag.Message = $"Cleaned up '{result}' orphaned program name mappings records.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            TempData["Message"] = ViewBag.Message;
            TempData["TabId"] = "programs";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ClearProgramList(string protectionKey)
        {
            try
            {
                if (protectionKey == "r1f2m3p4")
                {
                    var service = _ApplicationServiceFactory.GetApplicationService<IProgramMappingService>();
                    var result = service.DeletePrograms();
                    ViewBag.Message = $"Deleted '{result}' programs from the table.";
                }
                else
                {
                    ViewBag.Message = "Invalid protection key";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            TempData["Message"] = ViewBag.Message;
            TempData["TabId"] = "programs";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("ImportMasterList")]
        public ActionResult ImportMasterList(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                try
                {
                    var environmentService = _ApplicationServiceFactory.GetApplicationService<IEnvironmentService>();
                    var appFolderPath = environmentService.GetBroadcastAppFolderPath();
                    var fileService = _ApplicationServiceFactory.GetApplicationService<IFileService>();
                    const string dirName = "ProgramMappingMasterList";
                    
                    var completePath = Path.Combine(appFolderPath, dirName);
                    if (!fileService.DirectoryExists(completePath))
                        fileService.CreateDirectory(completePath);
                    var pathWithFile = Path.Combine(completePath, fileName);
                    if (fileService.Exists(pathWithFile))
                        fileService.Delete(pathWithFile);
                    fileService.Create(pathWithFile, file.InputStream);
                }
                catch (Exception exception)
                {
                    TempData["Message"] = exception.Message;

                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["Message"] = "Invalid file";

                return RedirectToAction("Index");
            }

            TempData["Message"] = "Upload successful!";

            return RedirectToAction("Index");
        }

        /* SDE 11/7/2022 BP-5955
         * Removing UploadProgramsFromBroadcastOps because we don't want to do UploadProgramsFromBroadcastOps anymore.
         * We may want to bring UploadProgramsFromBroadcastOps back in the future.
         * 
         * UploadProgramsFromBroadcastOps loads a static RedBee Schedule into the programs table.  
         * We know now that we want to orient off the Business's Programs-Genre Excel file which is loaded by the UploadProgramList below.
         *          
         * So we are hiding UploadProgramsFromBroadcastOps for now and enabling only UploadProgramList.
         * 
         * In the future we are looking at maybe scraping the RedBee Programs Schedule for a list of new programs.
         * The static RedBee Schedule file is a static snapshot of that feed.
         * 
         * If we do the feed we may we want to bring this back.

        [HttpPost]
        [Route("UploadProgramsFromRedBeeScheduleFile")]
        public ActionResult UploadProgramsFromBroadcastOps(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                if (!fileName.EndsWith(".txt"))
                {
                    TempData["Message"] = "Only Text (.txt) files supported";
                }
                else
                {
                    try
                    {
                        var service = _ApplicationServiceFactory.GetApplicationService<IProgramMappingService>();
                        service.UploadProgramsFromBroadcastOps(file.InputStream, fileName, "maintenance controller", DateTime.Now);
                        TempData["Message"] = $"Master file Uploaded Successfully";
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

        */

        [HttpPost]
        [Route("UploadProgramList")]
        public ActionResult UploadProgramList(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                if (!fileName.EndsWith(".xlsx"))
                {
                    TempData["ProgramMessage"] = "Only Excel (.xlsx) files supported";
                }
                else
                {
                    try
                    {
                        var service = _ApplicationServiceFactory.GetApplicationService<IProgramMappingService>();
                        service.UploadPrograms(file.InputStream, fileName, "maintenance controller", DateTime.Now);
                        TempData["ProgramMessage"] = $"Master file Uploaded Successfully";
                    }
                    catch (Exception ex)
                    {
                        TempData["ProgramMessage"] = ex.Message;
                    }
                }
            }
            TempData["TabId"] = "reference_data";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult TestPricingRequestLogBucket()
        {
            try
            {
                var planId = 1;
                var jobId = 1;
                var pricingApiRequest = new PlanPricingApiRequestDto();
                var apiVersion = "789";
                var spotAllocationModelMode = SpotAllocationModelMode.Efficiency;

                var service = _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>();
                Task.Run(() => service.SavePricingRequest(planId, jobId, pricingApiRequest, apiVersion, spotAllocationModelMode));

                var nowStr = DateTime.Now.ToString();
                TempData["Message"] = $"Save triggered {nowStr}";
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }

            TempData["TabId"] = "planning";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult GenresDefaultExclusion()
        {
            TempData["TabId"] = "migrations";
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IPlanService>();
                service.GenresDefaultExclusion();
                TempData["Message"] = $"Default Genres have been appended to exclusion list!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
        #region Aab Utilities

        [HttpPost]
        public ActionResult ClearAgenciesCache()
        {
            TempData["TabId"] = "aab_maintenance";
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IAgencyService>();
                service.ClearAgenciesCache();
                TempData["Message"] = $"Agencies cache cleared successfully!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ClearAdvertisersCache()
        {
            TempData["TabId"] = "aab_maintenance";
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IAdvertiserService>();
                service.ClearAdvertisersCache();
                TempData["Message"] = $"Advertisers cache cleared successfully!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SyncAdvertisersToSchedules()
        {
            TempData["TabId"] = "aab_maintenance";
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IAdvertiserService>();
                service.SyncAdvertisersToSchedules();
                TempData["Message"] = $"Advertisers Synced successfully!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        #endregion // #region Aab Utilities

        #region Spot Exceptions

        [HttpPost]
        public async Task<ActionResult> SpotExceptionSyncIngestWeek(bool chkRunInBackground = false)
        {
            var username = _GetCurrentUserFullName();
            var service = _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsSyncService>();

            var request = new SpotExceptionsIngestTriggerRequest
            {
                Username = username,
                RunInBackground = chkRunInBackground
            };

            var result = await service.TriggerIngestForWeekAsync(request);

            TempData["TabId"] = "spot_exceptions_sync";
            TempData["Message"] = result.Message;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> SpotExceptionSyncIngestDateRange(string txtSESStartDate, string txtSESEndDate, bool chkRunInBackground = false)
        {
            var username = _GetCurrentUserFullName();
            var service = _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsSyncService>();

            string result;
            if (!DateTime.TryParse(txtSESStartDate, out DateTime startDate))
            {
                result = $"Could not parse given StartDate '{txtSESStartDate}' to a valid Date.  Try format '{BroadcastConstants.DATE_FORMAT_STANDARD}'.";
            }
            else if (!DateTime.TryParse(txtSESEndDate, out DateTime endDate))
            {
                result = $"Could not parse given EndDate '{txtSESEndDate}' to a valid Date.  Try format '{BroadcastConstants.DATE_FORMAT_STANDARD}'.";
            }
            else
            {
                var request = new SpotExceptionsIngestTriggerRequest
                {
                    Username = username,
                    StartDate = startDate,
                    EndDate = endDate,
                    RunInBackground = chkRunInBackground
                };

                var serviceResult = await service.TriggerIngestForDateRangeAsync(request);
                result = serviceResult.Message;
            }

            TempData["TabId"] = "spot_exceptions_sync";
            TempData["Message"] = result;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SpotExceptionSyncIngestForceJobToError(string txtSESIngestJobId)
        {
            var username = _GetCurrentUserFullName();
            var service = _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsSyncService>();

            string result;
            if (!int.TryParse(txtSESIngestJobId, out int jobId))
            {
                result = $"Could not parse given jobId '{txtSESIngestJobId}' to a valid integer.";
            }
            else
            {
                try
                {
                    result = service.SetJobToError(username, jobId);
                }
                catch (Exception ex)
                {
                    result = $"Error : {ex.Message}";
                }                
            }

            TempData["TabId"] = "spot_exceptions_sync";
            TempData["Message"] = result;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ClearSpotExceptionsData(string protectionKey)
        {
            try
            {
                if (protectionKey == "r1f2m3p4")
                {
                    var service = _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsService>();
                    service.ClearSpotExceptionAllData();
                    ViewBag.Message = "Spot Exceptions Data has been cleared.";
                }
                else
                {
                    ViewBag.Message = "Invalid protection key";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            TempData["Message"] = ViewBag.Message;
            TempData["TabId"] = "spot_exceptions_sync";

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Spots the exceptions result reset indicator.
        /// </summary>
        /// <param name="protectionKey">The protection key.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SpotExceptionsResultResetIndicator(string protectionKey)
        {
            try
            {
                if (protectionKey == "r3f4m1p2")
                {
                    var service = _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsServiceV2>();
                    service.ResetSpotExceptionResultsIndicator();
                    ViewBag.Message = "Spot Exceptions Results Indicator has been reset.";
                }
                else
                {
                    ViewBag.Message = "Invalid protection key";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }

            TempData["Message"] = ViewBag.Message;
            TempData["TabId"] = "spot_exceptions_sync";

            return RedirectToAction("Index");
        }

        #endregion // #region Spot Exceptions Sync
    }
}