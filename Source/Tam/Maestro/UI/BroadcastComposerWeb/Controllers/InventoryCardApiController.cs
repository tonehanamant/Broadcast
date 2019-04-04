using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/InventoryCards")]
    public class InventoryCardApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private readonly IWebLogger _Logger;

        public InventoryCardApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(InventoryCardApiController).Name))
        {
            _Logger = logger;
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        [HttpGet]
        [Route("InitialData")]
        public BaseResponse<InventoryCardInitialData> GetInventoryCards()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventoryCardService>().GetInitialData(DateTime.Now));
        }

        [HttpPost]
        [Route("Cards")]
        public BaseResponse<List<InventoryCardDto>> GetInventoryCards(InventoryCardFilterDto inventorySourceCardFilter)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventoryCardService>().GetInventoryCards(inventorySourceCardFilter));
        }
    }
}