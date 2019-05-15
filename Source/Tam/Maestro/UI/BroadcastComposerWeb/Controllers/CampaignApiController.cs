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
    [RoutePrefix("api/Campaigns")]
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
        /// Endpoint for creating new campaigns
        /// </summary>
        /// <param name="campaignDto">The object with the data for the new campaign</param>
        /// <returns>The created campaign object</returns>
        [HttpPost]
        [Route("")]
        public BaseResponse<CampaignDto> CreateCampaign(CampaignDto campaignDto)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<ICampaignService>().CreateCampaign(campaignDto, Identity.Name, DateTime.Now));
        }
    }
}