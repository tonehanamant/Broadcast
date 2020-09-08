using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Plan.Buying;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Buying")]
    public class PlanBuyingApiController : BroadcastControllerBase
    {
        public PlanBuyingApiController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base( new ControllerNameRetriever(typeof(PlanBuyingApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Queues buying job
        /// </summary>
        /// <param name="planBuyingRequestDto">
        /// ProprietaryInventory is a list of proprietary summary ids
        /// </param>
        [HttpPost]
        [Route("queue")]
        public BaseResponse<PlanBuyingJob> Queue(PlanBuyingParametersDto planBuyingRequestDto)
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>()
                    .QueueBuyingJob(planBuyingRequestDto, DateTime.Now, _GetCurrentUserFullName()));
        }

        [HttpPost]
        [Route("execution")]
        public BaseResponse<CurrentBuyingExecution> GetCurrentBuyingExecution(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetCurrentBuyingExecution(planId));
        }

        /// <summary>
        /// Get programs data from the lastest pricing execution
        /// </summary>
        /// <param name="planId">Plan ID</param>
        /// <returns>Programs from the lastest pricing execution</returns>
        [HttpGet]
        [Route("programs/{planId}")]
        public BaseResponse<PlanBuyingResultProgramsDto> GetPrograms(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetPrograms(planId));
        }

        [HttpGet]
        [Route("bands/{planId}")]
        public BaseResponse<PlanBuyingBandsDto> GetBuyingBands(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingBands(planId));
        }

        [HttpGet]
        [Route("markets/{planId}")]
        public BaseResponse<PlanBuyingResultMarketsDto> GetBuyingMarkets(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetMarkets(planId));
        }

        [HttpGet]
        [Route("ownershipgroups/{planId}")]
        public BaseResponse<PlanBuyingResultOwnershipGroupDto> GetBuyingOwnershipGroups(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingOwnershipGroups(planId));
        }

        [HttpGet]
        [Route("repfirms/{planId}")]
        public BaseResponse<PlanBuyingResultRepFirmDto> GetBuyingRepFirms(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingRepFirms(planId));
        }

        [HttpPost]
        [Route("cancelexecution")]
        public BaseResponse<PlanBuyingResponseDto> CancelCurrentBuyingExecution(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().CancelCurrentBuyingExecution(planId));
        }

        /// <summary>
        /// Allows checking that correct inventory is used for pricing
        /// </summary>
        /// <param name="planId">Plan ID</param>
        /// <param name="requestParameters">Parameters</param>
        [HttpPost]
        [Route("requestprograms")]
        public BaseResponse<PlanBuyingApiRequestDto> GetBuyingProgramApiRequest(int planId, BuyingInventoryGetRequestParametersDto requestParameters)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingApiRequestPrograms(planId, requestParameters));
        }

        /// <summary>
        /// Allows checking that correct inventory is used for pricing
        /// </summary>
        /// <param name="planId">Plan ID</param>
        /// <param name="requestParameters">Parameters</param>
        [HttpPost]
        [Route("requestprograms3")]
        public BaseResponse<PlanBuyingApiRequestDto_v3> GetBuyingProgramApiRequest_v3(int planId, BuyingInventoryGetRequestParametersDto requestParameters)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingApiRequestPrograms_v3(planId, requestParameters));
        }

        [HttpPost]
        [Route("inventory")]
        public BaseResponse<List<PlanBuyingInventoryProgram>> GetBuyingInventory(int planId, BuyingInventoryGetRequestParametersDto requestParameters)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingInventory(planId, requestParameters));
        }

        /// <summary>
        /// Gets the unit caps.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("unitcaps")]
        public BaseResponse<List<LookupDto>> GetUnitCaps()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetUnitCaps());
        }

        /// <summary>
        /// Gets the pricing market groups.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("marketgroups")]
        public BaseResponse<List<LookupDto>> GetBuyingMarketGroups()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingMarketGroups());
        }

        [HttpGet]
        [Route("defaults")]
        public BaseResponse<PlanBuyingDefaults> GetPlanBuyingDefaults()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetPlanBuyingDefaults());
        }

        [HttpGet]
        [Route("stations/{planId}")]
        public BaseResponse<PlanBuyingStationResultDto> GetStations(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetStations(planId));
        }
    }
}
