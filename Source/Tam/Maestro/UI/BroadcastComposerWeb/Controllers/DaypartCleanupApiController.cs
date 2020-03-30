using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Maintenance;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/DaypartCleanup")]
    public class DaypartCleanupApiController : BroadcastControllerBase
    {
        public DaypartCleanupApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(DaypartCleanupApiController).Name), applicationServiceFactory)
        {
        }

        [HttpGet]
        [Route("")]
        public BaseResponse<List<DaypartCleanupDto>> GetAllErroneousDaypartCodes()
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IDaypartCleanupService>().FilterErroneousDayparts());
        }

        [HttpPost]
        [Route("")]
        public BaseResponse<List<DaypartCleanupDto>> RepairErroneousDaypartCodes()
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IDaypartCleanupService>().RepairErroneousDayparts());
        }
    }
}