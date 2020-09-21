using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/standarddaypart")]
    public class StandardDaypartApiController : BroadcastControllerBase
    {
        public StandardDaypartApiController(BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(new ControllerNameRetriever(typeof(StandardDaypartApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Gets all standard dayparts.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<StandardDaypartDto>> GetAllStandardDayparts()
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IStandardDaypartService>().GetAllStandardDayparts());
        }

        /// <summary>
        /// Gets all standard dayparts with all data.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("alldata")]
        public BaseResponse<List<StandardDaypartFullDto>> GetAllStandardDaypartsWithAllData()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IStandardDaypartService>()
                .GetAllStandardDaypartsWithAllData());
        }
    }
}