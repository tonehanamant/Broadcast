using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.RecommendedPlans;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Entities.SpotExceptions.Unposted;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;
using Services.Broadcast.Entities.SpotExceptions;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v2/spot-exceptions")]
    public class SpotExceptionApiV2Controller : BroadcastControllerBase
    {
        public SpotExceptionApiV2Controller(BroadcastApplicationServiceFactory applicationServiceFactory) :
          base(new ControllerNameRetriever(typeof(PlanIsciApiController).Name), applicationServiceFactory)
        {
        }
        /// <summary>
        /// Gets the spot exceptions out of spec reason codes.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-spot-reason-codes")]
        public async Task<BaseResponse<List<SpotExceptionsOutOfSpecReasonCodeResultDtoV2>>> GetSpotExceptionsOutOfSpecReasonCodes_V1(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                 .GetSpotExceptionsOutOfSpecReasonCodesAsyncV2(spotExceptionsOutOfSpecSpotsRequest);

            return _ConvertToBaseResponse(() => result);
        }

        /// <summary>
        /// Gets the count and inventory sources
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("~/api/v2/spot-exceptions/out-of-spec-spot-inventory-sources")]
        public async Task<BaseResponse<List<SpotExceptionOutOfSpecSpotInventorySourcesDtoV2>>> GetSpotExceptionsOutOfSpecSpotInventorySourcesAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().GetSpotExceptionsOutOfSpecSpotInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest);
            return _ConvertToBaseResponse(() => result);
        }
    }
}
