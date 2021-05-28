﻿using Services.Broadcast.ApplicationServices;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using System;
using System.Collections.Generic;
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
        public BaseResponse<PlanBuyingJob> Queue(PlanBuyingParametersDto planBuyingRequestDto)
        {
            return _ConvertToBaseResponse(() =>
                _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>()
                    .QueueBuyingJob(planBuyingRequestDto, DateTime.Now, _GetCurrentUserFullName()));
        }

        [HttpPost]
        [Route("execution")]
        public BaseResponse<CurrentBuyingExecution> GetCurrentBuyingExecution(int planId, PostingTypeEnum postingType = PostingTypeEnum.NSI)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetCurrentBuyingExecution(planId,postingType));
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
        public BaseResponse<PlanBuyingResultProgramsDto> GetPrograms(int planId, PostingTypeEnum? postingType = null ,SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetPrograms(planId, postingType,spotAllocationModelMode));
        }

        [HttpGet]
        [Route("bands/{planId}")]
        public BaseResponse<PlanBuyingBandsDto> GetBuyingBands(int planId, PostingTypeEnum? postingType = null, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingBands(planId, postingType ,spotAllocationModelMode));
        }

        [HttpGet]
        [Route("markets/{planId}")]
        public BaseResponse<PlanBuyingResultMarketsDto> GetBuyingMarkets(int planId, PostingTypeEnum? postingType = null, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetMarkets(planId,postingType ,spotAllocationModelMode));
        }

        [HttpGet]
        [Route("ownershipgroups/{planId}")]
        public BaseResponse<PlanBuyingResultOwnershipGroupDto> GetBuyingOwnershipGroups(int planId, PostingTypeEnum? postingType = null)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingOwnershipGroups(planId,postingType));
        }

        [HttpGet]
        [Route("repfirms/{planId}")]
        public BaseResponse<PlanBuyingResultRepFirmDto> GetBuyingRepFirms(int planId, PostingTypeEnum? postingType = null)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingRepFirms(planId,postingType));
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
        [Route("requestprograms")]
        public BaseResponse<PlanBuyingApiRequestDto> GetBuyingProgramApiRequest(int planId, BuyingInventoryGetRequestParametersDto requestParameters)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetBuyingApiRequestPrograms(planId, requestParameters));
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
        public BaseResponse<PlanBuyingStationResultDto> GetStations(int planId, PostingTypeEnum? postingType = null, SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IPlanBuyingService>().GetStations(planId, postingType ,spotAllocationModelMode));
        }

        [HttpPost]
        [Route("ExportBuyingScx")]
        public BaseResponse<Guid> ExportBuyingScx(PlanBuyingScxExportRequest request, 
            SpotAllocationModelMode spotAllocationModelMode = SpotAllocationModelMode.Quality,
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
    }
}
