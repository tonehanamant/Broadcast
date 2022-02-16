using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Campaign;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Campaigns")]
    public class CampaignApiController : BroadcastControllerBase
    {
        public CampaignApiController(BroadcastApplicationServiceFactory applicationServiceFactory) : 
            base(new ControllerNameRetriever(typeof(CampaignApiController).Name), applicationServiceFactory)
        {
        }

        /// <summary>
        /// Endpoint for listing the campaigns in the system, temporary until the FE is merged.
        /// Will be removed after.
        /// </summary>
        /// <returns>List of campaigns</returns>
        [Obsolete("This route is obsolete. Please use POST campaigns/Filter instead.")]
        [HttpGet]
        public BaseResponse<List<CampaignListItemDto>> GetCampaigns([FromUri]CampaignFilterDto campaignFilter)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().GetCampaigns(campaignFilter, DateTime.Now));
        }

        /// <summary>
        /// Endpoint for listing the campaigns in the system.
        /// </summary>
        /// <returns>List of campaigns</returns>
        [HttpPost]
        [Route("Filter")]
        public BaseResponse<List<CampaignListItemDto>> GetCampaignsWithFilter(CampaignFilterDto campaignFilter)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().GetCampaigns(campaignFilter, DateTime.Now));
        }

        /// <summary>
        /// Gets the campaign by identifier.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns>The campaign referenced by the given id.</returns>
        [HttpGet]
        [Route("{campaignId}")]
        public BaseResponse<CampaignDto> GetCampaignById(int campaignId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().GetCampaignById(campaignId));
        }

        /// <summary>
        /// Endpoint for creating new campaigns
        /// </summary>
        /// <param name="campaign">The object with the data for the new campaign</param>
        [HttpPost]
        [Route("")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<int> CreateCampaign(SaveCampaignDto campaign)
        {
            var fullName = _GetCurrentUserFullName();
            return
                _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>()
                .SaveCampaign(campaign, fullName, DateTime.Now));
        }

        /// <summary>
        /// Gets the quarters.
        /// </summary>
        /// <returns>An object with a list of quarters and the current quarter.</returns>
        [HttpGet]
        [Route("Quarters")]
        public BaseResponse<CampaignQuartersDto> GetQuarters(PlanStatusEnum? planStatus = null)
        {
            return
               _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().GetQuarters(planStatus, DateTime.Now));
        }

        /// <summary>
        /// Gets the statuses.
        /// </summary>
        /// <returns>A list of statuses for the selected quarter.</returns>
        [HttpGet]
        [Route("Statuses")]
        public BaseResponse<List<LookupDto>> GetStatuses(int? quarter = null, int? year = null)
        {
            return
               _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().GetStatuses(quarter, year));
        }

        // navigator.sendBeacon doesn`t support GET type that`s why we have to use Post
        [HttpPost]
        [Route("{campaignId}/Lock")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<BroadcastLockResponse> LockCampaign(int campaignId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().LockCampaigns(campaignId));
        }

        [HttpPost]
        [Route("{campaignId}/Unlock")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<BroadcastReleaseLockResponse> UnlockCampaign(int campaignId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().UnlockCampaigns(campaignId));
        }

        [HttpPost]
        [Route("Aggregate")]
        [Authorize]
        public BaseResponse<string> TriggerCampaignAggregation(int campaignId)
        {
            var fullName = _GetCurrentUserFullName();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().TriggerCampaignAggregationJob(campaignId, fullName));
        }

        /// <summary>
        /// Gets the campaign defaults.
        /// </summary>
        /// <returns>Gets the default values for creating a campaign in the form of <see cref="CampaignDefaultsDto"/></returns>
        [HttpGet]
        [Route("Defaults")]
        public BaseResponse<CampaignDefaultsDto> GetCampaignDefaults()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().GetCampaignDefaults());
        }

        /// <summary>
        /// Generates the campaign report.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Campaign report identifier</returns>
        [HttpPost]
        [Route("GenerateCampaignReport")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<Guid> GenerateCampaignReport([FromBody]CampaignReportRequest request)
        {
            var fullName = _GetCurrentUserFullName();
            var appDataPath = _GetAppDataPath();
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>()
                .GenerateCampaignReport(request, fullName, appDataPath));
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
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>()
                .GenerateProgramLineupReport(request, fullName, DateTime.Now, appDataPath));
        }

        /// <summary>
        /// Gets the campaign Copy.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns>The campaign copy by the given id.</returns>
        [HttpGet]
        [Route("{campaignId}/for-copy")]
        public BaseResponse<CampaignCopyDto> GetCampaignCopy(int campaignId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().GetCampaignCopy(campaignId));
        }
        /// <summary>
        /// Endpoint for creating copy campaign
        /// </summary>
        /// <param name="campaignCopy">The object with the data for the copy campaign</param>
        [HttpPost]
        [Route("copy")]
        [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
        public BaseResponse<int> CreateCampaignCopy(SaveCampaignCopyDto campaignCopy)
        {
            var createdBy = _GetCurrentUserFullName();
            return
                _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>()
                .SaveCampaignCopy(campaignCopy, createdBy, DateTime.Now));
        }
    }
}
