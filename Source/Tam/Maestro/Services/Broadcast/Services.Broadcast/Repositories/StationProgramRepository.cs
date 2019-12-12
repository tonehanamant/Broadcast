using System.Data.Entity;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Helpers;

namespace Services.Broadcast.Repositories
{

    public interface IStationProgramRepository : IDataRepository
    {
        List<ProposalProgramDto> GetPrograms(DateTime flightStart, DateTime flightEnd,
            int spotLength, int rateSource, List<int> proposalMarketIds);

        List<ProposalProgramDto> GetStationPrograms(List<int> manifestIds);
        
        List<PlanPricingInventoryProgram> GetProgramsForPricingModel(
            DateTime startDate, 
            DateTime endDate,
            int spotLengthId,
            List<int> inventorySourceTypes,
            List<int> marketCodes);
    }

    public class StationProgramRepository : BroadcastRepositoryBase, IStationProgramRepository
    {
        public StationProgramRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public List<ProposalProgramDto> GetPrograms(DateTime flightStart, DateTime flightEnd,
            int spotLengthId, int inventorySourceId, List<int> marketIds)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        var manifests = context.station_inventory_manifest
                            .Include(a => a.station_inventory_manifest_dayparts)
                            .Include(a => a.station_inventory_manifest_dayparts.Select(d => d.station_inventory_manifest_daypart_genres))
                            .Include(b => b.station_inventory_manifest_audiences)
                            .Include(m => m.station_inventory_manifest_rates)
                            .Include(w => w.station_inventory_manifest_weeks)
                            .Include(m => m.station_inventory_spots)
                            .Include(i => i.inventory_sources)
                            .Include(s => s.station)
                            .Where(s => s.station.market_code != null)
                            .Where(p => p.inventory_source_id == inventorySourceId)
                            .Where(x => x.station_inventory_manifest_weeks.Min(y => y.start_date) <= flightEnd.Date)
                            .Where(x => x.station_inventory_manifest_weeks.Max(y => y.end_date) >= flightStart.Date)
                            .ToList();

                        if (marketIds != null && marketIds.Count > 0)
                        {
                            manifests = manifests.Where(b => marketIds.Contains(b.station.market_code.Value)).ToList();
                        }

