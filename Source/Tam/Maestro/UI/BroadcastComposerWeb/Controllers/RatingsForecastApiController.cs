using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Web.Http;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/RatingsForecast")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class RatingsForecastApiController : BroadcastControllerBase
    {
        public RatingsForecastApiController(
            BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(new ControllerNameRetriever(typeof(ContainTypeApiController).Name), applicationServiceFactory)
        {
        }

        
        [HttpPost]
        [Route("ForecastRatings")]
        public BaseResponse<RatingsForecastResponseDto> ForecastRatings(RatingsForecastRequestDto request)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IRatingForecastService>().ForecastRatings(request));
        }
    }
}
