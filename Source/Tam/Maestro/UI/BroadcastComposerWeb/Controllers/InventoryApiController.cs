using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System;
using System.Net.Http;
using System.Web.Http;
using Tam.Maestro.Web.Common;
using System.Net.Http.Headers;
using System.Net;
using Tam.Maestro.Services.Cable.Entities;
using System.Collections.Generic;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Entities.Scx;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using System.Linq;
using System.IO;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Inventory")]
    public class InventoryApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public InventoryApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(InventoryApiController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        /// <summary>
        /// Get inventory sources for summaries
        /// </summary>
        /// <remarks>
        /// Get a list of inventory sources available for summary
        /// </remarks>
        [HttpGet]
        [Route("Sources")]
        public BaseResponse<List<InventorySource>> GetInventorySources()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>().GetInventorySources());
        }

        /// <summary>
        /// Get all inventory source types
        /// </summary>
        /// <remarks>
        /// Returns a list of inventory source types
        /// </remarks>
        [HttpGet]
        [Route("SourceTypes")]
        public BaseResponse<List<LookupDto>> GetInventorySourceTypes()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>().GetInventorySourceTypes());
        }

        /// <summary>
        /// Get all quarters for inventory
        /// </summary>
        /// <remarks>
        /// Get a list of quarters for which there is available inventory
        /// 
        /// Make a request without parameters or only with one of the parameters specified 
        /// in order to get a list of quarters for all sources
        /// 
        /// Make a request with both inventorySourceId and daypartCodeId specified in order to get 
        /// a list of quarters for specific inventory source and daypart
        /// </remarks>
        /// <param name="inventorySourceId">Unique identifier of inventory source which is used to filter inventory out</param>
        /// <param name="daypartCodeId">Unique identifier of daypart code which is used to filter inventory out</param>
        [HttpGet]
        [Route("Quarters")]
        public BaseResponse<InventoryQuartersDto> GetInventoryQuarters(int? inventorySourceId = null, int? daypartCodeId = null)
        {
            return _ConvertToBaseResponse(() =>
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>();

                return inventorySourceId.HasValue && daypartCodeId.HasValue ?
                    service.GetInventoryQuarters(inventorySourceId.Value, daypartCodeId.Value) :
                    service.GetInventoryQuarters(DateTime.Now);
            });
        }

        /// <summary>
        /// Lists the inventory summaries
        /// </summary>
        /// <remarks>
        /// The filter object works by either receiving a inventory source, or a quarter.
        /// Additional filters apply on the set that is returned based on what was available
        /// from the sources or quarters.
        /// </remarks>
        /// <param name="inventorySourceCardFilter">Filter object for filtering the summaries</param>
        /// <returns>A list of inventory summaries</returns>
        [HttpPost]
        [Route("Summaries")]
        public BaseResponse<List<InventorySummaryDto>> GetInventorySummaries(InventorySummaryFilterDto inventorySourceCardFilter)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>().GetInventorySummaries(inventorySourceCardFilter, DateTime.Now));
        }

        /// <summary>
        /// Generates the SCX zip archive containing all the inventory, for CNN, Q12019, all units
        /// </summary>
        /// <remarks>Returns same archive every time to support old UI</remarks>
        /// <returns>A zip archive containing multiple .scx files</returns>
        [HttpGet]
        [Route("ScxArchive")]
        public HttpResponseMessage GenerateScxArchive(DateTime? nowDate = null)
        {
            InventoryScxDownloadRequest request = new InventoryScxDownloadRequest()
            {
                StartDate = new DateTime(2019, 01, 01),
                EndDate = new DateTime(2019, 03, 31),
                InventorySourceId = 5,
                UnitNames = new List<string> { "Unit 1", "Unit 2", "Unit 3", "Unit 4", "AM News1", "AM News2", "AM News3", "AM News4", "AM News5" },
                DaypartCodeId = 2
            };
            var archive = _ApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().GenerateScxFileArchive(request);

            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(archive.Item2);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = archive.Item1
            };

            return result;
        }

        /// <summary>
        /// Generates the SCX zip archive containing all the inventory, filter by the input parameters
        /// </summary>
        /// <remarks>Current code returns inventory for first quarter</remarks>
        /// <returns>A zip archive containing multiple .scx files</returns>
        [HttpGet]
        [Route("ScxDownload")]
        public HttpResponseMessage GenerateScxArchive([FromUri(Name = "")]InventoryScxDownloadRequest request)
        {
            var archive = _ApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().GenerateScxFileArchive(request);

            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(archive.Item2);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = archive.Item1
            };

            return result;
        }

        [HttpPost]
        [Route("InventoryFile")]
        public BaseResponse<InventoryFileSaveResult> UploadInventoryFile(FileRequest saveRequest)
        {
            try
            {
                var result = _ApplicationServiceFactory.GetApplicationService<IInventoryService>().IsProprietaryFile(saveRequest.FileName)
                    ? _ApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().SaveProprietaryInventoryFile(saveRequest, Identity.Name, DateTime.Now)
                    : _ApplicationServiceFactory.GetApplicationService<IInventoryService>().SaveInventoryFile(
                        new InventoryFileSaveRequest
                        {
                            FileName = saveRequest.FileName,
                            StreamData = saveRequest.StreamData,
                            InventorySource = "Open Market"
                        },
                        Identity.Name, DateTime.Now);

                return new BaseResponse<InventoryFileSaveResult>()
                {
                    Data = result,
                    Success = true
                };
            }
            catch (FileUploadException<InventoryFileProblem> e)
            {
                return new BaseResponseWithProblems<InventoryFileSaveResult, InventoryFileProblem>()
                {
                    Problems = e.Problems,
                    Message = "Problems found while processing file",
                    Success = false
                };
            }
            catch (Exception e)
            {
                return new BaseResponse<InventoryFileSaveResult>()
                {
                    Success = false,
                    Message = e.Message
                };
            }
        }

        /// <summary>
        /// Get all daypart codes for inventory
        /// </summary>
        /// <remarks>
        /// Get a list of all available daypart codes for specific inventory
        /// </remarks>
        /// <param name="inventorySourceId">Unique identifier of inventory source</param>
        [HttpGet]
        [Route("DaypartCodes")]
        public BaseResponse<List<DaypartCodeDto>> GetDaypartCodes(int inventorySourceId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>().GetDaypartCodes(inventorySourceId));
        }

        /// <summary>
        /// Get all units for inventory
        /// </summary>
        /// <remarks>
        /// Get a list of units for which there is available inventory that match inventory source, daypart code, start date, end date
        /// </remarks>
        /// <param name="inventorySourceId">Unique identifier of inventory source which is used to filter inventory out</param>
        /// <param name="daypartCodeId">Unique identifier of daypart code which is used to filter inventory out</param>
        /// <param name="startDate">Start date of the period for which inventory needs to be found</param>
        /// <param name="endDate">End date of the period for which inventory needs to be found</param>
        [HttpGet]
        [Route("Units")]
        public BaseResponse<List<string>> GetUnits(int inventorySourceId, int daypartCodeId, DateTime startDate, DateTime endDate)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory
                .GetApplicationService<IInventorySummaryService>()
                .GetInventoryUnits(inventorySourceId, daypartCodeId, startDate, endDate));
        }


        [HttpGet]
        [Route("DownloadErrorFiles")]
        public HttpResponseMessage DownloadErrorFiles([FromUri(Name = "")]List<int> fileIds)
        {
            if (!fileIds.Any())
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent, ReasonPhrase = "No file ids were supplied" };

            var archive = _ApplicationServiceFactory.GetApplicationService<IInventoryService>().DownloadErrorFiles(fileIds);

            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(archive.Item2);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = archive.Item1
            };
            return result;
        }

        [HttpGet]
        [Route("DownloadErrorFile")]
        public HttpResponseMessage DownloadErrorFile(int fileId = 0)
        {
            if (fileId == 0)
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent, ReasonPhrase = "No file id was supplied" };

            var file = _ApplicationServiceFactory.GetApplicationService<IInventoryService>().DownloadErrorFile(fileId);

            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new StreamContent(file.Item2);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(file.Item3);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = file.Item1
            };
            return result;
        }

        [HttpGet]
        [Route("UploadHistory")]
        public BaseResponse<List<InventoryUploadHistoryDto>> GetInventoryUploadHistory(int inventorySourceId)
        {
            //Returning list of upload history records
            return _ConvertToBaseResponse(() => 
                _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                    .GetInventoryUploadHistory(inventorySourceId));
        }
    }
}
