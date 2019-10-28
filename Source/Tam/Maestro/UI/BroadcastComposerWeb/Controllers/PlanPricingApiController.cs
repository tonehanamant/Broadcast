using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.PlanPricing;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/PricingService")]
    public class PlanPricingApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public PlanPricingApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) : 
            base(logger, new ControllerNameRetriever(typeof(PlanPricingApiController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        [HttpGet]
        [Route("Run")]
        public BaseResponse<List<PlanPricingProgramDto>> Run(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().Run(planId));
        }
    }
}