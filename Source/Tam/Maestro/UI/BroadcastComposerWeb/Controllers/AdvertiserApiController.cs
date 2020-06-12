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
        public AdvertiserApiController(BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(new ControllerNameRetriever(typeof(AdvertiserApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Returns all advertisers
        /// </summary>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<AdvertiserDto>> GetAdvertisers()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IAdvertiserService>().GetAdvertisers());
        }
    }
}
