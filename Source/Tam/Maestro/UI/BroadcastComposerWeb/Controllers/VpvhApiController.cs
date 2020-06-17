using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Vpvh;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Vpvh")]
    public class VpvhApiController : BroadcastControllerBase
    {
        public VpvhApiController(BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(AudienceApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Returns the four books, last year and previous quarter VPVH for each audience
        /// </summary>
        /// <param name="request">Daypart and audience ids</param>
        /// <returns>Vpvh data to each audiences</returns>
        [Route("")]
        [HttpPost]
        public BaseResponse<List<VpvhResponse>> GetVPVHs(VpvhRequest request)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IVpvhService>().GetVpvhs(request));
        }

        /// <summary>
        /// Returns a list of VPVH default values for all combinations of the given audiences and all existing dayparts
        /// </summary>
        [HttpPost]
        [Route("Defaults")]
        [Authorize]
        public BaseResponse<List<VpvhDefaultResponse>> GetVpvhValues(VpvhDefaultsRequest request)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IVpvhService>().GetVpvhDefaults(request));
        }
    }
}
