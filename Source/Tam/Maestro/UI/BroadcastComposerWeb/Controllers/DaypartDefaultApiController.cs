using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/DaypartDefaults")]
    public class DaypartDefaultApiController : BroadcastControllerBase
    {
        public DaypartDefaultApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(logger, new ControllerNameRetriever(typeof(DaypartDefaultApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Lists all the daypart defaults in the system
        /// </summary>
        /// <returns>List of daypart defaults</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<DaypartDefaultDto>> GetAllDaypartDefaults()
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IDaypartDefaultService>().GetAllDaypartDefaults());
        }

        /// <summary>
        /// Lists all the daypart defaults with all data in the system.
        /// </summary>
        /// <returns>List of <see cref="DaypartDefaultFullDto"/>List of daypart defaults</returns>
        [HttpGet]
        [Route("Defaults")]
        public BaseResponse<List<DaypartDefaultFullDto>> GetAllDaypartDefaultsWithAllData()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IDaypartDefaultService>().GetAllDaypartDefaultsWithAllData());
        }
    }
}