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

namespace Services.Broadcast.Repositories
{

    public interface IStationProgramRepository : IDataRepository
    {
        List<ProposalProgramDto> GetPrograms(DateTime flightStart, DateTime flightEnd,
            int spotLength, int rateSource, List<int> proposalMarketIds);

        List<ProposalProgramDto> GetStationPrograms(List<int> manifestIds);
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
                                    Impressions = ma.impressions
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
