using Common.Services;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.StationInventory;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Common.Services.Extensions;

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
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds);

        Dictionary<int, PlanPricingInventoryProgram.ManifestDaypart.Program> GetPrimaryProgramsForManifestDayparts(IEnumerable<int> manifestDaypartIds);
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
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds)
        {
            return _InReadUncommitedTransaction(
                    context => 
                    {
                        var inventoryFileIds = (from file in context.inventory_files
                                                join ratingJob in context.inventory_file_ratings_jobs on file.id equals ratingJob.inventory_file_id
                                                join source in context.inventory_sources on file.inventory_source_id equals source.id
                                                where inventorySourceIds.Contains(source.id) &&
                                                      ratingJob.status == (int)BackgroundJobProcessingStatus.Succeeded // take only files with ratings calculated
                                                group file by file.id into fileGroup
                                                select fileGroup.Key).ToList();

                        var query = (from manifest in context.station_inventory_manifest
                                     from manifestWeek in manifest.station_inventory_manifest_weeks
                                     from manifestRate in manifest.station_inventory_manifest_rates
                                     where inventoryFileIds.Contains(manifest.file_id.Value) &&
                                           stationIds.Contains(manifest.station.id) &&
                                           manifestWeek.start_date <= endDate && manifestWeek.end_date >= startDate &&
                                           manifestRate.spot_length_id == spotLengthId
                                     group manifest by manifest.id into manifestGroup
                                     select manifestGroup.FirstOrDefault());

                        query = query
                            .Include(x => x.station_inventory_manifest_weeks)
                            .Include(x => x.station_inventory_manifest_rates)
                            .Include(x => x.station_inventory_manifest_dayparts)
                            .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.station_inventory_manifest_daypart_programs))
                            .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.station_inventory_manifest_daypart_programs.Select(p => p.genre)))
                            .Include(x => x.station_inventory_manifest_audiences)
                            .Include(x => x.station_inventory_group)
                            .Include(x => x.station)
                            .Include(x => x.inventory_sources);

                        return query.ToList().Select(x => new PlanPricingInventoryProgram
                        {
                            ManifestId = x.id,
                            ManifestWeeks = x.station_inventory_manifest_weeks
                                .Where(w => w.start_date <= endDate && w.end_date >= startDate)
                                .Select(w => new PlanPricingInventoryProgram.ManifestWeek
                                {
                                    Id = w.id,
                                    InventoryMediaWeekId = w.media_week_id,
                                    Spots = w.spots
                                })
                                .ToList(),
                            SpotCost = x.station_inventory_manifest_rates.Single(r => r.spot_length_id == spotLengthId).spot_cost,
                            Station = new DisplayBroadcastStation()
                            {
                                Id = x.station.id,
                                Affiliation = x.station.affiliation,
                                Code = x.station.station_code,
                                CallLetters = x.station.station_call_letters,
                                LegacyCallLetters = x.station.legacy_call_letters,
                                MarketCode = x.station.market_code
                            },
                            Unit = x.station_inventory_group?.name,
                            InventorySource = new InventorySource
                            {
                                Id = x.inventory_sources.id,
                                InventoryType = (InventorySourceTypeEnum)x.inventory_sources.inventory_source_type,
                                IsActive = x.inventory_sources.is_active,
                                Name = x.inventory_sources.name
                            },
                            ManifestDayparts = x.station_inventory_manifest_dayparts.Select(d =>
                            {
                                var item = new PlanPricingInventoryProgram.ManifestDaypart
                                {
                                    Id = d.id,
                                    Daypart = DaypartCache.Instance.GetDisplayDaypart(d.daypart_id),
                                    ProgramName = d.program_name,
                                    Programs = d.station_inventory_manifest_daypart_programs.Select(_MapToPlanPricingInventoryProgram).ToList()
                                };

                                if (d.primary_program_id.HasValue)
                                {
                                    var program = d.station_inventory_manifest_daypart_programs.Single(
                                        p => p.id == d.primary_program_id.Value, 
                                        $"Can not find primary program {d.primary_program_id.Value} for manifest daypart {d.id}");

                                    item.PrimaryProgram = _MapToPlanPricingInventoryProgram(program);
                                }

                                return item;
                            }).ToList(),
                            ManifestAudiences = x.station_inventory_manifest_audiences.Select(a => new PlanPricingInventoryProgram.ManifestAudience
                            {
                                AudienceId = a.audience_id,
                                Impressions = a.impressions,
                                IsReference = a.is_reference
                            }).ToList()
                        }).ToList();
                    });
        }

        public Dictionary<int, PlanPricingInventoryProgram.ManifestDaypart.Program> GetPrimaryProgramsForManifestDayparts(IEnumerable<int> manifestDaypartIds)
        {
            return _InReadUncommitedTransaction(
                    context =>
                    {
                        var manifestDayparts = context.station_inventory_manifest_dayparts
                            .Include(x => x.station_inventory_manifest_daypart_programs)
                            .Where(x => manifestDaypartIds.Contains(x.id) && x.primary_program_id != null)
                            .ToList();

                        var result = manifestDayparts.ToDictionary(
                            x => x.id,
                            x =>
                            {
                                var program = x.station_inventory_manifest_daypart_programs.Single(
                                        p => p.id == x.primary_program_id.Value,
                                        $"Can not find primary program {x.primary_program_id.Value} for manifest daypart {x.id}");

                                return _MapToPlanPricingInventoryProgram(program);
                            });

                        return result;
                    });
        }

        private PlanPricingInventoryProgram.ManifestDaypart.Program _MapToPlanPricingInventoryProgram(station_inventory_manifest_daypart_programs program)
        {
            return new PlanPricingInventoryProgram.ManifestDaypart.Program
            {
                Name = program.name,
                ShowType = program.show_type,
                Genre = program.maestro_genre.name,
                StartTime = program.start_time,
                EndTime = program.end_time
            };
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