                        return (manifests.Select(m =>
                            new ProposalProgramDto
                            {
                                ManifestId = m.id,
                                ManifestDayparts = m.station_inventory_manifest_dayparts.Select(md => new ProposalProgramDto.ManifestDaypartDto
                                {
                                    Id = md.id,
                                    DaypartId = md.daypart_id,
                                    ProgramName = md.program_name
                                }).ToList(),
                                ManifestAudiences = m.station_inventory_manifest_audiences.Select(ma => new ProposalProgramDto.ManifestAudienceDto
                                {
                                    AudienceId = ma.audience_id,
                                    Impressions = ma.impressions,
                                    IsReference = ma.is_reference
                                }).ToList(),
                                StartDate = m.station_inventory_manifest_weeks.Min(w => w.start_date),
                                EndDate = m.station_inventory_manifest_weeks.Max(w => w.end_date),
                                SpotCost = m.station_inventory_manifest_rates.Where(r => r.spot_length_id == spotLengthId).Select(r => r.spot_cost).SingleOrDefault(),
                                TotalSpots = m.spots_per_week ?? 0,
                                Station = new DisplayScheduleStation
                                {
                                    StationCode = (short)m.station.station_code.Value,
                                    LegacyCallLetters = m.station.legacy_call_letters,
                                    Affiliation = m.station.affiliation,
                                    CallLetters = m.station.station_call_letters
                                },
                                Market = new LookupDto
                                {
                                    Id = m.station.market_code.Value,
                                    Display = m.station.market.geography_name
                                },
                                Allocations = m.station_inventory_spots.Select(r => new StationInventorySpots
                                {
                                    ManifestId = r.station_inventory_manifest_id,
                                    ProposalVersionDetailQuarterWeekId = r.proposal_version_detail_quarter_week_id,
                                    MediaWeekId = r.media_week_id
                                }).ToList(),
                                Genres = m.station_inventory_manifest_dayparts
                                .SelectMany(x => x.station_inventory_manifest_daypart_genres
                                    .Select(genre => new LookupDto() { Id = genre.genre_id, Display = genre.genre.name }))
                                .Distinct().ToList()
                            }).ToList());
                    });
            }
        }

        public List<PlanPricingInventoryProgram> GetProgramsForPricingModel(
            DateTime startDate, 
            DateTime endDate, 
            int spotLengthId,
            List<int> inventorySourceTypes,
            List<int> marketCodes)
        {
            return _InReadUncommitedTransaction(
                    context => 
                    {
                        var query = (from manifest in context.station_inventory_manifest
                                     from manifestWeek in manifest.station_inventory_manifest_weeks
                                     from manifestRate in manifest.station_inventory_manifest_rates
                                     where inventorySourceTypes.Contains(manifest.inventory_sources.inventory_source_type) &&
                                           marketCodes.Contains(manifest.station.market_code.Value) &&
                                           manifestWeek.start_date <= endDate && manifestWeek.end_date >= startDate &&
                                           manifestRate.spot_length_id == spotLengthId &&
                                           manifest.inventory_files.inventory_file_ratings_jobs.FirstOrDefault().status == (int)BackgroundJobProcessingStatus.Succeeded // take only inventory with ratings calculated
                                     group manifest by manifest.id into manifestGroup
                                     select manifestGroup.FirstOrDefault());

                        query = query
                            .Include(x => x.station_inventory_manifest_weeks)
                            .Include(x => x.station_inventory_manifest_rates)
                            .Include(x => x.station_inventory_manifest_dayparts)
                            .Include(x => x.station_inventory_manifest_audiences)
                            .Include(x => x.station_inventory_group)
                            .Include(x => x.station)
                            .Include(x => x.inventory_sources);

                        return query.ToList().Select(x => new PlanPricingInventoryProgram
                        {
                            ManifestId = x.id,
                            MediaWeekIds = x.station_inventory_manifest_weeks
                                .Where(w => w.start_date <= endDate && w.end_date >= startDate)
                                .Select(w => w.media_week_id)
                                .Distinct()
                                .ToList(),
                            SpotCost = x.station_inventory_manifest_rates.Single(r => r.spot_length_id == spotLengthId).spot_cost,
                            StationLegacyCallLetters = x.station.legacy_call_letters,
                            Unit = x.station_inventory_group?.name,
                            InventorySource = x.inventory_sources.name,
                            InventorySourceType = ((InventorySourceTypeEnum)x.inventory_sources.inventory_source_type).GetDescriptionAttribute(),
                            ManifestDayparts = x.station_inventory_manifest_dayparts.Select(d => new ProposalProgramDto.ManifestDaypartDto
                            {
                                Id = d.id,
                                DaypartId = d.daypart_id,
                                ProgramName = d.program_name
                            }).ToList(),
                            ManifestAudiences = x.station_inventory_manifest_audiences.Select(a => new ProposalProgramDto.ManifestAudienceDto
                            {
                                AudienceId = a.audience_id,
                                Impressions = a.impressions,
                                IsReference = a.is_reference
                            }).ToList()
                        }).ToList();
                    });
        }

        public List<ProposalProgramDto> GetStationPrograms(List<int> manifestIds)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        var manifests = context.station_inventory_manifest
                            .Include(a => a.station_inventory_manifest_dayparts)
                            .Include(a => a.station_inventory_manifest_dayparts.Select(d => d.station_inventory_manifest_daypart_genres))
                            .Include(b => b.station_inventory_manifest_audiences)
                            .Include(m => m.station_inventory_manifest_rates)
                            .Include(m => m.station_inventory_spots)
                            .Include(m => m.station_inventory_manifest_weeks)
                            .Include(s => s.station)
                            .Include(i => i.inventory_sources)
                            .Where(p => manifestIds.Contains(p.id))
                            .ToList();

                        return (manifests.Select(m =>
                            new ProposalProgramDto()
                            {
                                ManifestId = m.id,
                                ManifestDayparts = m.station_inventory_manifest_dayparts.Select(md => new ProposalProgramDto.ManifestDaypartDto
                                {
                                    Id = md.id,
                                    DaypartId = md.daypart_id,
                                    ProgramName = md.program_name
                                }).ToList(),
                                ManifestAudiences = m.station_inventory_manifest_audiences.Select(ma => new ProposalProgramDto.ManifestAudienceDto
                                {
                                    AudienceId = ma.audience_id,
                                    Impressions = ma.impressions
                                }).ToList(),
                                StartDate = m.station_inventory_manifest_weeks.Min(w => w.start_date),
                                EndDate = m.station_inventory_manifest_weeks.Max(w => w.end_date),
                                TotalSpots = m.spots_per_week ?? 0,
                                Station = new DisplayScheduleStation
                                {
                                    StationCode = (short)m.station.station_code.Value,
                                    LegacyCallLetters = m.station.legacy_call_letters,
                                    Affiliation = m.station.affiliation,
                                    CallLetters = m.station.station_call_letters
                                },
                                Market = new LookupDto
                                {
                                    Id = m.station.market_code.Value,
                                    Display = m.station.market.geography_name
                                },
                                Allocations = m.station_inventory_spots.Select(r => new StationInventorySpots
                                {
                                    ManifestId = r.station_inventory_manifest_id,
                                    ProposalVersionDetailQuarterWeekId = r.proposal_version_detail_quarter_week_id,
                                    MediaWeekId = r.media_week_id
                                }).ToList(),
                                Genres = m.station_inventory_manifest_dayparts
                                .SelectMany(x => x.station_inventory_manifest_daypart_genres
                                    .Select(genre => new LookupDto() { Id = genre.genre_id, Display = genre.genre.name }))
                                .Distinct().ToList()
                            }).ToList());
                    });
            }
        }
    }
}
