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
    [RoutePrefix("api/v1/ContainTypes")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class ContainTypeApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public ContainTypeApiController(
            IWebLogger logger, 
            BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(DaypartCodeApiController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        /// <summary>
        /// Gets the contain types.
        /// </summary>
        /// <returns>A list of contain type objects.</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<LookupDto>> GetContainTypes()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IContainTypeService>().GetContainTypes());
        }
    }
}
