using Common.Services.ApplicationServices;
using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Services.Broadcast.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Plan")]
    public class PlanApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public PlanApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(PlanApiController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
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
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().SavePlan(newPlan, Identity.Name, DateTime.Now));
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
        public BaseResponse<PlanDeliveryBudget> Calculator(PlanDeliveryBudget planBudget)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().Calculate(planBudget));
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
        /// <returns>WeeklyBreakdownResponse object containing the weekly breakdown</returns>
        [HttpPost]
        [Route("WeeklyBreakdown")]
        public BaseResponse<WeeklyBreakdownResponseDto> CalculateWeeklyBreakdown(WeeklyBreakdownRequest request)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().CalculatePlanWeeklyGoalBreakdown(request));
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
        public BaseResponse<ReleaseLockResponse> UnlockPlan(int planId)
        {
            var key = KeyHelper.GetPlanLockingKey(planId);
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IBroadcastLockingManagerApplicationService>().ReleaseObject(key));
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
        public BaseResponse<List<QuarterDetailDto>> GetCurrentQuarters()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetCurrentQuarters(DateTime.Now));
        }
    }
}
