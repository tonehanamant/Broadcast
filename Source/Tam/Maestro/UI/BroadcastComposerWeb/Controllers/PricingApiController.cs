using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Pricing")]
    public class PricingApiController : BroadcastControllerBase
    {
        public PricingApiController(BroadcastApplicationServiceFactory applicationServiceFactory) :
            base(new ControllerNameRetriever(typeof(PricingApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Queues pricing job
        /// </summary>
        /// <param name="planPricingRequestDto">
        /// ProprietaryInventory is a list of proprietary summary ids
        /// </param>
        [HttpPost]
        [Route("Queue")]
        public BaseResponse<PlanPricingJob> Queue(PricingParametersWithoutPlanDto planPricingRequestDto)
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>()
                    .QueuePricingJob(planPricingRequestDto, DateTime.Now, _GetCurrentUserFullName()));
        }

        [HttpPost]
        [Route("Execution")]
        public BaseResponse<CurrentPricingExecution> GetCurrentPricingExecution(int jobId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetCurrentPricingExecutionByJobId(jobId));
        }

        /// <summary>
        /// Get programs data from the lastest pricing execution
        /// </summary>
        /// <param name="jobId">Job ID</param>
        /// <returns>Programs from the lastest pricing execution</returns>
        [HttpGet]
        [Route("Programs/{jobId}")]
        public BaseResponse<PricingProgramsResultDto> GetPrograms(int jobId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetProgramsByJobId(jobId));
        }

        [HttpGet]
        [Route("Bands/{jobId}")]
        public BaseResponse<PlanPricingBandDto> GetPricingBands(int jobId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPricingBandsByJobId(jobId));
        }

        [HttpGet]
        [Route("Markets/{jobId}")]
        public BaseResponse<PlanPricingResultMarketsDto> GetPricingMarkets(int jobId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetMarketsByJobId(jobId));
        }

        [HttpGet]
        [Route("Stations/{jobId}")]
        public BaseResponse<PlanPricingStationResultDto> GetStations(int jobId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetStationsByJobId(jobId));
        }

        [HttpPost]
        [Route("CancelExecution")]
        public BaseResponse<PlanPricingResponseDto> CancelCurrentPricingExecution(int jobId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().CancelCurrentPricingExecutionByJobId(jobId));
        }
    }
}
