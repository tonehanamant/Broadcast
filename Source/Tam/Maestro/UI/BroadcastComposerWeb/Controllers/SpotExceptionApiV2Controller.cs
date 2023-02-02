using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    /// <summary></summary>
    [RoutePrefix("api/v2/spot-exceptions")]
    public class SpotExceptionApiV2Controller : BroadcastControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpotExceptionApiV2Controller" /> class.
        /// </summary>
        /// <param name="applicationServiceFactory">The application service factory.</param>
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

        /// <summary>
        /// Generates the inventory market affiliates report.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("GenerateOutOfSpecReport")]
        [Authorize]
        public BaseResponse<Guid> GenerateOutOfSpecReport([FromBody] OutOfSpecExportRequestDto request)
        {
            var fullName = _GetCurrentUserFullName();
            var appDataPath = _GetAppDataPath();

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .GenerateOutOfSpecExportReport(request, fullName, DateTime.Now, appDataPath));
        }

        /// <summary>
        /// Triggers the decision synchronize.
        /// </summary>
        /// <param name="triggerDecisionSyncRequest">The trigger decision synchronize request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("trigger-decision-sync")]
        public async Task<BaseResponse<bool>> TriggerDecisionSync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsServiceV2>().TriggerDecisionSync(triggerDecisionSyncRequest);

            return _ConvertToBaseResponse(() => result);
        }
    }
}
