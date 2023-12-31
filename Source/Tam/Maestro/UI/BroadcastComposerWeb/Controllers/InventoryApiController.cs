﻿using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;
using Unity;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Inventory")]
    public class InventoryApiController : BroadcastControllerBase
    {
        public InventoryApiController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(InventoryApiController).Name), applicationServiceFactory)
        {
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
        /// Make a request with both inventorySourceId and standardDaypartId specified in order to get 
        /// a list of quarters for specific inventory source and daypart
        /// </remarks>
        /// <param name="inventorySourceId">Unique identifier of inventory source which is used to filter inventory out</param>
        /// <param name="standardDaypartId">Unique identifier of daypart default which is used to filter inventory out</param>
        [HttpGet]
        [Route("Quarters")]
        public BaseResponse<InventoryQuartersDto> GetInventoryQuarters(int? inventorySourceId = null, int? standardDaypartId = null)
        {
            return _ConvertToBaseResponse(() =>
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>();

                return inventorySourceId.HasValue && standardDaypartId.HasValue ?
                    service.GetInventoryQuarters(inventorySourceId.Value, standardDaypartId.Value) :
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
            var service = _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>();
            return _ConvertToBaseResponse(() => service.GetInventorySummariesWithCache(inventorySourceCardFilter, DateTime.Now));
        }
        
        /// <summary>
        /// Get number of items in inventory cache. Optionally reset the cache.
        /// </summary>
        /// <param name="reset">Flag indicating whether to reset cache. Defaults to false.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Cache")]
        public BaseResponse<long> GetCountAndReset(Boolean reset = false)
        {
            var cache = BroadcastApplicationServiceFactory.Instance.Resolve<IInventorySummaryCache>();

            return _ConvertToBaseResponse(() => cache.GetItemCount(reset));
        }

        /// <summary>
        /// Enqueues a job to generate the SCX files based on the parameters sent.
        /// The SCX will be generated and saved in the folder location
        /// </summary>
        /// <returns>Success or failure to create a new job to generate the SCX files</returns>
        [HttpGet]
        [Route("ScxDownloadJob")]
        [Authorize]
        public BaseResponse GenerateScxArchiveJob([FromUri(Name = "")]InventoryScxDownloadRequest request)
        {
            var fullName = _GetCurrentUserFullName();
            _ApplicationServiceFactory.GetApplicationService<IScxGenerationService>().QueueScxGenerationJob(request, fullName, DateTime.Now);

            return new BaseResponse
            {
                Success = true,
                Message = "SCX file generation job has been added to the queue successfully"
            };
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
        [Authorize]
        public BaseResponse<InventoryFileSaveResult> UploadInventoryFile(FileRequest saveRequest)
        {
            try
            {
                var fullName = _GetCurrentUserFullName();
                var result = _ApplicationServiceFactory.GetApplicationService<IInventoryService>().IsProprietaryFile(saveRequest.FileName)
                    ? _ApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().SaveProprietaryInventoryFile(saveRequest, fullName, DateTime.Now)
                    : _ApplicationServiceFactory.GetApplicationService<IInventoryService>().SaveInventoryFile(
                        new InventoryFileSaveRequest
                        {
                            FileName = saveRequest.FileName,
                            StreamData = saveRequest.StreamData,
                            InventorySource = "Open Market"
                        },
                        fullName, DateTime.Now);
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
        /// Get a list of all available standard dayparts for specific inventory
        /// </summary>
        /// <param name="inventorySourceId">The inventory source identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("standarddayparts")]
        public BaseResponse<List<StandardDaypartDto>> GetStandardDayparts(int inventorySourceId)
        {
            return _ConvertToBaseResponse(() => 
                _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>().GetStandardDayparts(inventorySourceId));
        }

        /// <summary>
        /// Get all units for inventory
        /// </summary>
        /// <remarks>
        /// Get a list of units for which there is available inventory that match inventory source, daypart code, start date, end date
        /// </remarks>
        /// <param name="inventorySourceId">Unique identifier of inventory source which is used to filter inventory out</param>
        /// <param name="standardDaypartId">Unique identifier of daypart default which is used to filter inventory out</param>
        /// <param name="startDate">Start date of the period for which inventory needs to be found</param>
        /// <param name="endDate">End date of the period for which inventory needs to be found</param>
        [HttpGet]
        [Route("Units")]
        public BaseResponse<List<string>> GetUnits(int inventorySourceId, int standardDaypartId, DateTime startDate, DateTime endDate)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory
                .GetApplicationService<IInventorySummaryService>()
                .GetInventoryUnits(inventorySourceId, standardDaypartId, startDate, endDate));
        }


        [HttpGet]
        [Route("DownloadErrorFiles")]
        public HttpResponseMessage DownloadErrorFiles([FromUri(Name = "")]List<int> fileIds)
        {
            if (!fileIds.Any())
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent, ReasonPhrase = "No file ids were supplied" };

            try
            {
                var archive = _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                    .DownloadErrorFiles(fileIds);

                var result = Request.CreateResponse(HttpStatusCode.OK);
                result.Content = new StreamContent(archive.Item2);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = archive.Item1
                };
                return result;
            }
            catch (Exception ex)
            {
                var message = $"Exception caught attempting to download error files with ids '{string.Join(",", fileIds)}'.";
                _LogError(message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message, ex);
            }
        }

        [HttpGet]
        [Route("DownloadErrorFile")]
        public HttpResponseMessage DownloadErrorFile(int fileId = 0)
        {
            if (fileId == 0)
                return new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent, ReasonPhrase = "No file id was supplied" };
            try
            {
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
            catch (Exception ex)
            {
                var message = $"Exception caught attempting to download error files with ids '{fileId}'.";
                _LogError(message, ex);

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message, ex);
            }
        }

        [HttpGet]
        [Route("UploadHistory")]
        public BaseResponse<List<InventoryUploadHistoryDto>> GetInventoryUploadHistory(int inventorySourceId, int? quarter = null, int? year = null)
        {
            return _ConvertToBaseResponse(() => 
                _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                    .GetInventoryUploadHistory(inventorySourceId, quarter, year));
        }

        [HttpGet]
        [Route("UploadHistoryQuarters")]
        public BaseResponse<List<QuarterDetailDto>> GetInventoryUploadHistoryQuarters(int inventorySourceId)
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                    .GetInventoryUploadHistoryQuarters(inventorySourceId));
        }

        /// <summary>
        /// Gets the SCX file generation history.
        /// </summary>
        /// <param name="inventorySourceId">The inventory source identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("ScxFileGenerationHistory")]
        public BaseResponse<List<ScxFileGenerationDetail>> GetScxFileGenerationHistory(int inventorySourceId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory
                .GetApplicationService<IScxGenerationService>()
                .GetScxFileGenerationHistory(inventorySourceId));
        }

        /// <summary>
        /// Downloads the generated SCX file.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("DownloadScxFile")]
        public HttpResponseMessage DownloadScxFile(int fileId)
        {
            try
            {
                var file = _ApplicationServiceFactory.GetApplicationService<IScxGenerationService>()
                    .DownloadGeneratedScxFile(fileId);

                var result = Request.CreateResponse(HttpStatusCode.OK);
                result.Content = new StreamContent(file.Item2);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(file.Item3);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = file.Item1
                };
                return result;
            }
            catch (Exception e)
            {
                var errorResponse = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
                return errorResponse;
            }
        }

        /// <summary>
        /// Enqueues a job to generate the SCX files based on the parameters sent.
        /// The SCX will be generated and saved in the folder location
        /// </summary>
        /// <returns>Success or failure to create a new job to generate the SCX files</returns>
        [HttpGet]
        [Route("ScxDownloadJob-OpenMarkets")]
        [Authorize]
        public BaseResponse ScxDownloadJobOpenMarkets([FromUri(Name = "")] InventoryScxOpenMarketsDownloadRequest request)
        {
            var fullName = _GetCurrentUserFullName();
            _ApplicationServiceFactory.GetApplicationService<IScxGenerationService>().QueueScxOpenMarketsGenerationJob(request, fullName, DateTime.Now);

            return new BaseResponse
            {
                Success = true,
                Message = "SCX file generation job for Open Market has been added to the queue successfully"
            };
        }


        [HttpGet]
        [Route("ScxOpenMarketFileGenerationHistory")]
        public BaseResponse<List<ScxOpenMarketFileGenerationDetail>> GetOpenMarketScxFileGenerationHistory()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory
                .GetApplicationService<IScxGenerationService>()
                .GetOpenMarketScxFileGenerationHistory());
        }

        /// <summary>
        /// Downloads the generated SCX file For Open Market.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("DownloadOpenMarketScxFile")]
        public HttpResponseMessage DownloadOpenMarketScxFile(int fileId)
        {
            try
            {
                var file = _ApplicationServiceFactory.GetApplicationService<IScxGenerationService>()
                    .DownloadGeneratedScxFileForOpenMarket(fileId);

                var result = Request.CreateResponse(HttpStatusCode.OK);
                result.Content = new StreamContent(file.Item2);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(file.Item3);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = file.Item1
                };
                return result;
            }
            catch (Exception e)
            {
                var errorResponse = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message);
                return errorResponse;
            }
        }
    }
}
