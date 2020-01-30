using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Web.Common;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Buying/Plans")]
    public class PlanBuyingApiController : BroadcastControllerBase
    {
        public PlanBuyingApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(PlanBuyingApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Get a list of plan buying
        /// </summary>
        /// <returns>List of plan buying</returns>
        [HttpPost]
        [Route("")]
        public BaseResponse<List<PlanBuyingListingItem>> GetPlansBuying(int StatusFilter, int FlightFilter)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetPlansBuying(StatusFilter, FlightFilter));
        }

        /// <summary>
        /// Gets the plan buying by id.
        /// </summary>
        /// <returns>Plans buying data</returns>
        [HttpGet]
        [Route("{planId}")]
        public BaseResponse<PlanBuying> GetPlanBuyingById(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetPlanBuyingById(planId));
        }

        /// <summary>
        /// Save plan buying data
        /// </summary>
        /// <param name="plan">Plan data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{planId}")]
        public BaseResponse<bool> SavePlanBuying(int planId, PlanBuyingRequest plan)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().SavePlanBuying(planId, plan));
        }
    }
}
