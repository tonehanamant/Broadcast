using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Entities.SpotExceptions.Unposted;

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
        /// Gets the out of spec plans to do plans.
        /// </summary>
        /// <param name="outOfSpecsPlansIncludingFiltersDoneRequest">The out of specs plans request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-plans-todo")]
        public async Task<BaseResponse<List<OutOfSpecPlansResultDto>>> GetOutOfSpecPlansToDoPlans(OutOfSpecPlansRequestDto outOfSpecsPlansIncludingFiltersDoneRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().GetOutOfSpecPlansToDoAsync(outOfSpecsPlansIncludingFiltersDoneRequest);

            return _ConvertToBaseResponse(() => result);
        }

        /// <summary>
        /// Gets the out of spec done plans with the  basis of filter inventory sources
        /// </summary>
        /// <param name="outOfSpecsPlansIncludingFiltersDoneRequest">week start date and end date and inventory sources</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-plans-done")]
        public async Task<BaseResponse<List<OutOfSpecPlansResultDto>>> GetOutOfSpecPlansDoneAsync(OutOfSpecPlansRequestDto outOfSpecsPlansIncludingFiltersDoneRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().GetOutOfSpecPlansDoneAsync(outOfSpecsPlansIncludingFiltersDoneRequest);

            return _ConvertToBaseResponse(() => result);
        }

        /// <summary>
        /// Gets the spot exception out of spec plan inventory sources.
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecPlansRequest">The out of spec plans request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-plan-inventory-sources")]
        public async Task<BaseResponse<List<string>>> GetOutOfSpecPlanInventorySourcesAsync(OutOfSpecPlanInventorySourcesRequestDto spotExceptionsOutOfSpecPlansRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().GetOutOfSpecPlanInventorySourcesAsync(spotExceptionsOutOfSpecPlansRequest);
            return _ConvertToBaseResponse(() => result);
        }


        /// <summary>
        /// Saves the out of spec plan decisions.
        /// </summary>
        /// <param name="saveOutOfSpecPlanAcceptanceRequest">The save out of spec plan decisions request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-plan-acceptance-todo")]
        public BaseResponse<bool> SaveOutOfSpecPlanAcceptance(SaveOutOfSpecPlanAcceptanceRequestDto saveOutOfSpecPlanAcceptanceRequest)
        {
            var userName = _GetCurrentUserFullName();
            var result = _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().SaveOutOfSpecPlanAcceptance(saveOutOfSpecPlanAcceptanceRequest, userName);

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
        /// Gets the programs.
        /// </summary>
        /// <param name="programNameQuery">The program name query.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("out-of-spec-spot-programs")]
        public async Task<BaseResponse<List<OutOfSpecSpotProgramsDto>>> GetProgramsAsync(string programNameQuery)
        {
            var fullName = _GetCurrentUserFullName();

            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().GetOutOfSpecSpotProgramsAsync(programNameQuery, fullName);

            return _ConvertToBaseResponse(() => result);
        }

        /// <summary>
        /// save the buld edit for out of spec done
        /// </summary>
        [HttpPost]
        [Route("out-of-spec-spot-bulk-edit-done")]
        public BaseResponse<bool> SaveOutOfSpecSpotBulkEditDoneAsync(OutOfSpecBulkEditRequestDto outOfSpecBulkEditRequest)
        {
            var userName = _GetCurrentUserFullName();
            
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .SaveOutOfSpecSpotsBulkEdit(outOfSpecBulkEditRequest, userName));
        }

        /// <summary>
        /// Gets the unposted spot exceptions.
        /// </summary>
        /// <param name="outOfSpecUnpostedRequest">The spot exception unposted request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-unposted")]
        public async Task<BaseResponse<OutOfSpecUnpostedResultsDto>> GetOutOfSpecUnpostedAsync(OutOfSpecUnpostedRequestDto outOfSpecUnpostedRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsUnpostedServiceV2>().GetOutOfSpecUnpostedAsync(outOfSpecUnpostedRequest);

            return _ConvertToBaseResponse(() => result);
        }

        /// <summary>
        /// Triggers the decision synchronize.
        /// </summary>
        /// <param name="triggerDecisionSyncRequest">The trigger decision synchronize request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("trigger-decision-sync")]
        public async Task<BaseResponse<bool>> TriggerDecisionSyncAsync(TriggerDecisionSyncRequestDto triggerDecisionSyncRequest)
        {
            var result = await _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsServiceV2>().TriggerDecisionSyncAsync(triggerDecisionSyncRequest);

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
    }
}
