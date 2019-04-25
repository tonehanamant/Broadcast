using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.InventorySummary;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/inventory")]
    public class InventorySummaryApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private readonly IWebLogger _Logger;

        public InventorySummaryApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(InventorySummaryApiController).Name))
        {
            _Logger = logger;
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        [HttpGet]
        [Route("sourcetypes")]
        public BaseResponse<List<InventorySource>> GetInventorySources()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>().GetInventorySources());
        }

        [HttpGet]
        [Route("quarters")]
        public BaseResponse<InventorySummaryQuarterFilterDto> GetInventoryQuarters()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>().GetInventoryQuarters(DateTime.Now));
        }

        [HttpPost]
        [Route("summaries")]
        public BaseResponse<List<InventorySummaryDto>> GetInventorySummaries(InventorySummaryFilterDto inventorySourceCardFilter)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventorySummaryService>().GetInventorySummaries(inventorySourceCardFilter, DateTime.Now));
        }
    }
}