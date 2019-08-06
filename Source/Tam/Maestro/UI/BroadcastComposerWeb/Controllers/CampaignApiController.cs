using Common.Services.WebComponents;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/v1/Campaigns")]
    public class CampaignApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public CampaignApiController(IWebLogger logger, BroadcastApplicationServiceFactory applicationServiceFactory) : 
            base(logger, new ControllerNameRetriever(typeof(ProposalController).Name))
        {
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        /// <summary>
        /// Endpoint for listing all the campaigns in the system
        /// </summary>
        /// <returns>List of campaigns</returns>
        [HttpGet]
        [Route("")]
        public BaseResponse<List<CampaignDto>> GetAllCampaigns()
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().GetAllCampaigns());
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
        public BaseResponse<int> CreateCampaign(CampaignDto campaign)
        {
            return
                _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>()
                .SaveCampaign(campaign, Identity.Name, DateTime.Now));
        }

        /// <summary>
        /// Gets the advertisers.
        /// </summary>
        /// <returns>A list of advertiser objects.</returns>
        [HttpGet]
        [Route("Advertisers")]
        public BaseResponse<List<AdvertiserDto>> GetAdvertisers()
        {
            return
                _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().GetAdvertisers());
        }

        /// <summary>
        /// Gets the agencies.
        /// </summary>
        /// <returns>A list of agency objects.</returns>
        [HttpGet]
        [Route("Agencies")]
        public BaseResponse<List<AgencyDto>> GetAgencies()
        {
            return
               _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().GetAgencies());
        }
    }
}