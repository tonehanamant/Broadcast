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
        /// Gets the out of spec plans to do.
        /// </summary>
        /// <param name="outOfSpecPlansRequest">The out of specs plans request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-plans-todo")]
        public BaseResponse<List<OutOfSpecPlansResultDto>> GetOutOfSpecPlansToDo(OutOfSpecPlansRequestDto outOfSpecPlansRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .GetOutOfSpecPlansToDo(outOfSpecPlansRequest));
        }

        /// <summary>
        /// Gets the out of spec plans done
        /// </summary>
        /// <param name="outOfSpecPlansRequest">The out of specs plans request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-plans-done")]
        public BaseResponse<List<OutOfSpecPlansResultDto>> GetOutOfSpecPlansDone(OutOfSpecPlansRequestDto outOfSpecPlansRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .GetOutOfSpecPlansDone(outOfSpecPlansRequest));
        }

        /// <summary>
        /// Gets the spot exception out of spec plan inventory sources.
        /// </summary>
        /// <param name="outOfSpecPlanInventorySourcesRequest">The out of spec plans request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-plan-inventory-sources")]
        public BaseResponse<List<string>> GetOutOfSpecPlanInventorySources(OutOfSpecPlanInventorySourcesRequestDto outOfSpecPlanInventorySourcesRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .GetOutOfSpecPlanInventorySources(outOfSpecPlanInventorySourcesRequest));
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

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .SaveOutOfSpecPlanAcceptance(saveOutOfSpecPlanAcceptanceRequest, userName));
        }

        /// <summary>
        /// Gets the spot exceptions out of spec spot inventory sources.
        /// </summary>
        /// <param name="outOfSpecSpotsRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-spot-inventory-sources")]
        public BaseResponse<List<OutOfSpecSpotInventorySourcesDto>> GetOutOfSpecSpotInventorySources(OutOfSpecSpotsRequestDto outOfSpecSpotsRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .GetOutOfSpecSpotInventorySources(outOfSpecSpotsRequest));
        }

        /// <summary>
        /// Gets the spot exceptions out of spec spot reason codes.
        /// </summary>
        /// <param name="outOfSpecSpotsRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-spot-reason-codes")]
        public BaseResponse<List<OutOfSpecSpotReasonCodeResultsDto>> GetOutOfSpecSpotReasonCodes(OutOfSpecSpotsRequestDto outOfSpecSpotsRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .GetOutOfSpecSpotReasonCodes(outOfSpecSpotsRequest));
        }

        /// <summary>
        /// Gets the programs.
        /// </summary>
        /// <param name="programNameQuery">The program name query.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("out-of-spec-spot-programs")]
        public BaseResponse<List<OutOfSpecSpotProgramsDto>> GetPrograms(string programNameQuery)
        {
            var fullName = _GetCurrentUserFullName();

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .GetOutOfSpecSpotPrograms(programNameQuery, fullName));
        }

        /// <summary>
        /// Saves the out of spec spot comment to do.
        /// </summary>
        /// <param name="saveOutOfSpecSpotCommentsRequest">The save out of spec spot comments request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-spot-comments-todo")]
        public BaseResponse<bool> SaveOutOfSpecSpotCommentsToDo(SaveOutOfSpecSpotCommentsRequestDto saveOutOfSpecSpotCommentsRequest)
        {
            var userName = _GetCurrentUserFullName();

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .SaveOutOfSpecSpotCommentsToDo(saveOutOfSpecSpotCommentsRequest, userName));
        }

        /// <summary>
        /// save the buld edit for out of spec done
        /// </summary>
        [HttpPost]
        [Route("out-of-spec-spot-comments-done")]
        public BaseResponse<bool> SaveOutOfSpecSpotDoneComments(SaveOutOfSpecSpotCommentsRequestDto outOfSpecCommentRequest)
        {
            var userName = _GetCurrentUserFullName();

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .SaveOutOfSpecCommentsDone(outOfSpecCommentRequest, userName));
        }

        /// <summary>
        /// Get the out of spec spot to do.
        /// </summary>
        /// <param name="OutOfSpecSpotsRequest">The save out of spec spot comments request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-spots-todo")]
        public BaseResponse<List<OutOfSpecSpotsResultDto>> GetOutOfSpecSpotsToDo(OutOfSpecSpotsRequestDto OutOfSpecSpotsRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .GetOutOfSpecSpotsToDo(OutOfSpecSpotsRequest));
        }

        /// <summary>
        /// save the bulk edit for out of spec todo
        /// </summary>
        [HttpPost]
        [Route("out-of-spec-spot-bulk-edit-todo")]
        public BaseResponse<bool> SaveOutOfSpecSpotBulkEditToDo(SaveOutOfSpecSpotBulkEditRequestDto saveOutOfSpecSpotBulkEditRequest)
        {
            var userName = _GetCurrentUserFullName();

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .SaveOutOfSpecSpotBulkEditToDo(saveOutOfSpecSpotBulkEditRequest, userName));
        }


        /// <summary>
        /// save the bulk edit for out of spec done
        /// </summary>
        [HttpPost]
        [Route("out-of-spec-spot-bulk-edit-done")]
        public BaseResponse<bool> SaveOutOfSpecSpotBulkEditDone(SaveOutOfSpecSpotBulkEditRequestDto saveOutOfSpecSpotBulkEditRequest)
        {
            var userName = _GetCurrentUserFullName();
            
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .SaveOutOfSpecSpotsBulkEditDone(saveOutOfSpecSpotBulkEditRequest, userName));
        }

       /// <summary>
       /// save the out of spec to do items 
       /// </summary>
        [HttpPost]
        [Route("out-of-spec-spot-edit-todo")]
        public BaseResponse<bool> SaveOutOfSpecSpotEditToDo(OutOfSpecEditRequestDto outOfSpecEditRequest)
        {
            var userName = _GetCurrentUserFullName();

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .SaveOutOfSpecSpotEditToDo(outOfSpecEditRequest, userName));
        }

        /// <summary>
        /// save the single edit for out of spec done
        /// </summary>
        [HttpPost]
        [Route("out-of-spec-spot-edit-done")]
        public BaseResponse<bool> SaveOutOfSpecSpotEditDone(OutOfSpecEditRequestDto outOfSpecEditRequest)
        {
            var userName = _GetCurrentUserFullName();

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>()
                .SaveOutOfSpecSpotsEditDone(outOfSpecEditRequest, userName));
        }
       
        /// <summary>
        /// Gets the unposted spot exceptions.
        /// </summary>
        /// <param name="outOfSpecUnpostedRequest">The spot exception unposted request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-unposted")]
        public BaseResponse<OutOfSpecUnpostedResultsDto> GetOutOfSpecUnposted(OutOfSpecUnpostedRequestDto outOfSpecUnpostedRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsUnpostedServiceV2>()
                .GetOutOfSpecUnposted(outOfSpecUnpostedRequest));
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
            return await _ConvertToBaseResponseAsync(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsServiceV2>()
                .TriggerDecisionSyncAsync(triggerDecisionSyncRequest));
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
        /// Gets the spot exceptions outof spec advertisers.
        /// </summary>
        /// <param name="outofSpecPlanAdvertisersRequest">The spot exceptions outof spec advertisers request.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("out-of-spec-advertisers")]
        public BaseResponse<List<MasterIdName>> GetOutofSpecAdvertisers(OutOfSpecPlanAdvertisersRequestDto outofSpecPlanAdvertisersRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().GetOutOfSpecAdvertisers(outofSpecPlanAdvertisersRequest));
        }

        /// <summary>
        /// Saves the out of spec decisions plans.
        /// </summary>
        /// <param name="outOfSpecTodoAcceptanceRequest">The spot exceptions out of spec save request.</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("out-of-spec-spot-acceptance-todo")]
        public BaseResponse<bool> SaveOutofSpecToDoAcceptanceAndComment(OutOfSpecSaveAcceptanceRequestDto outOfSpecTodoAcceptanceRequest)
        {
            var userName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().SaveOutOfSpecDecisionsToDoPlans(outOfSpecTodoAcceptanceRequest, userName));
        }

        /// <summary>
        /// Saves the out of spec decisions plans.
        /// </summary>
        /// <param name="outOfSpecTodoAcceptanceRequest">The spot exceptions out of spec save request.</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("out-of-spec-spot-acceptance-done")]
        public BaseResponse<bool> SaveOutofSpecDoneAcceptanceAndComment(OutOfSpecSaveAcceptanceRequestDto outOfSpecTodoAcceptanceRequest)
        {
            var userName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ISpotExceptionsOutOfSpecServiceV2>().SaveOutOfSpecDecisionsDonePlans(outOfSpecTodoAcceptanceRequest, userName));
        }
    }
}
