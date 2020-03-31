using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System.Web.Http;
using Tam.Maestro.Web.Common;
using Tam.Maestro.Services.Cable.Entities;
using System.Collections.Generic;
using Services.Broadcast.Entities.Plan;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Audiences")]
    public class AudienceApiController : BroadcastControllerBase
    {
        public AudienceApiController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(AudienceApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Gets all audiences.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<PlanAudienceDisplay>> GetAudiences()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IAudienceService>().GetAudiences());
        }
    }
}
