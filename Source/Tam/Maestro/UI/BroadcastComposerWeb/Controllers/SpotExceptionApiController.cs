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

        [HttpPost]
        [Route("recommended-plans")]
        public BaseResponse<List<SpotExceptionsRecommendedPlansResultDto>> GetSpotExceptionsRecommendedPlans(SpotExceptionsRecommendedPlansRequestDto spotExceptionsRecommendedPlansRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionService>().GetSpotExceptionsRecommendedPlans(spotExceptionsRecommendedPlansRequest));
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
    }
}
