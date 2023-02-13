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
        /// Gets the spot exception out of spec plan inventory sources.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecPlansRequest">The out of spec plans request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-plan-inventory-sources")]
        public async Task<BaseResponse<List<string>>> GetOutOfSpecPlanInventorySources(OutOfSpecPlansRequestDto spotExceptionsOutOfSpecPlansRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().GetOutOfSpecPlanInventorySourcesAsync(spotExceptionsOutOfSpecPlansRequest);
            return _ConvertToBaseResponse(() => result);
        }

        /// <summary>
        /// Gets the spot exceptions out of spec spot inventory sources.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-spot-inventory-sources")]
        public async Task<BaseResponse<List<OutOfSpecSpotInventorySourcesDto>>> GetOutOfSpecSpotInventorySourcesAsync(OutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().GetOutOfSpecSpotInventorySourcesAsync(spotExceptionsOutOfSpecSpotsRequest);
            return _ConvertToBaseResponse(() => result);
        }

        /// <summary>
        /// Gets the spot exceptions out of spec spot reason codes.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-spot-reason-codes")]
        public async Task<BaseResponse<List<OutOfSpecSpotReasonCodeResultsDto>>> GetOutOfSpecSpotReasonCodesAsync(OutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().GetOutOfSpecSpotReasonCodesAsync(spotExceptionsOutOfSpecSpotsRequest);

            return _ConvertToBaseResponse(() => result);
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
        /// Gets the out of spec done plans with the  basis of filter inventory sources
        /// </summary>
        /// <param name="OutOfSpecsPlansIncludingFiltersDoneRequest">week start date and end date and inventory sources</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-plans-done")]
        public async Task<BaseResponse<List<OutOfSpecPlansResult>>> GetSpotExceptionsOutofSpecsV2PlansDone(OutOfSpecPlansIncludingFiltersRequestDto OutOfSpecsPlansIncludingFiltersDoneRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().GetOutOfSpecPlansDoneAsync(OutOfSpecsPlansIncludingFiltersDoneRequest);

            return _ConvertToBaseResponse(() => result);
        }
    }
}
