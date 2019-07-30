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
            : base(logger, new ControllerNameRetriever(typeof(InventoryApiController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        /// <summary>
        /// Get inventory sources for summaries
        /// </summary>
        /// <remarks>
        /// Get a list of inventory sources available for summary
        /// </remarks>
        [HttpPost]
        [Route("Create")]
        public BaseResponse<int> CreatePlan(CreatePlanDto newPlan)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().CreatePlan(newPlan, Identity.Name, DateTime.Now));
        }

        /// <summary>
        /// Gets the products.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpPost]
        [Route("GetProducts")]
        public BaseResponse<List<LookupDto>> GetProducts()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetProducts());
        }

        /// <summary>
        /// Gets the plan statuses.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpPost]
        [Route("GetStatuses")]
        public BaseResponse<List<LookupDto>> GetStatuses()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanService>().GetPlanStatuses());
        }
    }
}
