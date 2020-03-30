using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/ShowTypes")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class ShowTypeApiController : BroadcastControllerBase
    {
        public ShowTypeApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(ShowTypeApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Gets the show types.
        /// </summary>
        /// <returns>A list of show type objects.</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<LookupDto>> GetShowTypes()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IShowTypeService>().GetShowTypes());
        }
    }
}
