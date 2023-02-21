using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Entities.SpotExceptions.Unposted;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;
using Services.Broadcast.Entities.SpotExceptions;

namespace BroadcastComposerWeb.Controllers
{
    /// <summary></summary>
    [RoutePrefix("api/v1/spot-exceptions")]
    public class SpotExceptionApiController : BroadcastControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpotExceptionApiController" /> class.
        /// </summary>
        /// <param name="applicationServiceFactory">The application service factory.</param>
        public SpotExceptionApiController(BroadcastApplicationServiceFactory applicationServiceFactory) :
           base(new ControllerNameRetriever(typeof(PlanIsciApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Gets the spot exception recommended plans.
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlansRequest">The spot exceptions recommended plans request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("spot-exception-recommended-plans")]
        public async Task<BaseResponse<SpotExceptionsRecommendedPlanGroupingResults>> GetSpotExceptionRecommendedPlansAsync(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>()
                .GetRecommendedPlanGroupingAsync(spotExceptionsRecommendedPlansRequest));
        }

        /// <summary>
        /// Gets the spot exceptions recommended plan spots.
        /// </summary>
        /// <param name="spotExceptionRecomendedPlanSpotsRequest">The recomended plans request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("recommended-plan-spots")]
        public async Task<BaseResponse<SpotExceptionsRecommendedPlanSpotsResultDto>> GetSpotExceptionsRecommendedPlanSpotsAsync(SpotExceptionsRecommendedPlanSpotsRequestDto spotExceptionRecomendedPlanSpotsRequest)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>()
                .GetRecommendedPlanSpotsAsync(spotExceptionRecomendedPlanSpotsRequest));
        }

