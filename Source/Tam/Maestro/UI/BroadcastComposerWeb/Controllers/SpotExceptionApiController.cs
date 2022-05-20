using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.SpotExceptions;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/spot-exceptions")]
    public class SpotExceptionApiController : BroadcastControllerBase
    {
        public SpotExceptionApiController(BroadcastApplicationServiceFactory applicationServiceFactory) :
           base(new ControllerNameRetriever(typeof(PlanIsciApiController).Name), applicationServiceFactory)
        {
        }
        /// <summary>
        /// Gets the OutofSpecPosts Data for the SpotException page
        /// </summary>       
        /// <param name="spotExceptionsOutOfSpecPostsRequest">The media week start and end date</param>
        /// <returns>List of SpotExceptionsOutOfSpecPostsResultDto object</returns>
        [HttpPost]
        [Route("out-of-spec")]
        public BaseResponse<List<SpotExceptionsOutOfSpecPostsResultDto>> GetOutofSpecPosts(SpotExceptionsOutOfSpecPostsRequestDto spotExceptionsOutOfSpecPostsRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsOutOfSpecsPosts(spotExceptionsOutOfSpecPostsRequest));
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
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionOutofSpecsDetails(spotExceptionsOutOfSpecId));
        }

        /// <summary>
        /// Get the result of out-of-spec advertiser. 
        /// </summary>       
        /// <param name="spotExceptionsOutofSpecAdvertisersRequest">The media week start and end date</param>     
        /// <returns>The list of advertiser name from out-of-spec result</returns>
        [HttpPost]
        [Route("out-of-spec-advertisers")]
        public BaseResponse<List<string>> GetSpotExceptionsOutofSpecAdvertisers(SpotExceptionsOutofSpecAdvertisersRequestDto spotExceptionsOutofSpecAdvertisersRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsOutofSpecAdvertisers(spotExceptionsOutofSpecAdvertisersRequest));
        }

        [HttpPost]
        [Route("recommended-plans")]
        public BaseResponse<List<SpotExceptionsRecommendedPlansResultDto>> GetSpotExceptionsRecommendedPlans(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest));
        }

        [HttpGet]
        [Route("recommended-plans-details")]
        public BaseResponse<SpotExceptionsRecommendedPlanDetailsResultDto> GetSpotExceptionsRecommendedPlanDetails(int spotExceptionsRecommendedPlanId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsRecommendedPlanDetails(spotExceptionsRecommendedPlanId));
        }

        /// <summary>
        /// Get the result of recommended plans advertiser. 
        /// </summary>       
        /// <param name="spotExceptionsRecommendedPlansAdvertisersRequest">The media week start and end date</param>     
        /// <returns>The list of advertiser name from recommended plans result</returns>
        [HttpPost]
        [Route("recommended-plans-advertisers")]
        public BaseResponse<List<string>> GetSpotExceptionsRecommendedPlansAdvertisers(SpotExceptionsRecommendedPlansAdvertisersRequestDto spotExceptionsRecommendedPlansAdvertisersRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsRecommendedPlansAdvertisers(spotExceptionsRecommendedPlansAdvertisersRequest));
        }
        [Authorize]
        [HttpPost]
        [Route("recommended-plans-save")]
        public BaseResponse<bool> SaveSpotExceptionsRecommendedPlan(SpotExceptionsRecommendedPlanSaveRequestDto spotExceptionsRecommendedPlanSaveRequest)
        {
            var userName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().SaveSpotExceptionsRecommendedPlan(spotExceptionsRecommendedPlanSaveRequest, userName));
        }

        /// <summary>
        /// Save the OutofSpecDecisions Post Data for the Slide Out Drawer page
        /// </summary>       
        /// <param name="spotExceptionsOutOfSpecDecisionsPostsRequest">The SpotExceptionsOutOfSpec Decision Posts Request</param>
        /// <returns>true or false</returns>
        [HttpPost]
        [Route("out-of-spec-save")]
        [Authorize]
        public BaseResponse<bool> SaveOutofSpecDecisions(SpotExceptionsOutOfSpecDecisionsPostsRequestDto spotExceptionsOutOfSpecDecisionsPostsRequest)
        {
            var userName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().SaveSpotExceptionsOutOfSpecsDecisions(spotExceptionsOutOfSpecDecisionsPostsRequest, userName));
        }

        [HttpGet]
        [Route("out-of-spec-reason-codes")]
        public BaseResponse<List<SpotExceptionsOutOfSpecReasonCodeResultDto>> GetSpotExceptionsOutOfSpecReasonCodes()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsOutOfSpecReasonCodes());
        }

        [HttpPost]
        [Route("recommended-plans-stations")]
        public BaseResponse<List<string>> GetSpotExceptionsRecommendedPlansStations(SpotExceptionsRecommendedPlansStationRequestDto spotExceptionsRecommendedPlansStationRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsRecommendedPlansStations(spotExceptionsRecommendedPlansStationRequest));
        }

        [HttpPost]
        [Route("out-of-spec-stations")]
        public BaseResponse<List<string>> GetSpotExceptionsOutofSpecsStations(SpotExceptionsOutofSpecsStationRequestDto spotExceptionsOutofSpecsStationRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsOutofSpecsStations(spotExceptionsOutofSpecsStationRequest));
        }

        [HttpPost]
        [Route("out-of-spec-plans")]
        public BaseResponse<SpotExceptionsOutOfSpecPlansResultDto> GetSpotExceptionsOutofSpecsPlans(SpotExceptionsOutofSpecsPlansRequestDto spotExceptionsOutofSpecsActivePlansRequestDto)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsOutofSpecsPlans(spotExceptionsOutofSpecsActivePlansRequestDto));
        }

        [HttpPost]
        [Route("out-of-spec-plan-spots")]
        public BaseResponse<SpotExceptionsOutOfSpecPlanSpotsResultDto> GetSpotExceptionsOutofSpecSpots(SpotExceptionsOutofSpecSpotsRequestDto spotExceptionsOutofSpecSpotsRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsOutofSpecSpots(spotExceptionsOutofSpecSpotsRequest));
        }

        [HttpPost]
        [Route("out-of-spec-markets")]
        public BaseResponse<List<string>> GetSpotExceptionsOutOfSpecMarkets(SpotExceptionsOutofSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsOutOfSpecMarkets(spotExceptionsOutOfSpecSpotsRequest));
        }

        [HttpPost]
        [Route("out-of-spec-unposted")]
        public BaseResponse<SpotExceptionOutOfSpecUnpostedResultDto> GetUnpostedSpotExceptions(SpotExceptionOutOfSpecUnpostedRequestDto spotExceptionOutOfSpecUnpostedRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsUnposted(spotExceptionOutOfSpecUnpostedRequest));
        }

        [HttpPost]
        [Route("out-of-spec-saves")]
        public BaseResponse<bool> SaveOutofSpecDecisionsPlans(SpotExceptionSaveDecisionsPlansRequestDto spotExceptionSaveDecisionsPlansRequest)
        {
            var userName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().SaveOutofSpecDecisionsPlans(spotExceptionSaveDecisionsPlansRequest, userName));
        }

        [HttpPost]
        [Route("trigger-decision-sync")]
        public BaseResponse<bool> TriggerDecisionSync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().TriggerDecisionSync(triggerDecisionSyncRequest));
        }

        [HttpGet]
        [Route("queued-decision-count")]
        public BaseResponse<int> GetQueuedDecisionCount()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetQueuedDecisionCount());
        }

        [HttpPost]
        [Route("spot-exception-recommanded-plans")]
        public BaseResponse<SpotExceptionsRecommendedPlansResultsDto> GetSpotExceptionRecommendedPlans(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetRecommendedPlans(spotExceptionsRecommendedPlansRequest));
        }

        [HttpGet]
        [Route("out-of-spec-genres")]
        public BaseResponse<List<SpotExceptionsOutOfSpecGenreDto>> GetGenres(string genre = "")
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsOutOfSpecGenres(genre));
        }
    }
}
