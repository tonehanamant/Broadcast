using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Plan")]
    public class PlanApiController : BroadcastControllerBase
    {
        public PlanApiController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(new ControllerNameRetriever(typeof(PlanApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Saves the plan.
        /// </summary>
        /// <param name="newPlan">The new plan.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        [Authorize]
        public BaseResponse<int> SavePlan(PlanDto newPlan)
        {
            const string SW_KEY_TOTAL_DURATION = "Total duration";
            const string SW_KEY_GET_USER_NAME = "Get Username";
            const string SW_KEY_GET_SERVICE = "Get Service";
            const string SW_KEY_SAVE_PLAN = "Save Plan";

            var processTimer = new ProcessWorkflowTimers();
            processTimer.Start(SW_KEY_TOTAL_DURATION);

            processTimer.Start(SW_KEY_GET_USER_NAME);
            var fullName = _GetCurrentUserFullName();
            processTimer.End(SW_KEY_GET_USER_NAME);

            processTimer.Start(SW_KEY_GET_SERVICE);
            var service = _ApplicationServiceFactory.GetApplicationService<IPlanService>();
            processTimer.End(SW_KEY_GET_SERVICE);

            processTimer.Start(SW_KEY_SAVE_PLAN);
            var result = _ConvertToBaseResponse(() => service.SavePlan(newPlan, fullName, DateTime.Now));
            processTimer.End(SW_KEY_SAVE_PLAN);

            processTimer.End(SW_KEY_TOTAL_DURATION);
            var timersReport = processTimer.ToString();
            _LogInfo($"SavePlan finished for PlanId '{newPlan.Id}' with result '{result.Data}'. Durations : '{timersReport}'");

            return result;
        }

        /// <summary>
        /// Gets the plan statuses.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("Statuses")]
        public BaseResponse<List<LookupDto>> GetStatuses()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetPlanStatuses());
        }

        /// <summary>
        /// Gets the plan by id.
        /// </summary>
        /// <returns>PlanDto object</returns>
        [HttpGet]
        [Route("{planId}")]
        public BaseResponse<PlanDto> GetPlanById(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetPlan(planId));
        }

        /// <summary>
        /// Gets the plan by id.
        /// </summary>
        /// <returns>PlanDto object</returns>
        [HttpGet]
        [Route("~/api/v2/Plan/{planId}")]
        public BaseResponse<PlanDto_v2> GetPlanById_v2(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetPlan_v2(planId));
        }

        /// <summary>
        /// Checks if the plan has a draft and returns the draft id
        /// </summary>
        /// <returns>Id of the draft</returns>
        [HttpGet]
        [Route("{planId}/draft")]
        public BaseResponse<int> CheckForDraft(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().CheckForDraft(planId));
        }

        /// <summary>
        /// Gets the plan by id and version id.
        /// </summary>
        /// <returns>PlanDto object</returns>
        [HttpGet]
        [Route("{planId}/versions/{versionId}")]
        public BaseResponse<PlanDto> GetVersionForPlan(int planId, int versionId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetPlan(planId, versionId));
        }

        /// <summary>
        /// Gets the currencies.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("Currencies")]
        public BaseResponse<List<LookupDto>> GetCurrencies()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetPlanCurrencies());
        }
                
        /// <summary>
        /// Calculates the specified plan budget based on 2 values.
        /// </summary>
        /// <param name="planBudget">The plan budget.</param>
        /// <returns>PlanDeliveryBudget object containing the calculated value</returns>
        [HttpPost]
        [Route("CalculateBudget")]
        public BaseResponse<PlanDeliveryBudget> CalculateBudget(PlanDeliveryBudget planBudget)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().CalculateBudget(planBudget));
        }

        [HttpPost]
        [Route("posting-type-budget-goals")]
        public BaseResponse<List<PlanDeliveryPostingTypeBudget>> CalculatePostingTypeBudgets(PlanDeliveryPostingTypeBudget budgetRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().CalculatePostingTypeBudgets(budgetRequest));
        }

        /// <summary>
        /// Gets the delivery types.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("GloalBreakdownTypes")]
        public BaseResponse<List<LookupDto>> GetDeliveryTypes()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().PlanGoalBreakdownTypes());
        }

        /// <summary>
        /// Calculates the weekly breakdown.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="clearAll"></param>
        /// <returns>WeeklyBreakdownResponse object containing the weekly breakdown</returns>
        [HttpPost]
        [Route("WeeklyBreakdown")]
        public BaseResponse<WeeklyBreakdownResponseDto> CalculateWeeklyBreakdown(WeeklyBreakdownRequest request, bool clearAll = false )
        {
            return _ConvertToBaseResponse(() =>  _ApplicationServiceFactory.GetApplicationService<IPlanService>().CalculatePlanWeeklyGoalBreakdown(request, clearAll));
        }

        /// <summary>
        /// Locks the plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{planId}/Lock")]
        [Authorize] // Locking only works with Authorize, RestrictedAccess doesn't
        public BaseResponse<PlanLockResponse> LockPlan(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().LockPlan(planId));
        }

        /// <summary>
        /// Unlocks the plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{planId}/Unlock")]
        [Authorize]
        public BaseResponse<BroadcastReleaseLockResponse> UnlockPlan(int planId)
        {
            var key = KeyHelper.GetPlanLockingKey(planId);
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().UnlockPlan(planId));
        }

        /// <summary>
        /// Gets the plan defaults.
        /// </summary>
        /// <returns>Gets the default values for creating a plan in the form of <see cref="PlanDefaultsDto"/></returns>
        [HttpGet]
        [Route("Defaults")]
        public BaseResponse<PlanDefaultsDto> GetPlanDefaults()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetPlanDefaults());
        }

        /// <summary>
        /// Gets the currencies.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("{planId}/history")]
        public BaseResponse<List<PlanVersionDto>> GetPlanHistory(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetPlanHistory(planId));
        }

        /// <summary>
        /// Delete the draft on a plan.
        /// </summary>
        /// <returns>True if the delete was successful</returns>
        [HttpGet]
        [Route("{planId}/DeleteDraft")]
        public BaseResponse<bool> DeleteDraft(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().DeletePlanDraft(planId));
        }

        [HttpGet]
        [Route("CurrentQuarters")]
        public BaseResponse<CurrentQuartersDto> GetCurrentQuarters()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetCurrentQuarters(DateTime.Now));
        }

        /// <summary>
        /// Calculates the creative length weight.
        /// </summary>
        /// <param name="request">Creative lengths set on the plan.</param>
        /// <returns>List of creative lengths with calculated values</returns>
        [HttpPost]
        [Route("CreativeLength")]
        public BaseResponse<List<CreativeLength>> CalculateCreativeLengthWeight(List<CreativeLength> request)
        {
            return _ConvertToBaseResponse(() => 
                _ApplicationServiceFactory.GetApplicationService<IPlanService>()
                    .CalculateCreativeLengthWeight(request));
        }

        /// <summary>
        /// Calculates the length make up table data
        /// </summary>
        /// <param name="request"></param>
        /// <returns>List of LengthMakeUpTableRow objects</returns>
        [HttpPost]
        [Route("LengthMakeUp")]
        public BaseResponse<List<LengthMakeUpTableRow>> CalculateLengthMakeUpTable(LengthMakeUpRequest request)
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPlanService>()
                    .CalculateLengthMakeUpTable(request));
        }

        /// <summary>
        /// Calculates and distributes the market weights.
        /// </summary>
        [HttpGet]
        [Route("available-markets/defaults")]
        public BaseResponse<PlanAvailableMarketCalculationResult> CalculateDefaultPlanAvailableMarkets()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>()
                .CalculateDefaultPlanAvailableMarkets());
        }

        /// <summary>
        /// Calculates and distributes the market weights.
        /// </summary>
        /// <param name="request">The request.</param>
        [HttpPost]
        [Route("available-markets/modified")]
        public BaseResponse<PlanAvailableMarketCalculationResult> CalculateMarketWeightChange(PlanCalculateMarketWeightChangeRequest request)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>()
                .CalculateMarketWeightChange(request.AvailableMarkets, request.ModifiedMarketCode, request.UserEnteredValue));
        }

        /// <summary>
        /// Adds the markets and calculates and distributes the weights.
        /// </summary>
        /// <param name="request">The request.</param>
        [HttpPost]
        [Route("available-markets/added")]
        public BaseResponse<PlanAvailableMarketCalculationResult> CalculateMarketsAdded(PlanCalculateMarketAddedRequest request)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>()
                .CalculateMarketsAdded(request.BeforeMarkets, request.AddedMarkets));
        }

        /// <summary>
        /// Removes the markets and calculates and distributes the weights.
        /// </summary>
        /// <param name="request">The request.</param>
        [HttpPost]
        [Route("available-markets/removed")]
        public BaseResponse<PlanAvailableMarketCalculationResult> CalculateMarketsRemoved(PlanCalculateMarketRemovedRequest request)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>()
                .CalculateMarketsRemoved(request.BeforeMarkets, request.RemovedMarketCodes));
        }

        /// <summary>
        /// Clears the user entered values and recalculates the weights.
        /// </summary>
        [HttpPost]
        [Route("available-markets/cleared")]
        public BaseResponse<PlanAvailableMarketCalculationResult> CalculateMarketWeightsClearAll(List<PlanAvailableMarketDto> availableMarkets)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>()
                .CalculateMarketWeightsClearAll(availableMarkets));
        }

        [HttpPost]
        [Route("commit-pricing-allocation-model")]
        [Authorize]
        public BaseResponse<bool> CommitPricingAllocationModel(CommitPricingAllocationModelRequest request)
        {
            var username = _GetCurrentUserFullName();

            var result = _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>()
                .CommitPricingAllocationModel(request.PlanId, request.SpotAllocationModelMode, request.PostingType, username));

            return result;
        }
        /// <summary>
        /// Gets the Custom daypart organizations Name.
        /// </summary>
        /// <returns>List of string organizations Name</returns>
        [HttpGet]
        [Route("custom-daypart-organizations")]
        public BaseResponse<List<CustomDaypartOrganizationDto>> GetCustomDaypartOrganizations()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetCustomDaypartOrganizations());
        }

        [Authorize]
        [HttpPost]        
        [Route("{planId}/delete")]
        public BaseResponse<bool> DeletePlan(int planId)
        {
            var deletedBy = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().DeletePlan(planId, deletedBy));
        }

        [HttpPost]
        [Route("plan-daypart-update")]
        public BaseResponse<PlanDaypartUpdateResponseDto> UpdatePlanDaypart(PlanDaypartUpdateRequestDto planDaypartUpdateRequest)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().UpdatePlanDaypart(planDaypartUpdateRequest));
        }
    }
}
