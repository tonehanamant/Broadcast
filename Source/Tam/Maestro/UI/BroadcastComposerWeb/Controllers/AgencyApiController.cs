using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Agencies")]
    public class AgencyApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public AgencyApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(DaypartCodeApiController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        /// <summary>
        /// Gets the agencies.
        /// </summary>
        /// <returns>A list of agency objects.</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<AgencyDto>> GetAgencies()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IAgencyService>().GetAgencies());
        }
    }
}
