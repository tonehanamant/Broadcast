using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Isci;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;


namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/plan-iscis")]
    public class PlanIsciApiController : BroadcastControllerBase
    {
        public PlanIsciApiController(BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(new ControllerNameRetriever(typeof(PlanIsciApiController).Name), applicationServiceFactory)
        {
        }
        /// <summary>
        /// Endpoint for listing iScis based on search key in the system.
        /// </summary>
        /// <returns>List of iscis</returns>
        [HttpPost]
        [Route("available-isci")]
        public BaseResponse<List<IsciListItemDto>> GetAvailableIscis(IsciSearchDto isciSearch)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IPlanIsciService>().GetAvailableIscis(isciSearch));
        }

        /// <summary>
        /// Gets media months
        /// </summary>
        /// <returns>List of MediaMonthDto object</returns>
        [HttpGet]
        [Route("media-months")]
        public BaseResponse<List<MediaMonthDto>> GetMediaMonths()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanIsciService>().GetMediaMonths());
        }
    }

}
