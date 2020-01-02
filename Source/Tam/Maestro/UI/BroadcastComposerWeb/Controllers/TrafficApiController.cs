using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Web.Http;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/Traffic")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class TrafficApiController : BroadcastControllerBase
    {
        private readonly IWebLogger _Logger;

        public TrafficApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof (TrafficApiController).Name), applicationServiceFactory)
        {
            _Logger = logger;
        }

        [HttpGet]
        [Route("GetTraffic")]
        public BaseResponse<TrafficDisplayDto> GetTraffic()
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<ITrafficService>()
                            .GetTrafficProposals(DateTime.Now, ProposalEnums.ProposalStatusType.Contracted));
        }

    }
}
