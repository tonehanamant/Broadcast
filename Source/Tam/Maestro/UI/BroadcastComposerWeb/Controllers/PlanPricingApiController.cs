using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.PlanPricing;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities.DataTransferObjects;
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
        [Route("GetInventoryForPlan")]
        public BaseResponse<List<PlanPricingProgramDto>> GetInventoryForPlan(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetInventoryForPlan(planId));
        }

        [HttpPost]
        [Route("Run")]
        public BaseResponse<PlanPricingResultDto> Run(PlanPricingRequestDto planPricingRequestDto)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().Run(planPricingRequestDto));
        }

        /// <summary>
        /// Gets the unit caps.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("UnitCaps")]
        public BaseResponse<List<LookupDto>> GetUnitCaps()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetUnitCaps());
        }

        [HttpGet]
        [Route("PlanPricingDefaults")]
        public BaseResponse<PlanPricingDefaults> GetPlanPricingDefaults()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPlanPricingDefaults());
        }
    }
}