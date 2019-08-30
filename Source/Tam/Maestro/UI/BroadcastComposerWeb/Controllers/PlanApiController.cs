using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Plan;
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
        /// Gets the plan statuses (This endpoint was added for testing purposes only).
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<PlanDto> GetPlanById(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetPlan(planId));
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
        /// Calculats the specified plan budget based on 2 values.
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
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().PlanGloalBreakdownTypes());
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
    }
}
