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
        /// Get all inventory sources
        /// </summary>
        /// <remarks>
        /// Get a list of all available inventory sources
        /// </remarks>
        [HttpGet]
        [Route("SourceTypes")]
        public BaseResponse<List<InventorySource>> GetInventorySources()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>().GetInventorySources());
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

        [HttpPost]
        [Route("Summaries")]
        public BaseResponse<List<InventorySummaryDto>> GetInventorySummaries(InventorySummaryFilterDto inventorySourceCardFilter)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>().GetInventorySummaries(inventorySourceCardFilter, DateTime.Now));
        }

        [HttpGet]
        [Route("ScxArchive/{nowDate?}")]
        public HttpResponseMessage GenerateScxArchive(DateTime? nowDate = null)
        {
            var archive = _ApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().GenerateScxFileArchive(nowDate ?? DateTime.Now);

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
                var result = _ApplicationServiceFactory
                    .GetApplicationService<IProprietaryInventoryService>()
                    .SaveProprietaryInventoryFile(saveRequest, Identity.Name, DateTime.Now);

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
    }
}
