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
using Newtonsoft.Json;
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

        [HttpGet]
        [Route("Quarters")]
        public BaseResponse<InventorySummaryQuarterFilterDto> GetInventoryQuarters()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>().GetInventoryQuarters(DateTime.Now));
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
    }
}
