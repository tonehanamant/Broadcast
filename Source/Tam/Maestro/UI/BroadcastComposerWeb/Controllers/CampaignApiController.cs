using Common.Services.ApplicationServices;
using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Campaigns")]
    public class CampaignApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public CampaignApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) : 
            base(logger, new ControllerNameRetriever(typeof(CampaignApiController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        /// <summary>
        /// Endpoint for listing the campaigns in the system, temporary until the FE is merged.
        /// Will be removed after.
        /// </summary>
        /// <returns>List of campaigns</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<CampaignListItemDto>> GetCampaigns(CampaignFilterDto campaignFilter)
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
        [Authorize]
        public BaseResponse<int> CreateCampaign(CampaignDto campaign)
        {
            return
                _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>()
                .SaveCampaign(campaign, Identity.Name, DateTime.Now));
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
        public BaseResponse<List<LookupDto>> GetStatuses(QuarterDto quarter)
        {
            return
               _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().GetStatuses(quarter));
        }

        [HttpGet]
        [Route("{campaignId}/Lock")]
        public BaseResponse<LockResponse> LockCampaign(int campaignId)
        {
            var key = KeyHelper.GetCampaignLockingKey(campaignId);
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ILockingManagerApplicationService>().LockObject(key));
        }

        [HttpGet]
        [Route("{campaignId}/Unlock")]
        public BaseResponse<ReleaseLockResponse> UnlockCampaign(int campaignId)
        {
            var key = KeyHelper.GetCampaignLockingKey(campaignId);
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ILockingManagerApplicationService>().ReleaseObject(key));
        }

        [HttpPost]
        [Route("Aggregate")]
        [Authorize]
        public BaseResponse<string> TriggerCampaignAggregation(int campaignId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().TriggerCampaignAggregationJob(campaignId, Identity.Name));
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
    }
}
