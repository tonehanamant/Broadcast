using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Services.Broadcast.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/PricingService")]
    public class PlanPricingApiController : BroadcastControllerBase
    {
        public PlanPricingApiController(BroadcastApplicationServiceFactory applicationServiceFactory) : 
            base(new ControllerNameRetriever(typeof(PlanPricingApiController).Name), applicationServiceFactory)
        {
        }

        [HttpPost]
        [Route("Queue")]
        public BaseResponse<PlanPricingJob> Queue(PlanPricingParametersDto planPricingRequestDto)
        {
            return _ConvertToBaseResponse(() => 
                _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>()
                    .QueuePricingJob(planPricingRequestDto, DateTime.Now, _GetCurrentUserFullName()));
        }

        [HttpPost]
        [Route("Execution")]
        public BaseResponse<PlanPricingResponseDto> GetCurrentPricingExecution(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetCurrentPricingExecution(planId));
        }

        [HttpPost]
        [Route("CancelExecution")]
        public BaseResponse<PlanPricingResponseDto> CancelCurrentPricingExecution(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().CancelCurrentPricingExecution(planId));
        }

        /// <summary>
        /// Allows checking that correct inventory is used for pricing
        /// </summary>
        /// <param name="planId">Plan ID</param>
        /// <param name="requestParameters">
        /// InventorySourceIds
        /// - pass InventorySourceIds only when you want to check what proprietary inventory could be potentially chosen
        /// - DO NOT pass InventorySourceIds if you want to check what is sent to the pricing model
        /// </param>
        [HttpPost]
        [Route("PricingApiRequestPrograms")]
        public BaseResponse<PlanPricingApiRequestDto> GetPricingProgramApiRequest(int planId, PricingInventoryGetRequestParametersDto requestParameters)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPricingApiRequestPrograms(planId, requestParameters));
        }

        [HttpPost]
        [Route("Inventory")]
        public BaseResponse<List<PlanPricingInventoryProgram>> GetPricingInventory(int planId, PricingInventoryGetRequestParametersDto requestParameters)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPricingInventory(planId, requestParameters));
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