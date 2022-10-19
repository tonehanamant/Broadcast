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
    [RoutePrefix("api/v1/spot-exceptions")]
    public class SpotExceptionApiController : BroadcastControllerBase
    {
        public SpotExceptionApiController(BroadcastApplicationServiceFactory applicationServiceFactory) :
           base(new ControllerNameRetriever(typeof(PlanIsciApiController).Name), applicationServiceFactory)
        {
        }

        [HttpPost]
        [Obsolete]
        [Route("recommended-plans")]
        public BaseResponse<List<SpotExceptionsRecommendedPlansResultsDto>> GetSpotExceptionsRecommendedPlans(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest)
        {
            //return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest));
            throw new NotSupportedException("We thought this operation is not used, this added 7/13/22 - BP-4306 !");
        }

        /// <summary>
        /// Gets the spot exception recommended plans.
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlansRequest">The spot exceptions recommended plans request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("spot-exception-recommended-plans")]
        public async Task<BaseResponse<SpotExceptionsRecommendedPlansResultsDto>> GetSpotExceptionRecommendedPlansAsync(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>().GetRecommendedPlansAsync(spotExceptionsRecommendedPlansRequest);

            return _ConvertToBaseResponse(() => result);
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
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>().GetRecommendedPlanSpotsAsync(spotExceptionRecomendedPlanSpotsRequest);

            return _ConvertToBaseResponse(() => result);
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
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>().GetRecommendedPlanDetailsAsync(spotExceptionsRecommendedPlanId);

            return _ConvertToBaseResponse(() => result);
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
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>().GetRecommendedPlanAdvertisersAsync(spotExceptionsRecommendedPlansAdvertisersRequest);

            return _ConvertToBaseResponse(() => result);
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
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>().GetRecommendedPlanStationsAsync(spotExceptionsRecommendedPlansStationRequest);

            return _ConvertToBaseResponse(() => result);
        }

        /// <summary>
        /// Gets the recommended plans filters.
        /// </summary>
        /// <param name="recomendedPlansFilterRequest">The recomended plans filter request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("recommended-plan-filters")]
        public async Task<BaseResponse<RecommendedPlanFiltersResultDto>> GetRecommendedPlansFilters(SpotExceptionsRecommendedPlansRequestDto recomendedPlansFilterRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>().GetRecommendedPlanFilters(recomendedPlansFilterRequest);

            return _ConvertToBaseResponse(() => result);
        }

        /// <summary>
        /// Saves the spot exceptions recommended plan.
        /// </summary>
        /// <param name="spotExceptionsRecommendedPlanSaveRequest">The spot exceptions recommended plan save request.</param>
        /// <returns></returns>
        //[Authorize]
        [HttpPost]
        [Route("recommended-plans-save")]
        public async Task<BaseResponse<bool>> SaveSpotExceptionsRecommendedPlan(SpotExceptionsRecommendedPlanSaveDecisionsRequestDto spotExceptionsRecommendedPlanSaveRequest)
        {
            var userName = _GetCurrentUserFullName();
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsRecommendedPlanService>().HandleSaveRecommendedPlanDecisionsAsync(spotExceptionsRecommendedPlanSaveRequest, userName);

            return _ConvertToBaseResponse(() => result);
        }

        /// Gets the OutofSpecPosts Data for the SpotException page
        /// </summary>       
        /// <param name="spotExceptionsOutOfSpecPostsRequest">The media week start and end date</param>
        /// <returns>List of SpotExceptionsOutOfSpecPostsResultDto object</returns>
        [HttpPost]
        [Route("out-of-spec")]
        public BaseResponse<List<SpotExceptionsOutOfSpecPostsResultDto>> GetOutofSpecPosts(SpotExceptionsOutOfSpecPostsRequestDto spotExceptionsOutOfSpecPostsRequest)
        {
            //return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsOutOfSpecsPosts(spotExceptionsOutOfSpecPostsRequest));
            throw new NotSupportedException("We thought this operation is not used, this added 9/06/22 - BP-5672 !");
        }

        [HttpPost]
        [Route("out-of-spec-plans")]
        public async Task<BaseResponse<SpotExceptionsOutOfSpecPlansResultDto>> GetSpotExceptionsOutofSpecsPlans(SpotExceptionsOutOfSpecPlansRequestDto spotExceptionsOutOfSpecsPlansReques)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>().GetSpotExceptionsOutOfSpecsPlansAsync(spotExceptionsOutOfSpecsPlansReques);

            return _ConvertToBaseResponse(() => result);
        }

        /// <summary>
        /// Gets spot exceptions out of specs details
        /// </summary>       
        /// <param name="spotExceptionsOutOfSpecId">The spot exceptions out of spec id</param>
        /// <returns>The spot exceptions out of spec details</returns>
        [HttpGet]
        [Route("out-of-spec-details")]
        public BaseResponse<SpotExceptionsOutOfSpecDetailsResultDto> GetSpotExceptionOutofSpecsDetails(int spotExceptionsOutOfSpecId)
        {
            //return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionOutofSpecsDetails(spotExceptionsOutOfSpecId));
            throw new NotSupportedException("We thought this operation is not used, this added 9/06/22 - BP-5672 !");
        }

        [HttpPost]
        [Route("out-of-spec-plan-spots")]
        public async Task<BaseResponse<SpotExceptionsOutOfSpecPlanSpotsResultDto>> GetSpotExceptionsOutOfSpecsSpots(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>().GetSpotExceptionsOutOfSpecSpotsAsync(spotExceptionsOutOfSpecSpotsRequest);

            return _ConvertToBaseResponse(() => result);
        }

        [HttpPost]
        [Route("out-of-spec-inventory-sources")]
        public async Task<BaseResponse<List<string>>> GetSpotExceptionsOutOfSpecSpotsInventorySources(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>().GetSpotExceptionsOutOfSpecSpotsInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest);
            return _ConvertToBaseResponse(() => result);
        }

        [HttpGet]
        [Route("out-of-spec-reason-codes")]
        public async Task<BaseResponse<List<SpotExceptionsOutOfSpecReasonCodeResultDto>>> GetSpotExceptionsOutOfSpecReasonCodes()
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>().GetSpotExceptionsOutOfSpecReasonCodesAsync();

            return _ConvertToBaseResponse(() => result);
        }

        [HttpPost]
        [Route("out-of-spec-markets")]
        public async Task<BaseResponse<List<string>>> GetSpotExceptionsOutOfSpecMarkets(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>().GetSpotExceptionsOutOfSpecMarketsAsync(spotExceptionsOutOfSpecSpotsRequest);

            return _ConvertToBaseResponse(() => result);
        }

        [HttpGet]
        [Obsolete]
        [Route("out-of-spec-genres")]
        public BaseResponse<List<SpotExceptionsOutOfSpecGenreDto>> GetGenres(string genre = "")
        {
            //return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsOutOfSpecGenres(genre));
            throw new NotSupportedException("We thought this operation is not used, this added  BP-5470 !");
        }

        [HttpGet]
        [Route("out-of-spec-programs")]
        public async Task<BaseResponse<List<SpotExceptionsOutOfSpecProgramsDto>>> GetPrograms(string programNameQuery)
        {
            var fullName = _GetCurrentUserFullName();

            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>().GetSpotExceptionsOutOfSpecProgramsAsync(programNameQuery, fullName);

            return _ConvertToBaseResponse(() => result);
        }

        [HttpPost]
        [Route("out-of-spec-advertisers")]
        public async Task<BaseResponse<List<string>>> GetSpotExceptionsOutofSpecAdvertisers(SpotExceptionsOutOfSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>().GetSpotExceptionsOutOfSpecAdvertisersAsync(spotExceptionsOutofSpecAdvertisersRequest);

            return _ConvertToBaseResponse(() => result);
        }

        [HttpPost]
        [Route("out-of-spec-stations")]
        public async Task<BaseResponse<List<string>>> GetSpotExceptionsOutofSpecsStations(SpotExceptionsOutofSpecsStationRequestDto spotExceptionsOutofSpecsStationRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>().GetSpotExceptionsOutOfSpecStationsAsync(spotExceptionsOutofSpecsStationRequest);

            return _ConvertToBaseResponse(() => result);
        }

        /// <summary>
        /// Save the OutofSpecDecisions Post Data for the Slide Out Drawer page
        /// </summary>       
        /// <param name="spotExceptionsOutOfSpecDecisionsPostsRequest">The SpotExceptionsOutOfSpec Decision Posts Request</param>
        /// <returns>true or false</returns>
        [HttpPost]
        [Obsolete]
        [Route("out-of-spec-save")]
        [Authorize]
        public BaseResponse<bool> SaveOutofSpecDecisions(SpotExceptionsOutOfSpecDecisionPostsRequestDto spotExceptionsOutOfSpecDecisionsPostsRequest)
        {
            //var userName = _GetCurrentUserFullName();
            //return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().SaveSpotExceptionsOutOfSpecsDecisions(spotExceptionsOutOfSpecDecisionsPostsRequest, userName));
            throw new NotSupportedException("We thought this operation is not used, this added 9/06/22 - BP-5672 !");
        }

        [HttpPost]
        [Route("out-of-spec-saves")]
        public async Task<BaseResponse<bool>> SaveOutofSpecDecisionsPlans(SpotExceptionsOutOfSpecSaveDecisionsRequestDto spotExceptionsOutOfSpecSaveRequest)
        {
            var userName = _GetCurrentUserFullName();

            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecService>().HandleSaveSpotExceptionsOutOfSpecAsync(spotExceptionsOutOfSpecSaveRequest, userName);

            return _ConvertToBaseResponse(() => result);
        }

        [HttpPost]
        [Route("out-of-spec-unposted")]
        public async Task<BaseResponse<SpotExceptionsUnpostedResultDto>> GetUnpostedSpotExceptions(SpotExceptionsUnpostedRequestDto spotExceptionUnpostedRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsUnpostedService>().GetSpotExceptionsUnposted(spotExceptionUnpostedRequest);

            return _ConvertToBaseResponse(() => result);
        }

        [HttpPost]
        [Route("trigger-decision-sync")]
        public async Task<BaseResponse<bool>> TriggerDecisionSync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsService>().TriggerDecisionSync(triggerDecisionSyncRequest);

            return _ConvertToBaseResponse(() => result);
        }

        [HttpGet]
        [Route("queued-decision-count")]
        public async Task<BaseResponse<int>> GetQueuedDecisionCount()
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsService>().GetQueuedDecisionCount();

            return _ConvertToBaseResponse(() => result);
        }
    }
}