using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    /// <summary>
    /// Data operations for the Campaign Repository.
    /// </summary>
    /// <seealso cref="Common.Services.Repositories.IDataRepository" />
    public interface ICampaignRepository : IDataRepository
    {
        /// <summary>
        /// Gets all campaigns.
        /// </summary>
        /// <returns></returns>
        List<CampaignDto> GetCampaigns(QuarterDetailDto quarter);

        /// <summary>
        /// Gets the campaign.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        CampaignDto GetCampaign(int id);

        /// <summary>
        /// Creates the campaign.
        /// </summary>
        /// <param name="campaignDto">The campaign dto.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns>Id of the new campaign</returns>
        int CreateCampaign(CampaignDto campaignDto, string createdBy, DateTime createdDate);

        /// <summary>
        /// Updates the campaign.
        /// </summary>
        /// <param name="campaignDto">The campaign dto.</param>
        /// <returns>Id of the campain updated</returns>
        int UpdateCampaign(CampaignDto campaignDto);

        /// <summary>
        /// Gets the list of all date ranges for campaign's plans
        /// </summary>
        /// <returns>A list of date ranges.</returns>
        List<DateRange> GetCampaignsDateRanges();
    }

    /// <summary>
    /// Data operations for the Campaign Repository.
    /// </summary>
    /// <seealso cref="BroadcastRepositoryBase" />
    /// <seealso cref="ICampaignRepository" />
    public class CampaignRepository : BroadcastRepositoryBase, ICampaignRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignRepository"/> class.
        /// </summary>
        /// <param name="pSmsClient">The p SMS client.</param>
        /// <param name="pBroadcastContextFactory">The p broadcast context factory.</param>
        /// <param name="pTransactionHelper">The p transaction helper.</param>
        /// <param name="pConfigurationWebApiClient">The p configuration web API client.</param>
        public CampaignRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        /// <inheritdoc />
        public int CreateCampaign(CampaignDto campaignDto, string createdBy, DateTime createdDate)
        {
            var campaign = new campaign
            {
                name = campaignDto.Name,
                advertiser_id = campaignDto.AdvertiserId,
                agency_id = campaignDto.AgencyId,
                notes = campaignDto.Notes,
                created_by = createdBy,
                created_date = createdDate,
                modified_date = campaignDto.ModifiedDate,
                modified_by = campaignDto.ModifiedBy
            };

            return _InReadUncommitedTransaction(
               context =>
               {
                   context.campaigns.Add(campaign);
                   context.SaveChanges();
                   return campaign.id;
               });
        }

        /// <inheritdoc />
        public int UpdateCampaign(CampaignDto campaignDto)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   var existingCampaign = context.campaigns.Single(x => x.id == campaignDto.Id, "Invalid campaign id");

                   existingCampaign.name = campaignDto.Name;
                   existingCampaign.advertiser_id = campaignDto.AdvertiserId;
                   existingCampaign.agency_id = campaignDto.AgencyId;
                   existingCampaign.notes = campaignDto.Notes;
                   existingCampaign.modified_by = campaignDto.ModifiedBy;
                   existingCampaign.modified_date = campaignDto.ModifiedDate;
                   
                   context.SaveChanges();

                   return existingCampaign.id;
               });
        }

        /// <inheritdoc />
        public List<CampaignDto> GetCampaigns(QuarterDetailDto quarter)
        {
            return _InReadUncommitedTransaction(
                context => (from c in context.campaigns
                            from p in c.plans.DefaultIfEmpty()
                            // If there are no plans, we filter by campaign created date.
                            where (p == null && 
                                   c.created_date >= quarter.StartDate && 
                                   c.created_date <= quarter.EndDate) ||
                                   // Has a plan, but without start date.
                                   (p != null &&
                                   p.flight_start_date == null &&
                                   c.created_date >= quarter.StartDate &&
                                   c.created_date <= quarter.EndDate) ||
                                   // Plan only has a start date.                                    
                                   (p != null && 
                                    p.flight_start_date != null &&
                                    p.flight_end_date == null &&
                                    p.flight_start_date >= quarter.StartDate &&
                                    p.flight_start_date <= quarter.EndDate) ||
                                   // Plan has start and end date, intersect the dates.
                                   (p != null &&
                                    p.flight_start_date != null &&
                                    p.flight_end_date != null &&
                                    p.flight_start_date <= quarter.EndDate &&
                                    p.flight_end_date >= quarter.StartDate)
                            orderby c.modified_date
                            group c by c.id into campaign
                            select campaign.FirstOrDefault()).Select(_MapToDto).ToList());
        }

        /// <inheritdoc />
        public CampaignDto GetCampaign(int campaignId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var found = context.campaigns.Single(c => c.id.Equals(campaignId), $"Could not find existing campaign with id '{campaignId}'");

                    var result = _MapToDto(found);

                    result.HasPlans = found.plans.Any();

                    return result;
                });
        }

        /// <inheritdoc />
        public List<DateRange> GetCampaignsDateRanges()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var campaignsWithValidPlans = (from p in context.plans
                                                   where p.flight_start_date != null
                                                   select p.campaign_id).Distinct().ToList();

                    var campaignsDates = (from c in context.campaigns
                                          where !campaignsWithValidPlans.Contains(c.id)
                                          select c.created_date).ToList();

                    var plansDateRanges = (from c in context.campaigns
                                           from p in c.plans
                                           where campaignsWithValidPlans.Contains(c.id)
                                           select new { p.flight_start_date, p.flight_end_date }).ToList();

                    return campaignsDates
                            .Select(c => new DateRange(c, null))
                            .Concat(plansDateRanges.Select(c => new DateRange(c.flight_start_date, c.flight_end_date))).ToList();
                 });
        }

        private CampaignDto _MapToDto(campaign c)
        {
            return new CampaignDto
            {
                Id = c.id,
                Name = c.name,
                AdvertiserId = c.advertiser_id,
                AgencyId = c.agency_id,
                Notes = c.notes,
                ModifiedDate = c.modified_date,
                ModifiedBy = c.modified_by
            };
        }
    }
}
