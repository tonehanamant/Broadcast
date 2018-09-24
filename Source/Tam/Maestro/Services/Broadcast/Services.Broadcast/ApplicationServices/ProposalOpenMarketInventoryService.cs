using System;
using System.Collections.Generic;
using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Linq;
using Services.Broadcast.Entities.DTO;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.Entities.OpenMarketInventory;
using Tam.Maestro.Common;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProposalOpenMarketInventoryService : IApplicationService
    {
        ProposalDetailOpenMarketInventoryDto GetInventory(int proposalDetailId, ProposalOpenMarketFilter openMarketFilter = null);
        ProposalDetailOpenMarketInventoryDto RefinePrograms(OpenMarketRefineProgramsRequest request, ProposalOpenMarketFilter openMarketFilter = null);
        ProposalDetailOpenMarketInventoryDto SaveInventoryAllocations(OpenMarketAllocationSaveRequest request);
        ProposalDetailOpenMarketInventoryDto UpdateOpenMarketInventoryTotals(ProposalDetailOpenMarketInventoryDto proposalInventoryDto);
        ProposalDetailOpenMarketInventoryDto ApplyFilterOnOpenMarketInventory(ProposalDetailOpenMarketInventoryDto proposalInventoryDto);
        List<OpenMarketInventoryAllocation> GetProposalInventoryAllocations(int proposalVersionDetailId);
        PricingGuideOpenMarketInventoryDto GetPricingGuideOpenMarketInventory(PricingGuideOpenMarketInventoryRequestDto request);
        PricingGuideOpenMarketInventoryDto ApplyFilterOnOpenMarketPricingGuideGrid(PricingGuideOpenMarketInventoryDto dto);
    }

    public class ProposalOpenMarketInventoryService : BaseProposalInventoryService, IProposalOpenMarketInventoryService
    {
        private readonly IProposalProgramsCalculationEngine _ProposalProgramsCalculationEngine;
        private readonly IProposalOpenMarketsTotalsCalculationEngine _ProposalOpenMarketsTotalsCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IProposalRepository _ProposalRepository;

        private const string HOUSEHOLD_AUDIENCE_CODE = "HH";

        public ProposalOpenMarketInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache, IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalProgramsCalculationEngine proposalProgramsCalculationEngine,
            IProposalOpenMarketsTotalsCalculationEngine proposalOpenMarketsTotalsCalculationEngine,
            IImpressionAdjustmentEngine impressionAdjustmentEngine,
            IProposalTotalsCalculationEngine proposalTotalsCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
            : base(broadcastDataRepositoryFactory, daypartCache, proposalMarketsCalculationEngine, impressionAdjustmentEngine, proposalTotalsCalculationEngine)
        {
            _ProposalProgramsCalculationEngine = proposalProgramsCalculationEngine;
            _ProposalOpenMarketsTotalsCalculationEngine = proposalOpenMarketsTotalsCalculationEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _ProposalRepository = broadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
        }

        public ProposalDetailOpenMarketInventoryDto GetInventory(int proposalDetailId, ProposalOpenMarketFilter openMarketFilter = null)
        {
            var proposalDetail = _ProposalRepository.GetProposalDetail(proposalDetailId);
            var request = new OpenMarketRefineProgramsRequest
            {
                ProposalDetailId = proposalDetailId,
                Criteria = new OpenMarketCriterion
                {
                    GenreSearchCriteria = proposalDetail.GenreCriteria,
                    ProgramNameSearchCriteria = proposalDetail.ProgramCriteria,
                    CpmCriteria = proposalDetail.CpmCriteria
                }
            };
            return RefinePrograms(request, openMarketFilter);

        }

        private static void _SetProposalOpenMarketDisplayFilters(ProposalDetailOpenMarketInventoryDto dto)
        {
            if (!dto.Weeks.Any()) return;

            dto.DisplayFilter = new ProposalOpenMarketDisplayFilter();

            // quick check through markets/stations data
            var stations =
                dto.Markets
                    .Where(m => m.Stations != null && m.Stations.Any())
                    .SelectMany(s => s.Stations)
                    .ToList();

            dto.DisplayFilter.Affiliations = stations.Select(q => q.Affiliation)
                .Distinct()
                .OrderBy(a => a)
                .ToList();
            // todo: deal with genre
            //dto.DisplayFilter.Genres = stations.Where(p => p.Programs.Any())
            //    .SelectMany(p => p.Programs.Where(g => g != null && g.Genres.Any()).SelectMany(z => z.Genres))
            //    .GroupBy(g => new { g.Id, g.Display })
            //    .Distinct()
            //    .Select(a => a.First())
            //    .OrderBy(b => b.Display)
            //    .ToList();
            dto.DisplayFilter.Markets = dto.Markets
                .Select(a => new { a.MarketId, a.MarketName })
                .Distinct()
                .Select(y => new LookupDto { Id = y.MarketId, Display = y.MarketName })
                .OrderBy(n => n.Display)
                .ToList();
            dto.DisplayFilter.ProgramNames = stations.Where(p => p.Programs.Any())
                .SelectMany(p => p.Programs.Where(l => l != null).SelectMany(z => z.ProgramNames))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(m => m)
                .ToList();
        }

        public ProposalDetailOpenMarketInventoryDto RefinePrograms(OpenMarketRefineProgramsRequest request, ProposalOpenMarketFilter openMarketFilter)
        {
            if (request.Criteria.CpmCriteria.GroupBy(c => c.MinMax).Any(g => g.Count() > 1))
            {
                throw new ApplicationException("Only 1 Min CPM and 1 Max CPM criteria allowed.");
            }

            var dto = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>().GetOpenMarketProposalDetailInventory(request.ProposalDetailId);
            UpdateCriteria(dto, request.Criteria);
            _PopulateMarkets(dto, request.IgnoreExistingAllocation);
            _ApplyProgramAndGenreFilter(dto, request.Criteria);
            _PopulateInventoryWeeks(dto);
            _SetProposalOpenMarketDisplayFilters(dto);
            _CalculateOpenMarketTotals(dto);

            if (openMarketFilter != null)
                dto.Filter = openMarketFilter;

            return dto;
        }

        private void _ApplyProgramAndGenreFilter(ProposalDetailOpenMarketInventoryDto dto, OpenMarketCriterion criteria)
        {
            List<ProposalInventoryMarketDto.InventoryMarketStationProgram> programsToExclude = new List<ProposalInventoryMarketDto.InventoryMarketStationProgram>();
            var programNamesToExclude = criteria.ProgramNameSearchCriteria.Where(x => x.Contain == ContainTypeEnum.Exclude).Select(x => x.Program.Display).ToList();
            var genreIdsToInclude = criteria.GenreSearchCriteria.Where(x => x.Contain == ContainTypeEnum.Include).Select(x => x.Genre.Id).ToList();
            var genreIdsToExclude = criteria.GenreSearchCriteria.Where(x => x.Contain == ContainTypeEnum.Exclude).Select(x => x.Genre.Id).ToList();
            dto.Markets.ForEach(market => market.Stations.ForEach(station => station.Programs.ForEach(program =>
            {
                foreach (var id in genreIdsToInclude)
                {
                    if (program.Genres.All(x => x.Id != id))
                    {
                        programsToExclude.Add(program);
                    }
                }
                foreach (var id in genreIdsToExclude)
                {
                    if (program.Genres.Any(x => x.Id == id))
                    {
                        programsToExclude.Add(program);
                    }
                }
                foreach (var name in programNamesToExclude)
                {
                    if (program.ProgramNames.Any(x=> name.Equals(x)))
                    {
                        programsToExclude.Add(program);
                    }
                }
            })));

            dto.Markets.ForEach(x => x.Stations.ForEach(y => y.Programs.RemoveAll(z => programsToExclude.Contains(z))));
        }

        private void _CalculateOpenMarketTotals(ProposalDetailOpenMarketInventoryDto dto)
        {
            var totals = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                .GetProposalDetailProprietaryInventoryTotals(dto.DetailId);
            var weekTotals = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                .GetProposalDetailProprietaryWeekInventoryTotals(dto.DetailId);
            _ProposalOpenMarketsTotalsCalculationEngine.CalculateOpenMarketDetailTotals(dto, totals, weekTotals);
        }

        public ProposalDetailOpenMarketInventoryDto UpdateOpenMarketInventoryTotals(
            ProposalDetailOpenMarketInventoryDto proposalInventoryDto)
        {
            // the spot filter should be ignored when editing totals.
            _ApplyProposalOpenMarketFilter(proposalInventoryDto, false);
            return proposalInventoryDto;
        }

        public ProposalDetailOpenMarketInventoryDto ApplyFilterOnOpenMarketInventory(
            ProposalDetailOpenMarketInventoryDto proposalInventoryDto)
        {
            _ApplyProposalOpenMarketFilter(proposalInventoryDto, true);
            return proposalInventoryDto;
        }

        private void _ApplyProposalOpenMarketFilter(ProposalDetailOpenMarketInventoryDto dto, bool useFilters)
        {
            _SetProposalOpenMarketDisplayFilters(dto);

            // there is a requirement where if user is editing spot with filter on (programs without spots), FE needs to maintain the edited grid line
            // Be does not know about it (the state) and would filter and calculte whatever comes down. The user filter allows calculate without considering the filter
            // Also, when editing spots, the "active" grid is sent down.
            // the flag below ignores the spot filter and mantain the structure to do the calculation
            if (!useFilters)
                dto.Filter.Clear();

            // will filter markets, stations and programs
            _ApplyFilterForProposalInventoryMarkets(dto);
            // after filtering, set the week structure
            _SetOpenMarketWeeksForFilteredMarkets(dto);

            _ProposalOpenMarketsTotalsCalculationEngine.CalculateOpenMarketProgramTotals(dto);
            _CalculateOpenMarketTotals(dto);
        }

        private void _ApplyFilterForProposalInventoryMarkets(ProposalDetailOpenMarketInventoryDto dto)
        {
            if (dto.Filter == null) return;
            var filter = dto.Filter;

            // to be used to filter programs with/without spots
            var listOfPrograms =
                dto.Weeks.SelectMany(
                    a =>
                        a.Markets.SelectMany(b => b.Stations.SelectMany(c => c.Programs.Where(d => d != null)))).ToList();

            // filter market
            var markets = dto.Markets.Where(
                    m =>
                        filter.Markets == null || !filter.Markets.Any() || filter.Markets.Any(a => a == m.MarketId)
                )
                .Select(m => new ProposalInventoryMarketDto
                {
                    MarketId = m.MarketId,
                    MarketName = m.MarketName,
                    MarketSubscribers = m.MarketSubscribers,
                    MarketRank = m.MarketRank,
                    Stations =
                        // filter affiliation
                        m.Stations.Where(
                                s =>
                                    filter.Affiliations == null || !filter.Affiliations.Any() || filter.Affiliations.Any(
                                        a => s.Affiliation.ToLower().Contains(a.ToLower())))
                            .Select(s1 => new ProposalInventoryMarketDto.InventoryMarketStation
                            {
                                StationCode = s1.StationCode,
                                Affiliation = s1.Affiliation,
                                CallLetters = s1.CallLetters,
                                LegacyCallLetters = s1.LegacyCallLetters,
                                Programs =
                                    // filter day parts - needs to intersect
                                    s1.Programs.Where(
                                            a =>
                                                filter.DayParts == null || !filter.DayParts.Any() ||
                                                filter.DayParts.Any(
                                                    filtersDaypart => a.Dayparts.Any(dp => DisplayDaypart.Intersects(DaypartDto.ConvertDaypartDto(filtersDaypart),
                                                        DaypartCache.GetDisplayDaypart(dp.Id)))))
                                        // filter program names
                                        .Where(
                                            p =>
                                                filter.ProgramNames == null || !filter.ProgramNames.Any() ||
                                                filter.ProgramNames.Any(
                                                    filterProgramName =>
                                                        p.ProgramNames.Any(manifestProgramName => string.Compare(manifestProgramName, filterProgramName,
                                                                                                      StringComparison.OrdinalIgnoreCase) == 0)))                                        // filter genres
                                        .Where(
                                            sp =>
                                                filter.Genres == null || !filter.Genres.Any() ||
                                                filter.Genres.Intersect(sp.Genres.Select(g => g.Id)).Any())
                                        // apply filter per spot
                                        .Where(
                                            ap =>
                                                !filter.SpotFilter.HasValue || filter.SpotFilter ==
                                                ProposalOpenMarketFilter.OpenMarketSpotFilter.AllPrograms ||
                                                filter.SpotFilter ==
                                                ProposalOpenMarketFilter.OpenMarketSpotFilter.ProgramWithSpots &&
                                                listOfPrograms.Where(pa => pa.ProgramId == ap.ProgramId)
                                                    .Sum(s => s.Spots) > 0 ||
                                                filter.SpotFilter ==
                                                ProposalOpenMarketFilter.OpenMarketSpotFilter.ProgramWithoutSpots &&
                                                listOfPrograms.Where(pa => pa.ProgramId == ap.ProgramId)
                                                    .Sum(s => s.Spots) == 0)
                                        .Select(pro => new ProposalInventoryMarketDto.InventoryMarketStationProgram
                                        {
                                            ProgramId = pro.ProgramId,
                                            ProgramNames = pro.ProgramNames,
                                            Dayparts = pro.Dayparts,
                                            TargetCpm = pro.TargetCpm,
                                            TargetImpressions = pro.TargetImpressions,
                                            UnitImpressions = pro.UnitImpressions,
                                            Genres = pro.Genres,
                                            Spots = pro.Spots,
                                            FlightWeeks = pro.FlightWeeks,
                                            StationCode = pro.StationCode
                                        })
                                        .ToList()
                            }).ToList()
                }).ToList();

            dto.Markets = markets;
        }

        private static void _SetOpenMarketWeeksForFilteredMarkets(ProposalDetailOpenMarketInventoryDto dto)
        {
            var groupedWeekPrograms = dto.Weeks.GroupBy(x => x.MediaWeekId)
                .ToDictionary(m => m.Key,
                    n =>
                        n.SelectMany(
                                y => y.Markets.SelectMany(t => t.Stations.SelectMany(o => o.Programs)))
                            .ToList());

            foreach (var week in dto.Weeks)
            {
                week.Markets = new List<ProposalOpenMarketInventoryWeekDto.InventoryWeekMarket>();
                List<ProposalOpenMarketInventoryWeekDto.InventoryWeekProgram> weekPrograms;
                groupedWeekPrograms.TryGetValue(week.MediaWeekId, out weekPrograms);

                foreach (var market in dto.Markets)
                {
                    var weekMarket = new ProposalOpenMarketInventoryWeekDto.InventoryWeekMarket
                    {
                        MarketId = market.MarketId,
                        Stations = new List<ProposalOpenMarketInventoryWeekDto.InventoryWeekStation>()
                    };

                    foreach (var station in market.Stations)
                    {
                        var weekStation = new ProposalOpenMarketInventoryWeekDto.InventoryWeekStation
                        {
                            StationCode = station.StationCode,
                            Programs = new List<ProposalOpenMarketInventoryWeekDto.InventoryWeekProgram>()
                        };

                        foreach (var program in station.Programs)
                        {
                            if (weekPrograms != null && weekPrograms.Any())
                            {
                                // it can either be a null program just for filling the grid or the actual program with values
                                // if it is null, does not matter, FE will just fake the program
                                var weekProgram = weekPrograms.SingleOrDefault(a => a != null && a.ProgramId == program.ProgramId);
                                weekStation.Programs.Add(weekProgram);
                            }
                        }
                        weekMarket.Stations.Add(weekStation);
                    }

                    week.Markets.Add(weekMarket);
                }
            }
        }

        private void _PopulateMarkets(ProposalDetailOpenMarketInventoryDto dto, bool ignoreExistingAllocation)
        {
            _SetProposalInventoryDetailDaypart(dto);
            _SetProposalInventoryDetailSpotLength(dto);

            var proposalMarketIds = ProposalMarketsCalculationEngine.GetProposalMarketsList(dto.ProposalId, dto.ProposalVersion,
                    dto.DetailId).Select(m => (short)m.Id).ToList();
            var programs = BroadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>()
                .GetStationProgramsForProposalDetail(dto.DetailFlightStartDate, dto.DetailFlightEndDate,
                    dto.DetailSpotLengthId, BroadcastConstants.OpenMarketSourceId, proposalMarketIds, dto.DetailId);

            _SetFlightWeeks(programs);

            //// represents the actual program names before any refine is applied
            dto.RefineFilterPrograms = programs.Where(l => l != null).SelectMany(z => z.ManifestDayparts.Select(md => md.ProgramName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(m => m)
                .ToList();

            var proposalDetailDaypart = DaypartCache.GetDisplayDaypart(dto.DetailDaypartId.Value);
            var filteredProgramsWithAllocations = new List<int>();
            programs.RemoveAll(p =>
            {
                return
                    p.ManifestDayparts.All(
                        d => !DaypartCache.GetDisplayDaypart(d.DaypartId).Intersects(proposalDetailDaypart));

                //if (FilterByGenreAndProgramNameCriteria(p, dto.Criteria))
                //{
                //    if (p.FlightWeeks.Any(fw => fw.Allocations.Any(a => a.Spots > 0)))
                //    {
                //        if (!ignoreExistingAllocation)
                //        {
                //            dto.NewCriteriaAffectsExistingAllocations = true;

                //            return false;
                //        }

                //        filteredProgramsWithAllocations.Add(p.ProgramId);
                //    }

                //    return true;
                //}

            });

            if (filteredProgramsWithAllocations.Any())
                BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>()
                    .RemoveAllocations(filteredProgramsWithAllocations, dto.DetailId);

            if (programs.Count < 1)
            {
                return;
            }

            _ApplyDaypartNames(programs);
            _ApplyProjectedImpressions(programs, dto);
            _ApplyStationImpressions(programs, dto);
            _ProposalProgramsCalculationEngine.CalculateCpmForPrograms(programs, dto.DetailSpotLength);

            filteredProgramsWithAllocations.Clear();
            // todo: fix allocations
            //programs.RemoveAll(p =>
            //{
            //    if (FilterByCpmCriteria(p, dto.Criteria.CpmCriteria))
            //    {
            //        if (p.FlightWeeks.Any(fw => fw.Allocations.Any(a => a.Spots > 0)))
            //        {
            //            if (!ignoreExistingAllocation)
            //            {
            //                dto.NewCriteriaAffectsExistingAllocations = true;
            //                return false;
            //            }
            //            filteredProgramsWithAllocations.Add(p.ProgramId);
            //        }
            //        return true;
            //    }
            //    return false;
            //});
            if (filteredProgramsWithAllocations.Any())
                BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>()
                    .RemoveAllocations(filteredProgramsWithAllocations, dto.DetailId);

            var inventoryMarkets = _GroupProgramsByMarketAndStation(programs);

            var postingBook = ProposalServiceHelper.GetBookId(dto);
            _ApplyInventoryMarketSubscribers(postingBook, inventoryMarkets);
            _ApplyInventoryMarketRankings(postingBook, inventoryMarkets);

            dto.Markets.AddRange(inventoryMarkets.OrderBy(m => m.MarketRank).ToList());

            _ApplyDefaultSorting(dto);
        }

        private void _ApplyStationImpressions(List<ProposalProgramDto> programs, ProposalDetailInventoryBase proposalDetail)
        {
            foreach (var program in programs)
            {
                var manifestAudienceForProposal = program.ManifestAudiences.SingleOrDefault(x => x.AudienceId == proposalDetail.GuaranteedAudience);
                var hasManifestAudiences = manifestAudienceForProposal != null && manifestAudienceForProposal.Impressions.HasValue;

                if (hasManifestAudiences)
                {
                    program.ProvidedUnitImpressions = manifestAudienceForProposal.Impressions.Value;
                }
            }            
        }

        private void _SetFlightWeeks(IEnumerable<ProposalProgramDto> programs)
        {
            foreach (var program in programs)
            {
                program.FlightWeeks = _GetFlightWeeks(program);
            }
        }

        private List<ProposalProgramFlightWeek> _GetFlightWeeks(ProposalProgramDto programDto)
        {
            var nonNullableEndDate = programDto.EndDate ?? programDto.StartDate.AddYears(1);

            var displayFlighWeeks = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(programDto.StartDate, nonNullableEndDate);

            var flighWeeks = new List<ProposalProgramFlightWeek>();

            foreach (var displayMediaWeek in displayFlighWeeks)
            {
                var totalSpotsAllocated = programDto.Allocations.Count(x => x.MediaWeekId == displayMediaWeek.Id);

                flighWeeks.Add(new ProposalProgramFlightWeek
                {
                    StartDate = displayMediaWeek.WeekStartDate,
                    EndDate = displayMediaWeek.WeekEndDate,
                    MediaWeekId = displayMediaWeek.Id,
                    Rate = programDto.SpotCost,
                    Allocations = new List<OpenMarketAllocationDto>
                    {
                        new OpenMarketAllocationDto
                        {
                            MediaWeekId = displayMediaWeek.Id,
                            Spots = totalSpotsAllocated
                        }
                    }
                });
            }

            return flighWeeks;
        }

        internal static bool FilterByCpmCriteria(ProposalProgramDto program, List<CpmCriteria> cpmCriteria)
        {
            foreach (var criteria in cpmCriteria)
            {
                if (criteria.MinMax == MinMaxEnum.Min && program.TargetCpm < criteria.Value)
                {
                    return true;
                }
                if (criteria.MinMax == MinMaxEnum.Max && program.TargetCpm > criteria.Value)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool FilterByGenreAndProgramNameCriteria(ProposalProgramDto program, OpenMarketCriterion marketCriterion)
        {
            foreach (var criteria in marketCriterion.GenreSearchCriteria.GroupBy(c => c.Contain)
                .Select(g => new { Type = g.Key, GenreIds = g.Select(gb => gb.Genre.Id) }))
            {
                var includeGenre = criteria.GenreIds.Intersect(program.Genres.Select(g => g.Id)).Any();
                if (criteria.Type == ContainTypeEnum.Include && !includeGenre)
                {
                    return true;
                }
                if (criteria.Type == ContainTypeEnum.Exclude && includeGenre)
                {
                    return true;
                }
            }
            foreach (var criteria in marketCriterion.ProgramNameSearchCriteria.GroupBy(c => c.Contain)
                .Select(g => new { Type = g.Key, ProgramNames = g.Select(gb => gb.Program.Display) }))
            {
                var includeProgramName = criteria.ProgramNames.Any(c => program.ManifestDayparts.Select(md => md.ProgramName).Any(pn => string.Equals(pn, c, StringComparison.CurrentCultureIgnoreCase)));
                if (criteria.Type == ContainTypeEnum.Include && !includeProgramName)
                {
                    return true;
                }
                if (criteria.Type == ContainTypeEnum.Exclude && includeProgramName)
                {
                    return true;
                }
            }
            return false;
        }

        private void _PopulateInventoryWeeks(ProposalDetailOpenMarketInventoryDto dto)
        {
            var existingAllocations = BroadcastDataRepositoryFactory
                .GetDataRepository<IProposalOpenMarketInventoryRepository>()
                .GetProposalDetailAllocations(dto.DetailId);

            foreach (var week in dto.Weeks)
            {
                week.Markets = new List<ProposalOpenMarketInventoryWeekDto.InventoryWeekMarket>();

                foreach (var market in dto.Markets)
                {
                    var weekMarket = new ProposalOpenMarketInventoryWeekDto.InventoryWeekMarket
                    {
                        MarketId = market.MarketId,
                        Stations = new List<ProposalOpenMarketInventoryWeekDto.InventoryWeekStation>()
                    };

                    foreach (var station in market.Stations)
                    {
                        var weekStation = new ProposalOpenMarketInventoryWeekDto.InventoryWeekStation
                        {
                            StationCode = station.StationCode,
                            Programs = new List<ProposalOpenMarketInventoryWeekDto.InventoryWeekProgram>()
                        };

                        foreach (var program in station.Programs)
                        {
                            var programFlightweek =
                                program.FlightWeeks.SingleOrDefault(
                                    f => f.MediaWeekId == week.MediaWeekId && f.IsHiatus == false);

                            if (programFlightweek == null)
                            {
                                weekStation.Programs.Add(null);
                                continue;
                            }
                            
                            var unitImpressions = program.UnitImpressions;
                            var weekProgram = new ProposalOpenMarketInventoryWeekDto.InventoryWeekProgram();
                            var numberOfSpotsAllocated =
                                existingAllocations.Count(
                                    a => a.MediaWeekId == week.MediaWeekId && a.ManifestId == program.ProgramId);
                            weekProgram.ProgramId = program.ProgramId;
                            weekProgram.Spots = numberOfSpotsAllocated;
                            weekProgram.UnitImpression = program.UnitImpressions;
                            weekProgram.ProvidedUnitImpressions = program.ProvidedUnitImpressions;
                            weekProgram.UnitCost = programFlightweek.Rate;
                            weekProgram.TargetImpressions = program.TargetImpressions;
                            weekProgram.TotalImpressions = weekProgram.Spots > 0 ? unitImpressions * weekProgram.Spots : unitImpressions;

                            if (program.ProvidedUnitImpressions.HasValue)
                            {
                                var providedUnitImpressions = program.ProvidedUnitImpressions.Value;
                                weekProgram.TotalProvidedImpressions = weekProgram.Spots > 0 ? providedUnitImpressions * weekProgram.Spots : providedUnitImpressions;
                            }

                            weekProgram.Cost = weekProgram.Spots > 0 ? weekProgram.Spots * weekProgram.UnitCost : weekProgram.UnitCost;

                            weekStation.Programs.Add(weekProgram);
                        }
                        weekMarket.Stations.Add(weekStation);
                    }
                    week.Markets.Add(weekMarket);
                }
            }
        }

        internal void UpdateCriteria(ProposalDetailOpenMarketInventoryDto dto, OpenMarketCriterion newCriterion)
        {
            var newCpmCriterion = newCriterion.CpmCriteria.Where(c => !c.Id.HasValue).ToList();
            var deleteCpmCriterion = dto.Criteria.CpmCriteria.Where(oc => oc.Id.HasValue && !newCriterion.CpmCriteria.Select(c => c.Id).Contains(oc.Id)).Select(oc => oc.Id.Value).ToList();

            var newGenreCriteria = newCriterion.GenreSearchCriteria.Where(c => !c.Id.HasValue).ToList();
            var deleteGenreCriteria = dto.Criteria.GenreSearchCriteria.Where(oc => oc.Id.HasValue && !newCriterion.GenreSearchCriteria.Select(c => c.Id).Contains(oc.Id)).Select(oc => oc.Id.Value).ToList();

            var newProgramNameCriteria = newCriterion.ProgramNameSearchCriteria.Where(c => !c.Id.HasValue).ToList();
            var oldProgramNameCriteria = dto.Criteria.ProgramNameSearchCriteria.Where(oc => oc.Id.HasValue && !newCriterion.ProgramNameSearchCriteria.Select(c => c.Id).Contains(oc.Id)).Select(oc => oc.Id.Value).ToList();

            var criteriaRepository = BroadcastDataRepositoryFactory.GetDataRepository<IProposalProgramsCriteriaRepository>();
            var criteria = criteriaRepository.UpdateCriteria(dto.DetailId, newCpmCriterion, deleteCpmCriterion, newGenreCriteria, deleteGenreCriteria, newProgramNameCriteria, oldProgramNameCriteria);

            dto.Criteria = criteria;
        }

        private static void _ApplyDefaultSorting(ProposalDetailOpenMarketInventoryDto proposalInventory)
        {
            var sortedMarkets = proposalInventory.Markets.OrderBy(m => m.MarketRank);
            foreach (var market in sortedMarkets)
            {
                market.Stations = market.Stations.OrderBy(s => s.Programs.Min(p => p.TargetCpm)).ThenBy(s => s.LegacyCallLetters).ToList();
                foreach (var station in market.Stations)
                {
                    station.Programs = station.Programs.OrderBy(p => p.TargetCpm).ToList();
                }
            }
            proposalInventory.Markets = sortedMarkets.ToList();
        }

        private void _ApplyProjectedImpressions(IEnumerable<ProposalProgramDto> programs,
            ProposalDetailInventoryBase proposalDetail)
        {
            var impressionRequests = new List<ManifestDetailDaypart>();
            var stationDetailImpressions = new Dictionary<int, ProposalProgramDto>();
            var manifestDaypartImpressions = new Dictionary<int, double>();

            foreach (var program in programs)
            {
                foreach (var manifestDaypart in program.ManifestDayparts)
                {
                    var manifestDp = Common.Services.DaypartCache.Instance.GetDisplayDaypart(manifestDaypart.DaypartId);

                    var stationDaypart = new ManifestDetailDaypart
                    {
                        LegacyCallLetters = program.Station.LegacyCallLetters,
                        Id = manifestDaypart.Id,
                        DisplayDaypart = manifestDp
                    };
                    impressionRequests.Add(stationDaypart);
                    manifestDaypartImpressions.Add(manifestDaypart.Id, 0); //initialize with zero

                }

                stationDetailImpressions[program.ManifestId] = program;
            }

            var ratingAudiences = BroadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>()
                .GetRatingsAudiencesByMaestroAudience(
                    new List<int>
                    {
                        proposalDetail.GuaranteedAudience.Value
                    }).Select(r => r.rating_audience_id).Distinct().ToList();

            var programImpressions = GetImpressions(proposalDetail, ratingAudiences, impressionRequests);

            foreach (var imp in programImpressions)
            {
                manifestDaypartImpressions[imp.id] += imp.impressions;
            }

            foreach (var program in programs)
            {
                var programManifestDaypartIds = program.ManifestDayparts.Select(d => d.Id).ToList();
                var programDaypartImpressions =
                                        manifestDaypartImpressions.Where(i => programManifestDaypartIds.Contains(i.Key)).ToList();
                var daypartCount = programManifestDaypartIds.Count;
                if (daypartCount > 0)
                {
                    program.UnitImpressions = programDaypartImpressions.Sum(i => i.Value) / daypartCount;
                }
            }
        }

        private void _ApplyDaypartNames(List<ProposalProgramDto> programs)
        {
            var programDaypartIds = programs.SelectMany(p => p.ManifestDayparts.Select(md => md.DaypartId)).Distinct().ToList();
            var programDayparts = DaypartCache.GetDisplayDayparts(programDaypartIds);

            foreach (var program in programs)
            {
                program.DayParts =
                    program.ManifestDayparts.Select(md => md.DaypartId).Select(daypartId => new LookupDto(daypartId, programDayparts[daypartId].ToString()))
                        .ToList();
            }
        }

        private void _ApplyInventoryMarketRankings(int mediaMonthId, IEnumerable<IInventoryMarket> inventoryMarkets)
        {
            var marketRankings =
                BroadcastDataRepositoryFactory.GetDataRepository<INsiMarketRepository>()
                    .GetMarketRankingsByMediaMonth(mediaMonthId);

            foreach (var inventoryMarket in inventoryMarkets)
            {
                marketRankings.TryGetValue(inventoryMarket.MarketId, out var rank);
                inventoryMarket.MarketRank = rank;
            }
        }

        private void _ApplyInventoryMarketCoverages(IEnumerable<IInventoryMarket> inventoryMarkets)
        {
            var marketsList = inventoryMarkets.ToList();

            var marketCoverages =
                BroadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>()
                    .GetMarketCoverages(marketsList.Select(x => x.MarketId));

            foreach (var inventoryMarket in marketsList)
            {
                marketCoverages.TryGetValue(inventoryMarket.MarketId, out var coverage);
                inventoryMarket.MarketCoverage = coverage;
            }
        }

        private static List<ProposalInventoryMarketDto> _GroupProgramsByMarketAndStation(IEnumerable<ProposalProgramDto> programs)
        {
            var programsByMarket = programs.GroupBy(p => p.Market.Id);
            var inventoryMarkets = programsByMarket.Select(
                g => new ProposalInventoryMarketDto
                {
                    MarketId = g.Key,
                    MarketName = g.First().Market.Display,
                    Stations = g.GroupBy(p => p.Station.StationCode).Select(s => new ProposalInventoryMarketDto.InventoryMarketStation
                    {
                        Affiliation = s.First().Station.Affiliation,
                        CallLetters = s.First().Station.CallLetters,
                        LegacyCallLetters = s.First().Station.LegacyCallLetters,
                        StationCode = s.First().Station.StationCode,
                        Programs = s.Select(p => new ProposalInventoryMarketDto.InventoryMarketStationProgram
                        {
                            ProgramId = p.ManifestId,
                            ProgramNames = p.ManifestDayparts.Select(md => md.ProgramName).ToList(),
                            TargetCpm = p.TargetCpm,
                            UnitImpressions = p.UnitImpressions,
                            ProvidedUnitImpressions = p.ProvidedUnitImpressions,
                            TargetImpressions = p.TargetImpressions,
                            Dayparts = p.DayParts,
                            Spots = p.TotalSpots,
                            FlightWeeks = p.FlightWeeks,
                            Genres = p.Genres,
                            StationCode = p.Station.StationCode
                        }).ToList()
                    }).ToList()
                }).ToList();

            return inventoryMarkets;
        }

        private void _ApplyInventoryMarketSubscribers(int monthId, IEnumerable<ProposalInventoryMarketDto> inventoryMarkets)
        {
            var householdAudienceId = BroadcastDataRepositoryFactory.GetDataRepository<IAudienceRepository>().GetDisplayAudienceByCode(HOUSEHOLD_AUDIENCE_CODE).Id;
            var marketSubscribers = BroadcastDataRepositoryFactory.GetDataRepository<INsiUniverseRepository>().GetUniverseDataByAudience(monthId, new List<int> { householdAudienceId });

            foreach (var inventoryMarket in inventoryMarkets)
            {
                marketSubscribers.TryGetValue((short)inventoryMarket.MarketId, out double subscribers);
                inventoryMarket.MarketSubscribers = subscribers;
            }
        }

        public List<OpenMarketInventoryAllocation> GetProposalInventoryAllocations(int proposalVersionDetailId)
        {
            var openMarketInventoryRepository = BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>();
            return openMarketInventoryRepository.GetProposalDetailAllocations(proposalVersionDetailId);
        }

        public ProposalDetailOpenMarketInventoryDto SaveInventoryAllocations(OpenMarketAllocationSaveRequest request)
        {
            var proposalRepository = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
            var openMarketInventoryRepository = BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>();
            var existingAllocations =
                openMarketInventoryRepository.GetProposalDetailAllocations(request.ProposalVersionDetailId);
            var guaranteedAudienceId = proposalRepository.GetProposalDetailGuaranteedAudienceId(request.ProposalVersionDetailId);
            var allocationToRemove = _GetAllocationsToRemove(request, existingAllocations);
            var allocationToAdd = _GetAllocationsToCreate(request, existingAllocations);

            _ValidateSpotsAllocation(allocationToAdd);

            using (var transaction = new TransactionScopeWrapper())
            {
                openMarketInventoryRepository.RemoveAllocations(allocationToRemove);

                openMarketInventoryRepository.AddAllocations(allocationToAdd, guaranteedAudienceId);

                var inventoryDto = GetInventory(request.ProposalVersionDetailId, null);

                _ProposalOpenMarketsTotalsCalculationEngine.CalculatePartialOpenMarketTotals(inventoryDto);

                var proposalDetailTotals = _GetProposalDetailTotals(inventoryDto);

                BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                    .SaveProposalDetailOpenMarketInventoryTotals(inventoryDto.DetailId, proposalDetailTotals);

                BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                    .SaveProposalDetailOpenMarketWeekInventoryTotals(inventoryDto);

                _UpdateProposalTotals(inventoryDto.ProposalVersionId);

                _CalculateOpenMarketTotals(inventoryDto);

                transaction.Complete();

                return GetInventory(request.ProposalVersionDetailId, request.Filter);
            }
        }

        private void _ValidateSpotsAllocation(IEnumerable<OpenMarketInventoryAllocation> allocations)
        {
            if (
                allocations.Any(
                    openMarketInventoryAllocation =>
                        openMarketInventoryAllocation.TotalImpressions == 0 &&
                        openMarketInventoryAllocation.Spots > 0))
            {
                throw new Exception("Cannot allocate spots that have zero impressions");
            }
        }

        private static ProposalDetailSingleInventoryTotalsDto _GetProposalDetailTotals(ProposalDetailInventoryBase inventoryDto)
        {
            return new ProposalDetailSingleInventoryTotalsDto
            {
                TotalCost = inventoryDto.DetailTotalBudget,
                TotalImpressions = inventoryDto.DetailTotalImpressions
            };
        }

        private static List<OpenMarketInventoryAllocation> _GetAllocationsToCreate(OpenMarketAllocationSaveRequest request, List<OpenMarketInventoryAllocation> existingAllocations)
        {
            var allocationsToAdd = new List<OpenMarketInventoryAllocation>();

            foreach (var week in request.Weeks)
            {
                foreach (var program in week.Programs)
                {
                    var numberOfPreviousAllocations = existingAllocations.Count(x => x.ManifestId == program.ProgramId);

                    if (program.Spots < numberOfPreviousAllocations)
                        continue;

                    var numberOfSpotsDifference = program.Spots - numberOfPreviousAllocations;

                    allocationsToAdd.Add(new OpenMarketInventoryAllocation
                    {
                        ManifestId = program.ProgramId,
                        MediaWeekId = week.MediaWeekId,
                        ProposalVersionDetailId = request.ProposalVersionDetailId,
                        Spots = numberOfSpotsDifference,
                        TotalImpressions = program.TotalImpressions,
                        UnitImpressions = program.UnitImpressions,
                        SpotCost = program.UnitCost,
                        Rate = ProposalMath.CalculateManifestSpotAudienceRate(program.UnitImpressions, program.UnitCost)
                    });
                }
            }

            return allocationsToAdd;
        }

        private static List<OpenMarketInventoryAllocation> _GetAllocationsToRemove(OpenMarketAllocationSaveRequest request, List<OpenMarketInventoryAllocation> existingAllocations)
        {
            var allocationsToRemove = new List<OpenMarketInventoryAllocation>();

            foreach (var week in request.Weeks)
            {
                foreach (var program in week.Programs)
                {
                    var numberOfPreviousAllocations = existingAllocations.Count(x => x.ManifestId == program.ProgramId);

                    if (program.Spots >= numberOfPreviousAllocations)
                        continue;

                    var numberOfSpotsDifference = numberOfPreviousAllocations - program.Spots;

                    allocationsToRemove.AddRange(
                        existingAllocations.Where(
                            x => x.MediaWeekId == week.MediaWeekId && x.ManifestId == program.ProgramId)
                            .Take(numberOfSpotsDifference));
                }
            }

            return allocationsToRemove;
        }

        private List<PricingGuideOpenMarketInventory.PricingGuideMarket> _GroupProgramsByMarketAndStationForPricingGuide(IEnumerable<ProposalProgramDto> programs)
        {
            var programsByMarket = programs.GroupBy(p => p.Market.Id);
            var inventoryMarkets = programsByMarket.Select(
                g => new PricingGuideOpenMarketInventory.PricingGuideMarket
                {
                    MarketId = g.Key,
                    MarketName = g.First().Market.Display,
                    Stations = g.GroupBy(p => p.Station.StationCode).Select(s => new PricingGuideOpenMarketInventory.PricingGuideMarket.PricingGuideStation
                    {
                        Affiliation = s.First().Station.Affiliation,
                        CallLetters = s.First().Station.CallLetters,
                        LegacyCallLetters = s.First().Station.LegacyCallLetters,
                        StationCode = s.First().Station.StationCode,
                        Programs = s.Select(p => new PricingGuideOpenMarketInventory.PricingGuideMarket.PricingGuideStation.PricingGuideProgram
                        {
                            ProgramId = p.ManifestId,
                            ProgramName = p.ManifestDayparts.Single().ProgramName,
                            BlendedCpm = p.TargetCpm,
                            ImpressionsPerSpot = p.UnitImpressions,
                            StationImpressions = p.ProvidedUnitImpressions ?? 0,
                            Daypart = p.DayParts.Single(),
                            CostPerSpot = p.SpotCost,
                            Cost = p.TotalCost,
                            Impressions = p.TotalImpressions,
                            Spots = p.TotalSpots,                        
                        }).ToList()
                    }).ToList()
                }).ToList();

            return inventoryMarkets;
        }

        private static void _ApplyDefaultSortingForPricingGuide(PricingGuideOpenMarketInventory proposalInventory)
        {
            var sortedMarkets = proposalInventory.Markets.OrderBy(m => m.MarketRank).ToList();

            foreach (var market in sortedMarkets)
            {
                market.Stations = market.Stations.OrderBy(s => s.Programs.Min(p => p.BlendedCpm)).ThenBy(s => s.LegacyCallLetters).ToList();
                foreach (var station in market.Stations)
                {
                    station.Programs = station.Programs.OrderBy(p => p.BlendedCpm).ToList();
                }
            }

            proposalInventory.Markets = sortedMarkets;
        }

        private void _ApplyProgramAndGenreFilterForPricingGuide(PricingGuideOpenMarketInventory dto, OpenMarketCriterion criterion)
        {
            var programsToExclude = new List<PricingGuideOpenMarketInventory.PricingGuideMarket.PricingGuideStation.PricingGuideProgram>();
            var programNamesToExclude = criterion.ProgramNameSearchCriteria.Where(x => x.Contain == ContainTypeEnum.Exclude).Select(x => x.Program.Display).ToList();
            var genreIdsToInclude = criterion.GenreSearchCriteria.Where(x => x.Contain == ContainTypeEnum.Include).Select(x => x.Genre.Id).ToList();
            var genreIdsToExclude = criterion.GenreSearchCriteria.Where(x => x.Contain == ContainTypeEnum.Exclude).Select(x => x.Genre.Id).ToList();
            dto.Markets.ForEach(market => market.Stations.ForEach(station => station.Programs.ForEach(program =>
            {
                foreach (var id in genreIdsToInclude)
                {
                    if (program.Genres.All(x => x.Id != id))
                    {
                        programsToExclude.Add(program);
                    }
                }
                foreach (var id in genreIdsToExclude)
                {
                    if (program.Genres.Any(x => x.Id == id))
                    {
                        programsToExclude.Add(program);
                    }
                }
                foreach (var name in programNamesToExclude)
                {
                    if (program.ProgramName.Equals(name))
                    {
                        programsToExclude.Add(program);
                    }
                }
            })));

            dto.Markets.ForEach(x => x.Stations.ForEach(y => y.Programs.RemoveAll(z => programsToExclude.Contains(z))));
        }

        public PricingGuideOpenMarketInventoryDto GetPricingGuideOpenMarketInventory(PricingGuideOpenMarketInventoryRequestDto request)
        {
            var proposalRepository = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
            var pricingGuideOpenMarketInventory =
                proposalRepository.GetProposalDetailPricingGuideInventory(request.ProposalDetailId);

            _SetProposalInventoryDetailSpotLength(pricingGuideOpenMarketInventory);

            var programs = _GetPrograms(pricingGuideOpenMarketInventory);

            _FilterProgramsByDaypart(pricingGuideOpenMarketInventory, programs);

            if (!programs.Any())
            {
                return ConvertPricingGuideOpenMarketInventoryDto(pricingGuideOpenMarketInventory);
            }

            _ApplyDaypartNames(programs);
            _ApplyProjectedImpressions(programs, pricingGuideOpenMarketInventory);
            _ApplyStationImpressions(programs, pricingGuideOpenMarketInventory);
            _CalculateProgramCosts(programs, pricingGuideOpenMarketInventory);

            var inventoryMarkets = _GroupProgramsByMarketAndStationForPricingGuide(programs);
            var postingBook = ProposalServiceHelper.GetBookId(pricingGuideOpenMarketInventory);

            _ApplyInventoryMarketRankings(postingBook, inventoryMarkets);
            _ApplyInventoryMarketCoverages(inventoryMarkets);

            pricingGuideOpenMarketInventory.Markets.AddRange(inventoryMarkets.OrderBy(m => m.MarketRank).ToList());
            
            _SumTotalsForMarkets(pricingGuideOpenMarketInventory);

            _ApplyDefaultSortingForPricingGuide(pricingGuideOpenMarketInventory);

            _ApplyProgramAndGenreFilterForPricingGuide(pricingGuideOpenMarketInventory, pricingGuideOpenMarketInventory.Criteria);

            var dto = ConvertPricingGuideOpenMarketInventoryDto(pricingGuideOpenMarketInventory);
            
            _SetProposalOpenMarketPricingGuideGridDisplayFilters(dto);

            return dto;
        }

        private PricingGuideOpenMarketInventoryDto ConvertPricingGuideOpenMarketInventoryDto(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory)
        {
            return new PricingGuideOpenMarketInventoryDto()
            {
                Markets = pricingGuideOpenMarketInventory.Markets
            };
        }

        private void _CalculateProgramCosts(List<ProposalProgramDto> programs,
            PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory)
        {
            _ProposalProgramsCalculationEngine.CalculateBlendedCpmForPrograms(programs,
                pricingGuideOpenMarketInventory.DetailSpotLengthId);
            _ProposalProgramsCalculationEngine.CalculateAvgCostForPrograms(programs);
            _ProposalProgramsCalculationEngine.CalculateTotalCostForPrograms(programs);
            _ProposalProgramsCalculationEngine.CalculateTotalImpressionsForPrograms(programs);
        }

        private void _SumTotalsForMarkets(PricingGuideOpenMarketInventory pricingGuideOpenMarket)
        {
            pricingGuideOpenMarket.Markets.ForEach(m => m.TotalCost = m.Stations.Sum(s => s.Programs.Sum(p => p.Cost)));
            pricingGuideOpenMarket.Markets.ForEach(m => m.TotalSpots = m.Stations.Sum(s => s.Programs.Sum(p => p.Spots)));
            pricingGuideOpenMarket.Markets.ForEach(m => m.TotalImpressions = m.Stations.Sum(s => s.Programs.Sum(p => p.Impressions)));
            pricingGuideOpenMarket.Markets.ForEach(m => m.TotalStationImpressions = m.Stations.Sum(s => s.Programs.Sum(p => p.StationImpressions)));
        }

        private void _SumTotalsForMarkets(PricingGuideOpenMarketInventoryDto pricingGuideOpenMarket)
        {
            pricingGuideOpenMarket.Markets.ForEach(m => m.TotalCost = m.Stations.Sum(s => s.Programs.Sum(p => p.Cost)));
            pricingGuideOpenMarket.Markets.ForEach(m => m.TotalSpots = m.Stations.Sum(s => s.Programs.Sum(p => p.Spots)));
            pricingGuideOpenMarket.Markets.ForEach(m => m.TotalImpressions = m.Stations.Sum(s => s.Programs.Sum(p => p.Impressions)));
            pricingGuideOpenMarket.Markets.ForEach(m => m.TotalStationImpressions = m.Stations.Sum(s => s.Programs.Sum(p => p.StationImpressions)));
        }

        private void _FilterProgramsByDaypart(ProposalDetailInventoryBase pricingGuideOpenMarketDto, List<ProposalProgramDto> programs)
        {
            if (pricingGuideOpenMarketDto.DetailDaypartId == null)
                return;

            var proposalDetailDaypart = DaypartCache.GetDisplayDaypart(pricingGuideOpenMarketDto.DetailDaypartId.Value);

            programs.RemoveAll(p =>
            {
                return
                    p.ManifestDayparts.All(
                        d => !DaypartCache.GetDisplayDaypart(d.DaypartId).Intersects(proposalDetailDaypart));
            });
        }

        private List<ProposalProgramDto> _GetPrograms(ProposalDetailInventoryBase pricingGuideOpenMarketDto)
        {
            var stationProgramRepository =
                BroadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            var proposalMarketIds = ProposalMarketsCalculationEngine
                .GetProposalMarketsList(pricingGuideOpenMarketDto.ProposalId, pricingGuideOpenMarketDto.ProposalVersion,
                    pricingGuideOpenMarketDto.DetailId)
                .Select(m => (short) m.Id).ToList();
            var programs = stationProgramRepository
                .GetStationProgramsForProposalDetail(pricingGuideOpenMarketDto.DetailFlightStartDate,
                    pricingGuideOpenMarketDto.DetailFlightEndDate,
                    pricingGuideOpenMarketDto.DetailSpotLengthId, BroadcastConstants.OpenMarketSourceId, proposalMarketIds,
                    pricingGuideOpenMarketDto.DetailId);

            _SetFlightWeeks(programs);

            return programs;
        }

        public PricingGuideOpenMarketInventoryDto ApplyFilterOnOpenMarketPricingGuideGrid(PricingGuideOpenMarketInventoryDto dto)
        {
            _ApplyFilterForProposalOpenMarketPricingGuideGrid(dto);
            _SumTotalsForMarkets(dto);

            return dto;
        }

        private static void _SetProposalOpenMarketPricingGuideGridDisplayFilters(PricingGuideOpenMarketInventoryDto dto)
        {
            dto.DisplayFilter = new OpenMarketPricingGuideGridDisplayFilterDto();

            var stations = dto.Markets
                    .Where(m => m.Stations != null)
                    .SelectMany(s => s.Stations)
                    .ToList();

            dto.DisplayFilter.ProgramNames = stations
                .SelectMany(s => s.Programs.Where(p => p != null).Select(p => p.ProgramName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToList();
        }

        private static void _ApplyFilterForProposalOpenMarketPricingGuideGrid(PricingGuideOpenMarketInventoryDto dto)
        {
            if (dto.Filter == null)
            {
                return;
            }

            var filter = dto.Filter;

            foreach (var market in dto.Markets)
            {
                foreach (var station in market.Stations)
                {
                    _ApplyProgramNamesFilter(station, filter);
                }
            }
        }

        private static void _ApplyProgramNamesFilter(
            PricingGuideOpenMarketInventory.PricingGuideMarket.PricingGuideStation station,
            OpenMarketPricingGuideGridFilterDto filter)
        {
            var programNames = filter.ProgramNames;

            if (programNames != null && programNames.Any())
            {
                station.Programs = station.Programs.Where(p => programNames.Contains(p.ProgramName)).ToList();
            }
        }
    }
}
