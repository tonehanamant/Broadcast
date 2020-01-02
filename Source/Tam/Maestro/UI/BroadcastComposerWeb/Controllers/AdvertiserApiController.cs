using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Advertisers")]
    public class AdvertiserApiController : BroadcastControllerBase
    {
        public AdvertiserApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(AdvertiserApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Returns advertisers for a specific agency
        /// </summary>
        /// <param name="agencyId">Agency id</param>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<AdvertiserDto>> GetAdvertisersByAgencyId(int agencyId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IAdvertiserService>().GetAdvertisersByAgencyId(agencyId));
        }
    }
}
