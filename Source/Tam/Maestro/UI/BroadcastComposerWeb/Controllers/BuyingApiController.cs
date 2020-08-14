using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Buying;
using Services.Broadcast.Entities.Buying;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Buying/Plans")]
    public class BuyingApiController : BroadcastControllerBase
    {
        public BuyingApiController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base( new ControllerNameRetriever(typeof(BuyingApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Get a list of plan buying
        /// </summary>
        /// <returns>List of plan buying</returns>
        [HttpPost]
        [Route("")]
        public BaseResponse<List<PlanBuyingListingItem>> GetPlansBuying(PlanBuyingListRequest request)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IBuyingService>().GetPlansBuying(request));
        }

        /// <summary>
        /// Gets the plan buying by id.
        /// </summary>
        /// <returns>Plans buying data</returns>
        [HttpGet]
        [Route("{planId}")]
        public BaseResponse<PlanBuying> GetPlanBuyingById(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IBuyingService>().GetPlanBuyingById(planId));
        }

        /// <summary>
        /// Save plan buying data
        /// </summary>
        /// <param name="planId">The id of the plan.</param>
        /// <param name="plan">Plan data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("{planId}")]
        public BaseResponse<bool> SavePlanBuying(int planId, PlanBuyingRequest plan)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IBuyingService>().SavePlanBuying(planId, plan));
        }

        /// <summary>
        /// Gets all the time frames.
        /// </summary>
        /// <returns>An object with a list of time frames.</returns>
        [HttpGet]
        [Route("TimeFrames")]
        public BaseResponse<List<LookupDto>> GetTimeFrames()
        {
            return
               _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IBuyingService>().GetTimeFrames());
        }

        /// <summary>
        /// Gets all the statuses.
        /// </summary>
        /// <returns>An object with a list of statuses.</returns>
        [HttpGet]
        [Route("Statuses")]
        public BaseResponse<List<LookupDto>> GetStatuses()
        {
            return
               _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IBuyingService>().GetStatuses());
        }
    }
}
