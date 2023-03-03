using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Plan.Pricing;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Data.Entities;
using Services.Broadcast.Entities.QuoteReport;
using Services.Broadcast.Entities.InventoryProprietary;
using Services.Broadcast.Entities.Plan;
using System.Threading.Tasks;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/PricingService")]
    public class PlanPricingApiController : BroadcastControllerBase
    {
        public PlanPricingApiController(BroadcastApplicationServiceFactory applicationServiceFactory) : 
            base(new ControllerNameRetriever(typeof(PlanPricingApiController).Name), applicationServiceFactory)
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
        public BaseResponse<PlanPricingJob> Queue(PlanPricingParametersDto planPricingRequestDto)
        {
            return _ConvertToBaseResponseWithStackTrace(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>()
                    .QueuePricingJob(planPricingRequestDto, DateTime.Now, _GetCurrentUserFullName()));
        }

        [HttpPost]
        [Route("Execution")]
        public BaseResponse<CurrentPricingExecution> GetCurrentPricingExecution(int planId, int? planVersionId = null)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetCurrentPricingExecution(planId, planVersionId));
        }

        [HttpPost]               
        [Route("~/api/v2/PricingService/Execution")]
        public BaseResponse<CurrentPricingExecutions> GetAllCurrentPricingExecution(int planId, int? planVersionId = null)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetAllCurrentPricingExecutions(planId, planVersionId));
        }
        /// <summary>
        /// Get programs data from the lastest pricing execution
        /// </summary>
        /// <param name="planId">Plan ID</param>
        /// <returns>Programs from the lastest pricing execution</returns>
        [HttpGet]
        [Route("Programs/{planId}")]
        public BaseResponse<PricingProgramsResultDto> GetPrograms(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPrograms(planId));
        }

        [HttpGet]
        [Route("Programs/{planId}/{planVersionId}")]
        public BaseResponse<PricingProgramsResultDto> GetPrograms(int planId, int planVersionId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetProgramsForVersion(planId, planVersionId));
        }

        /// <summary>
        /// Get programs data from the lastest pricing execution
        /// </summary>
        /// <param name="planId">Plan ID</param>
        /// <param name="spotAllocationModelMode">Quality = 1, Efficiency = 2, Floor = 3</param>
        /// <returns>Programs from the lastest pricing execution</returns>
        [HttpGet]
        [Route("~/api/v2/PricingService/Programs/{planId}")]
        public BaseResponse<PricingProgramsResultDto_v2> GetPrograms_v2(int planId, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPrograms_v2(planId, spotAllocationModelMode));
        }

        [HttpGet]
        [Route("~/api/v2/PricingService/Programs/{planId}/{planVersionId}")]
        public BaseResponse<PricingProgramsResultDto_v2> GetPrograms_v2(int planId, int planVersionId, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetProgramsForVersion_v2(planId, planVersionId, spotAllocationModelMode));
        }

        [HttpGet]
        [Route("Bands/{planId}")]
        public BaseResponse<PlanPricingBandDto> GetPricingBands(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPricingBands(planId));
        }

        [HttpGet]
        [Route("Bands/{planId}/{planVersionId}")]
        public BaseResponse<PlanPricingBandDto> GetPricingBands(int planId, int planVersionId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPricingBandsForVersion(planId, planVersionId));
        }

        [HttpGet]
        [Route("~/api/v2/PricingService/Bands/{planId}")]
        public BaseResponse<PlanPricingBandDto_v2> GetPricingBands_v2(int planId, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPricingBands_v2(planId, spotAllocationModelMode));
        }

        [HttpGet]
        [Route("~/api/v2/PricingService/Bands/{planId}/{planVersionId}")]
        public BaseResponse<PlanPricingBandDto_v2> GetPricingBands_v2(int planId, int planVersionId, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPricingBandsForVersion_v2(planId, planVersionId, spotAllocationModelMode));
        }

        [HttpGet]
        [Route("Markets/{planId}")]
        public BaseResponse<PlanPricingResultMarketsDto> GetPricingMarkets(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetMarkets(planId));
        }

        [HttpGet]
        [Route("Markets/{planId}/{planVersionId}")]
        public BaseResponse<PlanPricingResultMarketsDto> GetPricingMarkets(int planId, int planVersionId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetMarketsForVersion(planId, planVersionId));
        }

        [HttpGet]
        [Route("~/api/v2/PricingService/Markets/{planId}")]
        public BaseResponse<PlanPricingResultMarketsDto_v2> GetPricingMarkets_v2(int planId, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetMarkets_v2(planId, spotAllocationModelMode));
        }

        [HttpGet]
        [Route("~/api/v2/PricingService/Markets/{planId}/{planVersionId}")]
        public BaseResponse<PlanPricingResultMarketsDto_v2> GetPricingMarkets_v2(int planId, int planVersionId, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetMarketsForVersion_v2(planId, planVersionId, spotAllocationModelMode));
        }

        [HttpPost]
        [Route("CancelExecution")]
        public BaseResponse<PlanPricingResponseDto> CancelCurrentPricingExecution(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().CancelCurrentPricingExecution(planId));
        }

        /// <summary>
        /// Allows checking that correct inventory is used for pricing
        /// </summary>
        /// <param name="planId">Plan ID</param>
        /// <param name="requestParameters">
        /// InventorySourceIds
        /// - pass InventorySourceIds only when you want to check what proprietary inventory could be potentially chosen
        /// - DO NOT pass InventorySourceIds if you want to check what is sent to the pricing model
        /// </param>
        [HttpPost]
        [Route("PricingApiRequestPrograms3")]
        public BaseResponse<PlanPricingApiRequestDto_v3> GetPricingProgramApiRequest_v3(int planId, PricingInventoryGetRequestParametersDto requestParameters)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPricingApiRequestPrograms_v3(planId, requestParameters));
        }

        [HttpPost]
        [Route("Inventory")]
        public BaseResponse<List<PlanPricingInventoryProgram>> GetPricingInventory(int planId, PricingInventoryGetRequestParametersDto requestParameters)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPricingInventory(planId, requestParameters));
        }

        /// <summary>
        /// Gets the unit caps.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("UnitCaps")]
        public BaseResponse<List<LookupDto>> GetUnitCaps()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetUnitCaps());
        }

        /// <summary>
        /// Gets the pricing market groups.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("MarketGroups")]
        public BaseResponse<List<LookupDto>> GetPricingMarketGroups()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPricingMarketGroups());
        }

        [HttpGet]
        [Route("PlanPricingDefaults")]
        public BaseResponse<PlanPricingDefaults> GetPlanPricingDefaults()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetPlanPricingDefaults());
        }

        [HttpGet]
        [Route("Stations/{planId}")]
        public BaseResponse<PlanPricingStationResultDto> GetStations(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetStations(planId));
        }

        [HttpGet]
        [Route("Stations/{planId}/{planVersionId}")]
        public BaseResponse<PlanPricingStationResultDto> GetStationsForVersion(int planId, int planVersionId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetStationsForVersion(planId, planVersionId));
        }

        [HttpGet]
        [Route("~/api/v2/PricingService/Stations/{planId}")]
        public BaseResponse<PlanPricingStationResultDto_v2> GetStations_v2(int planId, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetStations_v2(planId, spotAllocationModelMode));
        }

        [HttpGet]
        [Route("~/api/v2/PricingService/Stations/{planId}/{planVersionId}")]
        public BaseResponse<PlanPricingStationResultDto_v2> GetStationsForVersion_v2(int planId, int planVersionId, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GetStationsForVersion_v2(planId, planVersionId, spotAllocationModelMode));
        }

        [HttpPost]
        [Route("ExportPricingScx")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<Guid> ExportPricingScx(int planId, SpotAllocationModelMode spotAllocationModelMode,
            PostingTypeEnum postingType)
        {
            var username = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>()
            .ExportPlanPricingScx(planId, username, spotAllocationModelMode, postingType));
        }

        /// <summary>
        /// Generates the report with OpenMarket inventory prices.
        /// </summary>
        /// <returns>Identifier of the generated report</returns>
        [HttpPost]
        [Route("RunQuote")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<Guid> RunQuote([FromBody]QuoteRequestDto request)
        {
            var appDataPath = _GetAppDataPath();
            var fullName = _GetCurrentUserFullName();

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().RunQuote(request, fullName, appDataPath));
        }

        [HttpPost]
        [Route("ResultsReport")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<Guid> ResultsReport([FromBody]PlanPricingResultsReportRequest request)
        {
            var appDataPath = _GetAppDataPath();
            var fullName = _GetCurrentUserFullName();

            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>().GeneratePricingResultsReportAndSave(request, appDataPath, fullName));
        }

        /// <summary>
        /// Lists the Inventory Proprietary Summaries
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ProprietarySummaries")]
        public BaseResponse<InventoryProprietarySummaryResponse> GetInventoryProprietarySummaries(InventoryProprietarySummaryRequest request)
        {
            var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProprietarySummaryService>();

	        return _ConvertToBaseResponse(() => service.GetInventoryProprietarySummaries(request));
        }

        /// <summary>
        /// Lists the Inventory Proprietary Summaries
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PlanProprietarySummary")]
        public BaseResponse<TotalInventoryProprietarySummaryResponse> GetPlanProprietarySummaryAggregation(TotalInventoryProprietarySummaryRequest request)
        {
	        var service = _ApplicationServiceFactory.GetApplicationService<IInventoryProprietarySummaryService>();

	        return _ConvertToBaseResponse(() => service.GetPlanProprietarySummaryAggregation(request));
        }

        /// <summary>
        /// Tests the repository query for getting the Inventory Programs.
        /// Query dimensions are configured per the given Job Id.
        /// </summary>
        [HttpPost]
        [Route("TestGetProgramsForPricingModel")]
        public BaseResponse<string> TestGetProgramsForPricingModel(int jobId)
        {
            return _ConvertToBaseResponseWithStackTrace(() => _ApplicationServiceFactory.GetApplicationService<IPlanPricingService>()
                    .TestGetProgramsForPricingModel(jobId));
        }
    }
}