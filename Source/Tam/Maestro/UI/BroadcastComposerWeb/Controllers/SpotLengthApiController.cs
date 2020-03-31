using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System.Web.Http;
using Tam.Maestro.Web.Common;
using Tam.Maestro.Services.Cable.Entities;
using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/SpotLengths")]
    public class SpotLengthApiController : BroadcastControllerBase
    {
        public SpotLengthApiController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(SpotLengthApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Gets all spot lengths.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<LookupDto>> GetAllSpotLengths()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotLengthService>().GetAllSpotLengths());
        }
    }
}
