using System.Data.Entity;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services;
using ConfigurationService.Client;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.Repositories
{
    public interface IProposalBuyRepository : IDataRepository
    {
        void DeleteProposalBuyByProposalDetail(int proposalVersionDetailId);
        int SaveProposalBuy(ProposalBuyFile proposalBuy, string username, DateTime timestamp);

        /// <summary>
        /// Returns a list of ProposalBuyFiles filtered by proposal details ids
        /// </summary>
        /// <param name="proposalDetailIds">List of proposal details ids</param>
        /// <returns>List of ProposalBuyFile</returns>
        List<ProposalBuyFile> GetProposalBuyFilesForProposalDetails(IEnumerable<int> proposalDetailIds);
    }
    public class ProposalBuyRepository : BroadcastRepositoryBase, IProposalBuyRepository
    {
        public ProposalBuyRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }

        public void DeleteProposalBuyByProposalDetail(int proposalVersionDetailId)
        {
            _InReadUncommitedTransaction(context =>
            {
                var proposalBuy = context.proposal_buy_files.SingleOrDefault(b => b.proposal_version_detail_id == proposalVersionDetailId);

                if(proposalBuy != null)
                {
                    context.proposal_buy_files.Remove(proposalBuy);
                    context.SaveChanges();
                }
                
            });
        }

        public List<ProposalBuyFile> GetProposalBuyFilesForProposalDetails(IEnumerable<int> proposalDetailIds)
        {
            return _InReadUncommitedTransaction(context =>
                context.proposal_buy_files
                    .Include(x => x.proposal_buy_file_details)
                    .Include(x => x.proposal_buy_file_details.Select(d => d.station))
                    .Include(x => x.proposal_buy_file_details.Select(d => d.station.market))
                    .Include(x => x.proposal_buy_file_details.Select(d => d.proposal_buy_file_detail_weeks))
                    .Include(x => x.proposal_buy_file_details.Select(d => d.proposal_buy_file_detail_weeks.Select(w => w.media_weeks)))
                    .Where(x => proposalDetailIds.Contains(x.proposal_version_detail_id))
                    .ToList()
                    .Select(MapToModel))
                    .ToList();
        }

        // Not all the fields were mapped
        private ProposalBuyFile MapToModel(proposal_buy_files file)
        {
            return new ProposalBuyFile
            {
                EstimateId = file.estimate_id ?? default(int),
                ProposalVersionDetailId = file.proposal_version_detail_id,
                StartDate = file.start_date,
                EndDate = file.end_date,
                Details = file.proposal_buy_file_details.Select(d => new ProposalBuyFile.ProposalBuyFileDetail
                {
                    Station = new DisplayBroadcastStation
                    {
                        Code = d.station.station_code.Value,
                        CallLetters = d.station.station_call_letters,
                        LegacyCallLetters = d.station.legacy_call_letters,
                        OriginMarket = d.station.market.geography_name,
                        MarketCode = d.station.market_code,
                        Affiliation = d.station.affiliation
                    },
                    Weeks = d.proposal_buy_file_detail_weeks.Select(w => new ProposalBuyFile.ProposalBuyFileDetailWeek
                    {
                        Spots = w.spots,
                        MediaWeek = new Tam.Maestro.Services.ContractInterfaces.Common.DisplayMediaWeek
                        {
                            Id = w.media_week_id,
                            WeekStartDate = w.media_weeks.start_date,
                            WeekEndDate = w.media_weeks.end_date
                        }
                    }).ToList()
                }).ToList()
            };
        }

        public int SaveProposalBuy(ProposalBuyFile proposalBuyFile, string username, DateTime timestamp)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var dbProposalBuyFile = new proposal_buy_files
                {
                    file_name = proposalBuyFile.FileName,
                    file_hash = proposalBuyFile.FileHash,
                    estimate_id = proposalBuyFile.EstimateId,
                    proposal_version_detail_id = proposalBuyFile.ProposalVersionDetailId,
                    start_date = proposalBuyFile.StartDate,
                    end_date = proposalBuyFile.EndDate,
                    created_by = username,
                    created_date = timestamp
                };

                foreach (var buyDetail in proposalBuyFile.Details)
                {
                    var dbProposalBuyDetail = new proposal_buy_file_details
                    {
                        station_id = buyDetail.Station.Id,
                        spot_cost = buyDetail.SpotCost,
                        total_spots = buyDetail.TotalSpots,
                        total_cost = buyDetail.TotalCost,
                        spot_length_id = buyDetail.SpotLengthId,
                        daypart_id = buyDetail.Daypart.Id
                        
                    };
                    foreach(var week in buyDetail.Weeks)
                    {
                        var dbBuyWeek = new proposal_buy_file_detail_weeks
                        {
                            media_week_id = week.MediaWeek.Id,
                            spots = week.Spots
                        };
                        dbProposalBuyDetail.proposal_buy_file_detail_weeks.Add(dbBuyWeek);
                    }
                    foreach(var buyAudience in buyDetail.Audiences)
                    {
                        var dbBuyAudience = new proposal_buy_file_detail_audiences
                        {
                            audience_id = buyAudience.Audience.Id,
                            audience_rank = buyAudience.Rank,
                            audience_population = buyAudience.Population,
                            impressions = buyAudience.Impressions
                        };
                        dbProposalBuyDetail.proposal_buy_file_detail_audiences.Add(dbBuyAudience);
                    }

                    dbProposalBuyFile.proposal_buy_file_details.Add(dbProposalBuyDetail);
                }

                context.proposal_buy_files.Add(dbProposalBuyFile);
                context.SaveChanges();
                return dbProposalBuyFile.id;
            });
        }
    }
}
