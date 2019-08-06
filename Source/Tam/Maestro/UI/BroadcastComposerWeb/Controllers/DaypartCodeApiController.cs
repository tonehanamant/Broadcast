using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/DaypartCodes")]
    public class DaypartCodeApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public DaypartCodeApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(DaypartCodeApiController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        /// <summary>
        /// Lists all the daypart codes in the system
        /// </summary>
        /// <returns>List of daypart codes</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<DaypartCodeDto>> GetAllDaypartCodes()
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IDaypartCodeService>().GetAllDaypartCodes());
        }

        /// <summary>
        /// Gets the daypart code defaults.
        /// </summary>
        /// <returns>List of <see cref="DaypartCodeDefaultDto"/></returns>
        [HttpGet]
        [Route("Defaults")]
        public BaseResponse<List<DaypartCodeDefaultDto>> GetDefaultDaypartCodes()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IDaypartCodeService>().GetDaypartCodeDefaults());
        }
    }
}