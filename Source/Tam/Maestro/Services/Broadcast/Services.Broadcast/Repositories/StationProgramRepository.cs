using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan.Buying;
using Services.Broadcast.Entities.Plan.CommonPricingEntities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Entities.QuoteReport;
using Services.Broadcast.Entities.StationInventory;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.ContractInterfaces.Common;

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
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds);

        /// <summary>
        /// Gets the programs for the pricing model.  
        /// V2 uses a different query than GetProgramsForPricingModel (V1) 
        /// in the hopes to improve and avoid resource hogging issues.
        /// </summary>
        List<PlanPricingInventoryProgram> GetProgramsForPricingModelv2(
            DateTime startDate,
            DateTime endDate,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds);

        Dictionary<int, PlanPricingInventoryProgram.ManifestDaypart.Program> GetPrimaryProgramsForManifestDayparts(IEnumerable<int> manifestDaypartIds);

        List<QuoteProgram> GetProgramsForQuoteReport(
            DateTime startDate,
            DateTime endDate,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds);

        List<PlanBuyingInventoryProgram> GetProgramsForBuyingModel(
            DateTime startDate,
            DateTime endDate,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds);

        /// <summary>
        /// Gets the programs for the pricing model.  
        /// V2 uses a different query than GetProgramsForBuyingModelv2 (V1) 
        /// in the hopes to improve and avoid resource hogging issues.
        /// </summary>
        List<PlanBuyingInventoryProgram> GetProgramsForBuyingModelv2(
            DateTime startDate,
            DateTime endDate,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds);
    }

    public class StationProgramRepository : BroadcastRepositoryBase, IStationProgramRepository
    {
        public StationProgramRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

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

        public List<QuoteProgram> GetProgramsForQuoteReport(
            DateTime startDate,
            DateTime endDate,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds)
        {
            return _InReadUncommitedTransaction(context =>
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
                                   spotLengthIds.Contains(manifestRate.spot_length_id) &&
                                   manifest.station_inventory_manifest_dayparts.Any(m => m.primary_program_id != null)
                             group manifest by manifest.id into manifestGroup
                             select manifestGroup.FirstOrDefault());

                query = query
                    .Include(x => x.station_inventory_manifest_rates)
                    .Include(x => x.station_inventory_manifest_dayparts)
                    .Include(x => x.station_inventory_manifest_dayparts.Select(d => d.station_inventory_manifest_daypart_programs))
                    .Include(x => x.station_inventory_manifest_audiences)
                    .Include(x => x.station);

                return query
                    .Select(x => new QuoteProgram
                    {
                        ManifestId = x.id,
                        ManifestRates = x.station_inventory_manifest_rates
                            .Where(r => spotLengthIds.Contains(r.spot_length_id))
                            .Select(r => new QuoteProgram.ManifestRate
                            {
                                SpotLengthId = r.spot_length_id,
                                Cost = r.spot_cost
                            })
                            .ToList(),
                        Station = new DisplayBroadcastStation()
                        {
                            Id = x.station.id,
                            Affiliation = x.station.affiliation,
                            Code = x.station.station_code,
                            CallLetters = x.station.station_call_letters,
                            LegacyCallLetters = x.station.legacy_call_letters,
                            MarketCode = x.station.market_code
                        },
                        ManifestDayparts = x.station_inventory_manifest_dayparts
                            .Where(d => d.primary_program_id.HasValue)
                            .Select(d => new QuoteProgram.ManifestDaypart
                            {
                                Id = d.id,
                                Daypart = new DisplayDaypart
                                {
                                    Id = d.daypart_id
                                },
                                PrimaryProgramId = d.primary_program_id.Value,
                                Programs = d.station_inventory_manifest_daypart_programs.Select(z => new QuoteProgram.ManifestDaypart.Program
                                    {
                                        Id = z.id,
                                        Name = z.name
                                    }).ToList()
                            }).ToList(),
                        ManifestAudiences = x.station_inventory_manifest_audiences
                            .Where(a => a.is_reference)
                            .Select(a => new QuoteProgram.ManifestAudience
                            {
                                AudienceId = a.audience_id,
                                Impressions = a.impressions
                            }).ToList()
                    })
                    .ToList();
            });
        }

        public List<PlanPricingInventoryProgram> GetProgramsForPricingModel(
            DateTime startDate,
            DateTime endDate,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds)
        {
            return _InReadUncommitedTransaction(context =>
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
                                   spotLengthIds.Contains(manifestRate.spot_length_id) &&
                                   manifest.station_inventory_manifest_dayparts.Any(m => m.primary_program_id != null)
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

                return query
                    .Select(x => new PlanPricingInventoryProgram
                    {
                        ManifestId = x.id,
                        SpotLengthId = x.spot_length_id,
                        ManifestWeeks = x.station_inventory_manifest_weeks
                            .Where(w => w.start_date <= endDate && w.end_date >= startDate)
                            .Select(w => new PlanPricingInventoryProgram.ManifestWeek
                            {
                                Id = w.id,
                                InventoryMediaWeekId = w.media_week_id,
                                Spots = w.spots
                            })
                            .ToList(),
                        ManifestRates = x.station_inventory_manifest_rates
                            .Where(r => spotLengthIds.Contains(r.spot_length_id))
                            .Select(r => new PlanPricingInventoryProgram.ManifestRate
                            {
                                SpotLengthId = r.spot_length_id,
                                Cost = r.spot_cost
                            })
                            .ToList(),
                        Station = new DisplayBroadcastStation()
                        {
                            Id = x.station.id,
                            Affiliation = x.station.affiliation,
                            Code = x.station.station_code,
                            CallLetters = x.station.station_call_letters,
                            LegacyCallLetters = x.station.legacy_call_letters,
                            MarketCode = x.station.market_code,
                            IsTrueInd = x.station.is_true_ind
                        },
                        InventorySource = new InventorySource
                        {
                            Id = x.inventory_sources.id,
                            InventoryType = (InventorySourceTypeEnum)x.inventory_sources.inventory_source_type,
                            IsActive = x.inventory_sources.is_active,
                            Name = x.inventory_sources.name
                        },
                        ManifestDayparts = x.station_inventory_manifest_dayparts
                            .Where(d => d.primary_program_id.HasValue)
                            .Select(d => new PlanPricingInventoryProgram.ManifestDaypart
                            {
                                Id = d.id,
                                Daypart = new DisplayDaypart
                                {
                                    Id = d.daypart_id
                                },
                                ProgramName = d.program_name,
                                PrimaryProgramId = d.primary_program_id,
                                Programs = d.station_inventory_manifest_daypart_programs.Select(
                                    z => new PlanPricingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = z.id,
                                        Name = z.name,
                                        ShowType = z.show_type,
                                        Genre = z.maestro_genre.name,
                                        StartTime = z.start_time,
                                        EndTime = z.end_time
                                    }).ToList()
                            }).ToList(),
                        ManifestAudiences = x.station_inventory_manifest_audiences
                            .Where(a => a.is_reference)
                            .Select(a => new PlanPricingInventoryProgram.ManifestAudience
                            {
                                AudienceId = a.audience_id,
                                Impressions = a.impressions
                            })
                            .ToList()
                    })
                    .ToList();
            });
        }

        /// <inheritdoc/>
        public List<PlanPricingInventoryProgram> GetProgramsForPricingModelv2(
            DateTime startDate,
            DateTime endDate,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                /*
                 * In some plans the size of the data used up the resources on the server and this blew up.
                 * We have split up the query to bring down the memory footprint.
                 * 
                 * 1. Query to get just the list of manifests to get.
                 * 2. Gather the details in chunks.
                 * 3. Transform at query-time to not bring back all that unused data.
                 * */

                // use the media weeks ids for a more direct and faster query.
                var startMediaWeekId = context.media_weeks
                    .Single(w => w.start_date <= startDate && w.end_date >= startDate).id;

                var endMediaWeekId = context.media_weeks
                    .Single(w => w.start_date <= endDate.Date && w.end_date >= endDate.Date).id;

                var manifestIds = context.station_inventory_manifest
                    .Where(s =>
                        inventorySourceIds.Contains(s.inventory_source_id)
                        // has weeks within this time frame. 
                        // that it has weeks at all guarantees that it is "active"
                        && s.station_inventory_manifest_weeks.Any(w => w.media_week_id >= startMediaWeekId && w.media_week_id <= endMediaWeekId)
                        // within our list of target stations
                        && s.station_id.HasValue && stationIds.Contains(s.station_id.Value)
                        // has a program
                        && s.station_inventory_manifest_dayparts.Any(d => d.primary_program_id != null)
                        // has rates
                        && spotLengthIds.Intersect(s.station_inventory_manifest_rates.Select(r => r.spot_length_id)).Any()
                    )
                    .Select(s => s.id)
                    .Distinct()
                    .ToList();

                // Chunk up what we found to minimize the memory of each query.
                // 10,000 hits the resource issue.
                //  7,500 clears it and plays nicer with the neighbors.
                const int chunkSize = 7500;
                var idChunks = manifestIds.GetChunks(chunkSize);

                var foundManifests = new List<PlanPricingInventoryProgram>();

                foreach (var chunk in idChunks)
                {
                    var queryResult = context.station_inventory_manifest
                    // .Includes(...) slowed this down, so don't use them.
                    // They are not needed since we transform in-line with the query.
                    .Where(s => chunk.Contains(s.id))
                    // Transform in-line here to not bring back all that unused data.
                    .Select(x => new PlanPricingInventoryProgram
                    {
                        ManifestId = x.id,
                        SpotLengthId = x.spot_length_id,
                        ManifestWeeks = x.station_inventory_manifest_weeks
                                                             .Where(w => w.media_week_id >= startMediaWeekId && w.media_week_id <= endMediaWeekId)
                                                             .Select(w => new PlanPricingInventoryProgram.ManifestWeek
                                                             {
                                                                 Id = w.id,
                                                                 InventoryMediaWeekId = w.media_week_id,
                                                                 Spots = w.spots
                                                             })
                                                             .ToList(),
                        ManifestRates = x.station_inventory_manifest_rates
                                                             .Where(r => spotLengthIds.Contains(r.spot_length_id))
                                                             .Select(r => new PlanPricingInventoryProgram.ManifestRate
                                                             {
                                                                 SpotLengthId = r.spot_length_id,
                                                                 Cost = r.spot_cost
                                                             })
                                                             .ToList(),
                        Station = new DisplayBroadcastStation()
                        {
                            Id = x.station.id,
                            Affiliation = x.station.affiliation,
                            Code = x.station.station_code,
                            CallLetters = x.station.station_call_letters,
                            LegacyCallLetters = x.station.legacy_call_letters,
                            MarketCode = x.station.market_code,
                            IsTrueInd = x.station.is_true_ind
                        },
                        InventorySource = new InventorySource
                        {
                            Id = x.inventory_sources.id,
                            InventoryType = (InventorySourceTypeEnum)x.inventory_sources.inventory_source_type,
                            IsActive = x.inventory_sources.is_active,
                            Name = x.inventory_sources.name
                        },
                        ManifestDayparts = x.station_inventory_manifest_dayparts
                                                             .Where(d => d.primary_program_id.HasValue)
                                                             .Select(d => new PlanPricingInventoryProgram.ManifestDaypart
                                                             {
                                                                 Id = d.id,
                                                                 Daypart = new DisplayDaypart
                                                                 {
                                                                     Id = d.daypart_id
                                                                 },
                                                                 ProgramName = d.program_name,
                                                                 PrimaryProgramId = d.primary_program_id,
                                                                 Programs = d.station_inventory_manifest_daypart_programs.Select(
                                                                     z => new PlanPricingInventoryProgram.ManifestDaypart.Program
                                                                     {
                                                                         Id = z.id,
                                                                         Name = z.name,
                                                                         ShowType = z.show_type,
                                                                         Genre = z.maestro_genre.name,
                                                                         StartTime = z.start_time,
                                                                         EndTime = z.end_time
                                                                     }).ToList()
                                                             }).ToList(),
                        ManifestAudiences = x.station_inventory_manifest_audiences
                                                             .Where(a => a.is_reference)
                                                             .Select(a => new PlanPricingInventoryProgram.ManifestAudience
                                                             {
                                                                 AudienceId = a.audience_id,
                                                                 Impressions = a.impressions
                                                             })
                                                             .ToList()
                    }).ToList();

                    foundManifests.AddRange(queryResult);
                }

                return foundManifests;
            });
        }

        public List<PlanBuyingInventoryProgram> GetProgramsForBuyingModel(
            DateTime startDate,
            DateTime endDate,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds)
        {
            return _InReadUncommitedTransaction(context =>
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
                                   spotLengthIds.Contains(manifestRate.spot_length_id) &&
                                   manifest.station_inventory_manifest_dayparts.Any(m => m.primary_program_id != null)
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

                return query
                    .Select(x => new PlanBuyingInventoryProgram
                    {
                        ManifestId = x.id,
                        SpotLengthId = x.spot_length_id,
                        ManifestWeeks = x.station_inventory_manifest_weeks
                            .Where(w => w.start_date <= endDate && w.end_date >= startDate)
                            .Select(w => new PlanBuyingInventoryProgram.ManifestWeek
                            {
                                Id = w.id,
                                InventoryMediaWeekId = w.media_week_id,
                                Spots = w.spots
                            })
                            .ToList(),
                        ManifestRates = x.station_inventory_manifest_rates
                            .Where(r => spotLengthIds.Contains(r.spot_length_id))
                            .Select(r => new BasePlanInventoryProgram.ManifestRate
                            {
                                SpotLengthId = r.spot_length_id,
                                Cost = r.spot_cost
                            })
                            .ToList(),
                        Station = new DisplayBroadcastStation()
                        {
                            Id = x.station.id,
                            Affiliation = x.station.affiliation,
                            Code = x.station.station_code,
                            CallLetters = x.station.station_call_letters,
                            LegacyCallLetters = x.station.legacy_call_letters,
                            MarketCode = x.station.market_code,
                            OwnershipGroupName = x.station.owner_name,
                            RepFirmName = x.station.rep_firm_name,
                            IsTrueInd = x.station.is_true_ind
                        },
                        InventorySource = new InventorySource
                        {
                            Id = x.inventory_sources.id,
                            InventoryType = (InventorySourceTypeEnum)x.inventory_sources.inventory_source_type,
                            IsActive = x.inventory_sources.is_active,
                            Name = x.inventory_sources.name
                        },
                        ManifestDayparts = x.station_inventory_manifest_dayparts
                            .Where(d => d.primary_program_id.HasValue)
                            .Select(d => new BasePlanInventoryProgram.ManifestDaypart
                            {
                                Id = d.id,
                                Daypart = new DisplayDaypart
                                {
                                    Id = d.daypart_id
                                },
                                ProgramName = d.program_name,
                                PrimaryProgramId = d.primary_program_id,
                                Programs = d.station_inventory_manifest_daypart_programs.Select(
                                    z => new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                    {
                                        Id = z.id,
                                        Name = z.name,
                                        ShowType = z.show_type,
                                        Genre = z.maestro_genre.name,
                                        StartTime = z.start_time,
                                        EndTime = z.end_time
                                    }).ToList()
                            }).ToList(),
                        ManifestAudiences = x.station_inventory_manifest_audiences
                            .Where(a => a.is_reference)
                            .Select(a => new PlanBuyingInventoryProgram.ManifestAudience
                            {
                                AudienceId = a.audience_id,
                                Impressions = a.impressions
                            })
                            .ToList()
                    })
                    .ToList();
            });
        }

        /// <inheritdoc/>
        public List<PlanBuyingInventoryProgram> GetProgramsForBuyingModelv2(
            DateTime startDate,
            DateTime endDate,
            List<int> spotLengthIds,
            IEnumerable<int> inventorySourceIds,
            List<int> stationIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                /*
                 * In some plans the size of the data used up the resources on the server and this blew up.
                 * We have split up the query to bring down the memory footprint.
                 * 
                 * 1. Query to get just the list of manifests to get.
                 * 2. Gather the details in chunks.
                 * 3. Transform at query-time to not bring back all that unused data.
                 * */

                // use the media weeks ids for a more direct and faster query.
                var startMediaWeekId = context.media_weeks
                    .Single(w => w.start_date <= startDate && w.end_date >= startDate).id;

                var endMediaWeekId = context.media_weeks
                    .Single(w => w.start_date <= endDate.Date && w.end_date >= endDate.Date).id;

                var manifestIds = context.station_inventory_manifest
                    .Where(s =>
                        inventorySourceIds.Contains(s.inventory_source_id)
                        // has weeks within this time frame. 
                        // that it has weeks at all guarantees that it is "active"
                        && s.station_inventory_manifest_weeks.Any(w => w.media_week_id >= startMediaWeekId && w.media_week_id <= endMediaWeekId)
                        // within our list of target stations
                        && s.station_id.HasValue && stationIds.Contains(s.station_id.Value)
                        // has a program
                        && s.station_inventory_manifest_dayparts.Any(d => d.primary_program_id != null)
                        // has rates
                        && spotLengthIds.Intersect(s.station_inventory_manifest_rates.Select(r => r.spot_length_id)).Any()
                    )
                    .Select(s => s.id)
                    .Distinct()
                    .ToList();

                // Chunk up what we found to minimize the memory of each query.
                // 10,000 hits the resource issue.
                //  7,500 clears it and plays nicer with the neighbors.
                const int chunkSize = 7500;
                var idChunks = manifestIds.GetChunks(chunkSize);

                var foundManifests = new List<PlanBuyingInventoryProgram>();

                foreach (var chunk in idChunks)
                {
                    var queryResult = context.station_inventory_manifest
                    // .Includes(...) slowed this down, so don't use them.
                    // They are not needed since we transform in-line with the query.
                    .Where(s => chunk.Contains(s.id))
                    // Transform in-line here to not bring back all that unused data.
                    .Select(x => new PlanBuyingInventoryProgram
                    {
                        ManifestId = x.id,
                        SpotLengthId = x.spot_length_id,
                        ManifestWeeks = x.station_inventory_manifest_weeks
                            .Where(w => w.media_week_id >= startMediaWeekId && w.media_week_id <= endMediaWeekId)
                            .Select(w => new PlanBuyingInventoryProgram.ManifestWeek
                            {
                                Id = w.id,
                                InventoryMediaWeekId = w.media_week_id,
                                Spots = w.spots
                            })
                            .ToList(),
                        ManifestRates = x.station_inventory_manifest_rates
                            .Where(r => spotLengthIds.Contains(r.spot_length_id))
                            .Select(r => new BasePlanInventoryProgram.ManifestRate
                            {
                                SpotLengthId = r.spot_length_id,
                                Cost = r.spot_cost
                            })
                            .ToList(),
                        Station = new DisplayBroadcastStation()
                        {
                            Id = x.station.id,
                            Affiliation = x.station.affiliation,
                            Code = x.station.station_code,
                            CallLetters = x.station.station_call_letters,
                            LegacyCallLetters = x.station.legacy_call_letters,
                            MarketCode = x.station.market_code,
                            IsTrueInd = x.station.is_true_ind
                        },
                        InventorySource = new InventorySource
                        {
                            Id = x.inventory_sources.id,
                            InventoryType = (InventorySourceTypeEnum)x.inventory_sources.inventory_source_type,
                            IsActive = x.inventory_sources.is_active,
                            Name = x.inventory_sources.name
                        },
                        ManifestDayparts = x.station_inventory_manifest_dayparts
                                                             .Where(d => d.primary_program_id.HasValue)
                                                             .Select(d => new BasePlanInventoryProgram.ManifestDaypart
                                                             {
                                                                 Id = d.id,
                                                                 Daypart = new DisplayDaypart
                                                                 {
                                                                     Id = d.daypart_id
                                                                 },
                                                                 ProgramName = d.program_name,
                                                                 PrimaryProgramId = d.primary_program_id,
                                                                 Programs = d.station_inventory_manifest_daypart_programs.Select(
                                                                     z => new PlanBuyingInventoryProgram.ManifestDaypart.Program
                                                                     {
                                                                         Id = z.id,
                                                                         Name = z.name,
                                                                         ShowType = z.show_type,
                                                                         Genre = z.maestro_genre.name,
                                                                         StartTime = z.start_time,
                                                                         EndTime = z.end_time
                                                                     }).ToList()
                                                             }).ToList(),
                        ManifestAudiences = x.station_inventory_manifest_audiences
                                                             .Where(a => a.is_reference)
                                                             .Select(a => new PlanBuyingInventoryProgram.ManifestAudience
                                                             {
                                                                 AudienceId = a.audience_id,
                                                                 Impressions = a.impressions
                                                             })
                                                             .ToList()
                    }).ToList();

                    foundManifests.AddRange(queryResult);
                }

                return foundManifests;
            });
        }

        public Dictionary<int, BasePlanInventoryProgram.ManifestDaypart.Program> 
            GetPrimaryProgramsForManifestDayparts(IEnumerable<int> manifestDaypartIds)
        {
            var chunks = manifestDaypartIds.GetChunks(BroadcastConstants.DefaultDatabaseQueryChunkSize);

            var primaryPrograms = chunks
                .AsParallel()
                .SelectMany(chunk =>
                {
                    return _InReadUncommitedTransaction(
                        context =>
                        {
                            return context.station_inventory_manifest_dayparts
                                .Where(x => chunk.Contains(x.id) && x.primary_program_id != null)
                                .Include(x => x.station_inventory_manifest_daypart_programs)
                                .Include(x => x.station_inventory_manifest_daypart_programs.Select(p => p.maestro_genre))
                                .Select(x => x.station_inventory_manifest_daypart_programs.FirstOrDefault(p => p.id == x.primary_program_id.Value))
                                .ToList()
                                .Select(x => new
                                {
                                    x.station_inventory_manifest_daypart_id,
                                    program = _MapToPlanPricingInventoryProgram(x)
                                })
                                .ToList();
                        });
                })
                .ToList();

            var result = primaryPrograms.Where(x => x != null).ToDictionary(
                x => x.station_inventory_manifest_daypart_id,
                x => x.program);

            return result;
        }

        private BasePlanInventoryProgram.ManifestDaypart.Program _MapToPlanPricingInventoryProgram(station_inventory_manifest_daypart_programs program)
        {
            if (program == null)
                return null;

            return new BasePlanInventoryProgram.ManifestDaypart.Program
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
