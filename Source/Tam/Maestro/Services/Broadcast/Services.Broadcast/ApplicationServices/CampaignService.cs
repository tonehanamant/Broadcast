using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices
{
    public interface ICampaignService : IApplicationService
    {
        List<CampaignDto> GetAllCampaigns();
        CampaignDto CreateCampaign(CampaignDto campaignDto, string userName, DateTime createdDate);
    }

    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _CampaginRepository;
        private readonly ISMSClient _SmsClient;

        public const string InvalidDatesMessage = "The end date must be greater than the start date for the campaign";
        public const string InvalidAdvertiserMessage = "Invalid advertiser id in campaign, please inform a valid and active id";

        public CampaignService(IDataRepositoryFactory dataRepositoryFactory, ISMSClient smsClient)
        {
            _CampaginRepository = dataRepositoryFactory.GetDataRepository<ICampaignRepository>();
            _SmsClient = smsClient;
        }

        public List<CampaignDto> GetAllCampaigns()
        {
            return _CampaginRepository.GetAllCampaigns();
        }

        public CampaignDto CreateCampaign(CampaignDto campaignDto, string createdBy, DateTime createdDate)
        {
            _SetAuditFields(campaignDto, createdBy, createdDate);
            _ValidateAdvertiser(campaignDto);
            _ValidateDates(campaignDto);

            return _CampaginRepository.CreateCampaign(campaignDto);
        }

        private void _ValidateDates(CampaignDto campaignDto)
        {
            if (campaignDto.StartDate >= campaignDto.EndDate)
                throw new Exception(InvalidDatesMessage);
        }

        private void _ValidateAdvertiser(CampaignDto campaignDto)
        {
            try
            {
                _SmsClient.FindAdvertiserById(campaignDto.AdvertiserId);
            }
            catch
            {
                throw new Exception(InvalidAdvertiserMessage);
            }
        }

        private void _SetAuditFields(CampaignDto campaignDto, string createdBy, DateTime createdDate)
        {
            campaignDto.CreatedBy = createdBy;
            campaignDto.CreatedDate = createdDate;
            campaignDto.ModifiedBy = null;
            campaignDto.ModifiedDate = null;
        }
    }
}