        /// <summary>
        /// Gets the spot exceptions recommended plan details asynchronous.
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlanId">The spot exceptions recommended plan details identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("recommended-plans-details")]
        public async Task<BaseResponse<SpotExceptionsRecommendedPlanDetailsResultDto>> GetSpotExceptionsRecommendedPlanDetailsAsync(int spotExceptionsRecommendedPlanId)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>()
                .GetRecommendedPlanDetailsAsync(spotExceptionsRecommendedPlanId));

        }

        /// <summary>
        /// Gets the spot exceptions recommended plans advertisers.
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlansAdvertisersRequest">The spot exceptions recommended plans advertisers request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("recommended-plans-advertisers")]
        public async Task<BaseResponse<List<string>>> GetSpotExceptionsRecommendedPlansAdvertisers(SpotExceptionsRecommendedPlanAdvertisersRequestDto spotExceptionsRecommendedPlansAdvertisersRequest)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>()
                .GetRecommendedPlanAdvertisersAsync(spotExceptionsRecommendedPlansAdvertisersRequest));
        }

        /// <summary>
        /// Gets the spot exceptions recommended plan stations.
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlansStationRequest">The spot exceptions recommended plans station request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("recommended-plans-stations")]
        public async Task<BaseResponse<List<string>>> GetSpotExceptionsRecommendedPlanStations(SpotExceptionsRecommendedPlanStationsRequestDto spotExceptionsRecommendedPlansStationRequest)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>()
                .GetRecommendedPlanStationsAsync(spotExceptionsRecommendedPlansStationRequest));
        }

        /// <summary>
        /// Gets the recommended plans filters.
        /// </summary>
        /// <param name="recomendedPlansFilterRequest">The recomended plans filter request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("recommended-plan-filters")]
        public async Task<BaseResponse<SpotExceptionsRecommendedPlanFiltersResultDto>> GetRecommendedPlansFilters(SpotExceptionsRecommendedPlansRequestDto recomendedPlansFilterRequest)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>()
                .GetRecommendedPlanFilters(recomendedPlansFilterRequest));
        }

        /// <summary>
        /// Saves the spot exceptions recommended plan.
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlanSaveRequest">The spot exceptions recommended plan save request.</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("recommended-plans-save")]
        public async Task<BaseResponse<bool>> SaveSpotExceptionsRecommendedPlan(SpotExceptionsRecommendedPlanSaveDecisionsRequestDto spotExceptionsRecommendedPlanSaveRequest)
        {
            var userName = _GetCurrentUserFullName();
            
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>()
                .HandleSaveRecommendedPlanDecisionsAsync(spotExceptionsRecommendedPlanSaveRequest, userName));
        }

        /// <summary>
        /// Gets the spot exceptions outof specs plans.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecsPlansReques">The spot exceptions out of specs plans reques.</param>
        /// <returns> </returns>
        [HttpPost]
        [Route("out-of-spec-plans")]
        public async Task<BaseResponse<SpotExceptionsOutOfSpecGroupingResults>> GetSpotExceptionsOutofSpecsPlans(SpotExceptionsOutOfSpecPlansRequestDto spotExceptionsOutOfSpecsPlansReques)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>()
                .GetSpotExceptionsOutOfSpecGroupingAsync(spotExceptionsOutOfSpecsPlansReques));
        }

        /// <summary>
        /// Gets the spot exceptions out of specs spots.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest">The spot exceptions out of spec spots request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-plan-spots")]
        public async Task<BaseResponse<SpotExceptionsOutOfSpecSpotsResultDto>> GetSpotExceptionsOutOfSpecsSpots(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>()
                .GetSpotExceptionsOutOfSpecSpotsAsync(spotExceptionsOutOfSpecSpotsRequest));
        }

        /// <summary>
        /// Gets the spot exceptions out of spec spots inventory sources.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest">The spot exceptions out of spec spots request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-inventory-sources")]
        public async Task<BaseResponse<List<string>>> GetSpotExceptionsOutOfSpecSpotsInventorySources(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>()
                .GetSpotExceptionsOutOfSpecSpotsInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest));
        }

        /// <summary>
        /// Gets the spot exceptions out of spec reason codes.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("out-of-spec-reason-codes")]
        public async Task<BaseResponse<List<SpotExceptionsOutOfSpecReasonCodeResultDto>>> GetSpotExceptionsOutOfSpecReasonCodes()
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>()
                .GetSpotExceptionsOutOfSpecReasonCodesAsync());
        }

        /// <summary>
        /// Gets the spot exceptions out of spec markets.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest">The spot exceptions out of spec spots request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-markets")]
        public async Task<BaseResponse<List<string>>> GetSpotExceptionsOutOfSpecMarkets(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>()
                .GetSpotExceptionsOutOfSpecMarketsAsync(spotExceptionsOutOfSpecSpotsRequest));
        }

        /// <summary>
        /// Gets the programs.
        /// </summary>
        /// <param name="programNameQuery">The program name query.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("out-of-spec-programs")]
        public async Task<BaseResponse<List<SpotExceptionsOutOfSpecProgramsDto>>> GetPrograms(string programNameQuery)
        {
            var fullName = _GetCurrentUserFullName();

            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>()
                .GetSpotExceptionsOutOfSpecProgramsAsync(programNameQuery, fullName));
        }

        /// <summary>
        /// Gets the spot exceptions outof spec advertisers.
        /// </summary>
        /// <param name="spotExceptionsOutofSpecAdvertisersRequest">The spot exceptions outof spec advertisers request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-advertisers")]
        public async Task<BaseResponse<List<string>>> GetSpotExceptionsOutofSpecAdvertisers(SpotExceptionsOutOfSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>()
                .GetSpotExceptionsOutOfSpecAdvertisersAsync(spotExceptionsOutofSpecAdvertisersRequest));
        }

        /// <summary>
        /// Gets the spot exceptions outof specs stations.
        /// </summary>
        /// <param name="spotExceptionsOutofSpecsStationRequest">The spot exceptions outof specs station request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-stations")]
        public async Task<BaseResponse<List<string>>> GetSpotExceptionsOutofSpecsStations(SpotExceptionsOutofSpecsStationRequestDto spotExceptionsOutofSpecsStationRequest)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>()
                .GetSpotExceptionsOutOfSpecStationsAsync(spotExceptionsOutofSpecsStationRequest));
        }

        /// <summary>
        /// Saves the out of spec decisions plans.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSaveRequest">The spot exceptions out of spec save request.</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("out-of-spec-saves")]
        public async Task<BaseResponse<bool>> SaveOutofSpecDecisionsPlans(SpotExceptionsOutOfSpecSaveDecisionsRequestDto spotExceptionsOutOfSpecSaveRequest)
        {
            var userName = _GetCurrentUserFullName();

            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>()
                .HandleSaveSpotExceptionsOutOfSpecAsync(spotExceptionsOutOfSpecSaveRequest, userName));
        }

        /// <summary>
        /// Gets the unposted spot exceptions.
        /// </summary>
        /// <param name="spotExceptionUnpostedRequest">The spot exception unposted request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-unposted")]
        public async Task<BaseResponse<SpotExceptionsUnpostedResultDto>> GetUnpostedSpotExceptions(SpotExceptionsUnpostedRequestDto spotExceptionUnpostedRequest)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsUnpostedService>()
                .GetSpotExceptionsUnposted(spotExceptionUnpostedRequest));
        }

        /// <summary>
        /// Triggers the decision synchronize.
        /// </summary>
        /// <param name="triggerDecisionSyncRequest">The trigger decision synchronize request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("trigger-decision-sync")]
        public async Task<BaseResponse<bool>> TriggerDecisionSync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest)
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsService>()
                .TriggerDecisionSync(triggerDecisionSyncRequest));
        }

        /// <summary>
        /// Gets the queued decision count.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("queued-decision-count")]
        public async Task<BaseResponse<int>> GetQueuedDecisionCount()
        {
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory
                .GetApplicationService<ISpotExceptionsService>().GetQueuedDecisionCount());
        }
    }
}