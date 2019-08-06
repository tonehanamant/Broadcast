using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using System;
using System.Web.Http;
using Tam.Maestro.Web.Common;
using Tam.Maestro.Services.Cable.Entities;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Plan;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using System.Collections.Generic;

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
        public BaseResponse<int> SavePlan(PlanDto newPlan)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().SavePlan(newPlan, Identity.Name, DateTime.Now));
        }

        /// <summary>
        /// Gets the products.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("Products")]
        public BaseResponse<List<LookupDto>> GetProducts()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetProducts());
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
    }
}
