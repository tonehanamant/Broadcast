using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Inventory;
using Services.Broadcast.Entities.Inventory;
using Services.Broadcast.Entities.InventoryMarketsAffiliates;
using Services.Broadcast.Entities.InventorySummary;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers.Inventory
{
    [RoutePrefix("api/v1/InventoryExport")]
	public class InventoryExportApiController : BroadcastControllerBase
	{
		#region constructor

		public InventoryExportApiController(
			BroadcastApplicationServiceFactory applicationServiceFactory)
			: base(new ControllerNameRetriever(typeof(InventoryExportApiController).Name), applicationServiceFactory)
		{
		}

		#endregion

		#region api calls

		/// <summary>
		///     Get all quarters for Open Market inventory
		/// </summary>
		/// <remarks>
		///     Get a list of quarters for which there is available Open Market inventory
		///     Make a request without parameters or only with one of the parameters specified
		///     in order to get a list of quarters for Open Market
		///     Make a request with both inventorySourceId specified in order to get
		///     a list of quarters for  Open Market inventory source
		/// </remarks>
		/// <param name="inventorySourceId">Unique identifier of inventory source which is used to filter inventory out</param>
		[HttpGet]
		[Route("QuartersOpenMarket")]
		public BaseResponse<InventoryQuartersDto> GetOpenMarketInventoryExportQuarters(int inventorySourceId)
		{
			return _ConvertToBaseResponse(() =>
			{
				var service = _ApplicationServiceFactory.GetApplicationService<IInventoryExportService>();

				return
					service.GetOpenMarketExportInventoryQuarters(inventorySourceId);
			});
		}

		///<summary>
		/// Get Genres for OpenMarket
		/// </summary>
		[HttpGet]
		[Route("GenreTypes")]
		public BaseResponse<List<LookupDto>> GetGenresForInventoryExport()
		{
			return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventoryExportService>().GetOpenMarketExportGenreTypes());
		}

		///<summary>
		/// Export OpenMarket Inventory File
		/// </summary>
		[HttpPost]
		[Route("GenerateExportForOpenMarket")]
		[Authorize]
		public BaseResponse<int> GenerateExportForOpenMarket(InventoryExportRequestDto dto)
		{
			try
			{
				var fullName = _GetCurrentUserFullName();
                var appDataPath = _GetAppDataPath();
				var result = _ApplicationServiceFactory.GetApplicationService<IInventoryExportService>().GenerateExportForOpenMarket(dto, fullName, appDataPath);

				return new BaseResponse<int>()
				{
					Data = result,
					Success = true
				};
			}

			catch (Exception e)
			{
				return new BaseResponse<int>()
				{
					Success = false,
					Message = e.Message
				};
			}
		}

		[HttpGet]
		[Route("DownloadInventoryExportFile")]
		public HttpResponseMessage DownloadInventoryExportFile(int id = 0)
		{
			if (id == 0)
				return new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent, ReasonPhrase = "No file id was supplied" };
			try
			{
				var file = _ApplicationServiceFactory.GetApplicationService<IInventoryExportService>().DownloadOpenMarketExportFile(id);

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
				var message = $"Exception caught attempting to download inventory export files with ids '{id}'.";
				_LogError(message, ex);

				return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message, ex);
			}
		}

        /// <summary>
		/// Generates the inventory market affiliates report.
		/// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [HttpPost]
		[Route("GenerateMarketAffiliatesReport")]
		public BaseResponse<Guid> GenerateInventoryMarketAffiliatesReport([FromBody] InventoryMarketAffiliatesReportRequest request)
		{
			var fullName = _GetCurrentUserFullName();
			var appDataPath = _GetAppDataPath();

			return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventoryMarketAffiliatesExportService>()
				.GenerateMarketAffiliatesReport(request, fullName, DateTime.Now, appDataPath));
		}
		#endregion
	}
}