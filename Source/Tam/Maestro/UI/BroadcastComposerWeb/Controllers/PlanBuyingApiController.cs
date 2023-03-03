using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Buying")]
    public class PlanBuyingApiController : BroadcastControllerBase
    {
        public PlanBuyingApiController(
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base( new ControllerNameRetriever(typeof(PlanBuyingApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Queues buying job
        /// </summary>
        /// <param name="planBuyingRequestDto">
        /// ProprietaryInventory is a list of proprietary summary ids
        /// </param>
        [HttpPost]
        [Route("queue")]
        public async Task<BaseResponse<PlanBuyingJob>> Queue(PlanBuyingParametersDto planBuyingRequestDto)
        {
            var result = (await _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>()
                    .QueueBuyingJobAsync(planBuyingRequestDto, DateTime.Now, _GetCurrentUserFullName()));

            return _ConvertToBaseResponse(() => result);
        }

        [HttpPost]
        [Route("execution")]
        public BaseResponse<CurrentBuyingExecution> GetCurrentBuyingExecution(int planId, PostingTypeEnum postingType = PostingTypeEnum.NSI)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetCurrentBuyingExecution(planId,postingType));
        }

        [HttpPost]
        [Route("~/api/v2/Buying/execution")]
        public BaseResponse<CurrentBuyingExecutions> GetCurrentBuyingExecution_v2(int planId, PostingTypeEnum postingType = PostingTypeEnum.NSI)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetCurrentBuyingExecution_v2(planId, postingType));
        }

        /// <summary>
        /// Get programs data from the lastest pricing execution
        /// </summary>
        /// <param name="planId">Plan ID</param>
        /// <param name="postingType">The Posting Type</param>
        /// <param name="spotAllocationModelMode">The Spot Allocation Model Mode</param>
        /// <returns>Programs from the lastest pricing execution</returns>
        [HttpGet]
        [Route("programs/{planId}")]
        public BaseResponse<PlanBuyingResultProgramsDto> GetPrograms(int planId, PostingTypeEnum? postingType = null ,SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetPrograms(planId, postingType,spotAllocationModelMode));
        }
        /// <summary>
        /// Get programs data from the lastest pricing execution
        /// </summary>
        /// <param name="planId">Plan ID</param>
        /// <param name="postingType">The Posting Type</param>
        /// <param name="spotAllocationModelMode">The Spot Allocation Model Mode</param>
        /// <param name="planBuyingFilter">Filter parameter to filter the program data</param>
        /// <returns>Programs from the lastest pricing execution</returns>
        [HttpPost]
        [Route("~/api/v2/Buying/Programs")]
        public BaseResponse<PlanBuyingResultProgramsDto> GetPrograms(int planId, PlanBuyingFilterDto planBuyingFilter, PostingTypeEnum? postingType = null, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetPrograms(planId, postingType, spotAllocationModelMode, planBuyingFilter));
        }

        [HttpGet]
        [Route("bands/{planId}")]
        public BaseResponse<PlanBuyingBandsDto> GetBuyingBands(int planId, PostingTypeEnum? postingType = null, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingBands(planId, postingType ,spotAllocationModelMode));
        }

        [HttpPost]
        [Route("~/api/v2/Buying/bands")]
        public BaseResponse<PlanBuyingBandsDto> GetBuyingBands(int planId, PlanBuyingFilterDto planBuyingFilter, PostingTypeEnum? postingType = null, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingBands(planId, postingType, spotAllocationModelMode, planBuyingFilter));
        }

        [HttpGet]
        [Route("markets/{planId}")]
        public BaseResponse<PlanBuyingResultMarketsDto> GetBuyingMarkets(int planId, PostingTypeEnum? postingType = null, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetMarkets(planId, postingType, spotAllocationModelMode));
        }

        [HttpPost]
        [Route("~/api/v2/Buying/markets")]
        public BaseResponse<PlanBuyingResultMarketsDto> GetBuyingMarkets(int planId, PlanBuyingFilterDto planBuyingFilter, PostingTypeEnum? postingType = null, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetMarkets(planId, postingType, spotAllocationModelMode, planBuyingFilter));
        }

        [HttpGet]
        [Route("ownershipgroups/{planId}")]
        public BaseResponse<PlanBuyingResultOwnershipGroupDto> GetBuyingOwnershipGroups(int planId, PostingTypeEnum? postingType = null,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingOwnershipGroups(planId, postingType, spotAllocationModelMode));
        }

        [HttpPost]
        [Route("~/api/v2/Buying/ownershipgroups")]
        public BaseResponse<PlanBuyingResultOwnershipGroupDto> GetBuyingOwnershipGroups(int planId, PlanBuyingFilterDto planBuyingFilter, PostingTypeEnum? postingType = null,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingOwnershipGroups(planId, postingType, spotAllocationModelMode, planBuyingFilter));
        }

        [HttpGet]
        [Route("repfirms/{planId}")]
        public BaseResponse<PlanBuyingResultRepFirmDto> GetBuyingRepFirms(int planId, PostingTypeEnum? postingType = null,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingRepFirms(planId, postingType, spotAllocationModelMode));
        }

        [HttpPost]
        [Route("~/api/v2/Buying/repfirms")]
        public BaseResponse<PlanBuyingResultRepFirmDto> GetBuyingRepFirms(int planId, PlanBuyingFilterDto planBuyingFilter, PostingTypeEnum? postingType = null,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingRepFirms(planId, postingType, spotAllocationModelMode, planBuyingFilter));
        }

        [HttpPost]
        [Route("cancelexecution")]
        public BaseResponse<PlanBuyingResponseDto> CancelCurrentBuyingExecution(int planId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().CancelCurrentBuyingExecution(planId));
        }

        /// <summary>
        /// Allows checking that correct inventory is used for pricing
        /// </summary>
        /// <param name="planId">Plan ID</param>
        /// <param name="requestParameters">Parameters</param>
        [HttpPost]
        [Route("requestprograms3")]
        public BaseResponse<PlanBuyingApiRequestDto_v3> GetBuyingProgramApiRequest_v3(int planId, BuyingInventoryGetRequestParametersDto requestParameters)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingApiRequestPrograms_v3(planId, requestParameters));
        }

        [HttpPost]
        [Route("inventory")]
        public BaseResponse<List<PlanBuyingInventoryProgram>> GetBuyingInventory(int planId, BuyingInventoryGetRequestParametersDto requestParameters)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingInventory(planId, requestParameters));
        }

        /// <summary>
        /// Gets the unit caps.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("unitcaps")]
        public BaseResponse<List<LookupDto>> GetUnitCaps()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetUnitCaps());
        }

        /// <summary>
        /// Gets the pricing market groups.
        /// </summary>
        /// <returns>List of LookupDto objects</returns>
        [HttpGet]
        [Route("marketgroups")]
        public BaseResponse<List<LookupDto>> GetBuyingMarketGroups()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingMarketGroups());
        }

        [HttpGet]
        [Route("defaults")]
        public BaseResponse<PlanBuyingDefaults> GetPlanBuyingDefaults()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetPlanBuyingDefaults());
        }

        [HttpGet]
        [Route("stations/{planId}")]
        public BaseResponse<PlanBuyingStationResultDto> GetStations(int planId, PostingTypeEnum? postingType = null, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetStations(planId, postingType, spotAllocationModelMode));
        }

        [HttpPost]
        [Route("~/api/v2/Buying/stations")]
        public BaseResponse<PlanBuyingStationResultDto> GetStations(int planId, PlanBuyingFilterDto planBuyingFilter, PostingTypeEnum? postingType = null, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetStations(planId, postingType, spotAllocationModelMode, planBuyingFilter));
        }

        [HttpPost]
        [Route("ExportBuyingScx")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<Guid> ExportBuyingScx(PlanBuyingScxExportRequest request,
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency,
            PostingTypeEnum postingType = PostingTypeEnum.NSI)
        {
            var username = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>()
            .ExportPlanBuyingScx(request, username, spotAllocationModelMode, postingType));
        }

        /// <summary>
        /// Generates the program lineup report.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Identifier of the generated report</returns>
        [HttpPost]
        [Route("GenerateProgramLineupReport")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<Guid> GenerateProgramLineupReport([FromBody]ProgramLineupReportRequest request)
        {
            var fullName = _GetCurrentUserFullName();
            var appDataPath = _GetAppDataPath();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>()
                .GenerateProgramLineupReport(request, fullName, DateTime.Now, appDataPath));
        }

        /// <summary>
        /// Gets the plan buying parameters. 
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="postingType">The type of posting</param>
        /// <returns>The PlanBuyingParametersDto object</returns>
        [HttpGet]
        [Route("plangoals/{planId}")]
        public BaseResponse<PlanBuyingParametersDto> GetPlanBuyingGoals(int planId, PostingTypeEnum postingType)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetPlanBuyingGoals(planId, postingType));
        }
        /// <summary>
        /// Gets the Result Rep Firms. 
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="postingType">The posting Type</param>
        /// <param name="spotAllocationModelMode">The Spot Allocation Model Mode</param>       
        /// <returns>The list of Rep firms from buying result</returns>
        [HttpGet]
        [Route("result-rep-firms")]
        public BaseResponse<List<string>> GetResultRepFirms(int planId, PostingTypeEnum? postingType = null, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetResultRepFirms(planId,postingType,spotAllocationModelMode));
        }

        /// <summary>
        /// Gets the Result Ownership Groups. 
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="postingType">The posting Type</param>
        /// <param name="spotAllocationModelMode">The Spot Allocation Model Mode</param>       
        /// <returns>The list of Ownership Groups from buying result</returns>
        [HttpGet]
        [Route("result-ownership-groups")]
        public BaseResponse<List<string>> GetResultOwnershipGroups(int planId, PostingTypeEnum? postingType = null, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Efficiency)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetResultOwnershipGroups(planId, postingType, spotAllocationModelMode));
        }

        /// <summary>
        /// Gets the Guid For Plan Buying. 
        /// </summary>
        /// <param name="planBuyingResultsReportRequest"></param>
        /// <returns>The Guid</returns>
        [HttpPost]
        [Route("ResultsReport")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<Guid> ResultsReport([FromBody] PlanBuyingResultsReportRequest planBuyingResultsReportRequest)
        {
            var appDataPath = _GetAppDataPath();
            var createdBy = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GenerateBuyingResultsReportAndSave(planBuyingResultsReportRequest, appDataPath, createdBy));
        }

        /// <summary>
        /// Tests the repository query for getting the Inventory Programs.
        /// Query dimensions are configured per the given Job Id.
        /// </summary>
        [HttpPost]
        [Route("TestGetProgramsForPricingModel")]
        public BaseResponse<string> TestGetProgramsForBuyingModel(int jobId)
        {
            return _ConvertToBaseResponseWithStackTrace(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>()
                    .TestGetProgramsForBuyingModel(jobId));
        }
    }
}
