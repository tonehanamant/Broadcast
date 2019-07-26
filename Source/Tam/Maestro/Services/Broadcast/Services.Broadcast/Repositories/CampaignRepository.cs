using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface ICampaignRepository : IDataRepository
    {
        List<CampaignDto> GetAllCampaigns();
        CampaignDto CreateCampaign(CampaignDto campaignDto);
    }

    public class CampaignRepository : BroadcastRepositoryBase, ICampaignRepository
    {
        public CampaignRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public CampaignDto CreateCampaign(CampaignDto campaignDto)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var campaign = _MapFromDto(campaignDto);
                   context.campaigns.Add(campaign);
                   context.SaveChanges();
                   return _MapToDto(campaign);
               });
        }

        public List<CampaignDto> GetAllCampaigns()
        {
            return _InReadUncommitedTransaction(
                context => (from c in context.campaigns
                            select c).Select(_MapToDto).ToList());
        }

        private CampaignDto _MapToDto(campaign c)
        {
            return new CampaignDto
            {
                Id = c.id,
                Name = c.name,
                AdvertiserId = c.advertiser_id,
                AgencyId = c.agency_id,
                StartDate = c.start_date,
                EndDate = c.end_date,
                Budget = c.budget,
                CreatedDate = c.created_date,
                CreatedBy = c.created_by,
                ModifiedDate = c.modified_date,
                ModifiedBy = c.modified_by
            };
        }

        private campaign _MapFromDto(CampaignDto c)
        {
            return new campaign
            {
                name = c.Name,
                advertiser_id = c.AdvertiserId,
                agency_id = c.AgencyId,
                start_date = c.StartDate,
                end_date = c.EndDate,
                budget = c.Budget,
                created_date = c.CreatedDate,
                created_by = c.CreatedBy,
                modified_date = c.ModifiedDate,
                modified_by = c.ModifiedBy
            };
        }
    }
}
