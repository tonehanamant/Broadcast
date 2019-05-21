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

        public const string InvalidDatesErrorMessage = "The end date must be greater than the start date for the campaign";
        public const string InvalidAdvertiserErrorMessage = "The advertiser id is invalid, please provide a valid and active id";
        public const string InvalidCampaignNameErrorMessage = "The campaign name is invalid, please provide a valid name";

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
            _ValidateCampaignName(campaignDto);

            return _CampaginRepository.CreateCampaign(campaignDto);
        }

        private void _ValidateCampaignName(CampaignDto campaignDto)
        {
            if (string.IsNullOrWhiteSpace(campaignDto.Name))
                throw new Exception(InvalidCampaignNameErrorMessage);
        }

        private void _ValidateDates(CampaignDto campaignDto)
        {
            if (campaignDto.StartDate >= campaignDto.EndDate)
                throw new Exception(InvalidDatesErrorMessage);
        }

        private void _ValidateAdvertiser(CampaignDto campaignDto)
        {
            try
            {
                _SmsClient.FindAdvertiserById(campaignDto.AdvertiserId);
            }
            catch
            {
                throw new Exception(InvalidAdvertiserErrorMessage);
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
