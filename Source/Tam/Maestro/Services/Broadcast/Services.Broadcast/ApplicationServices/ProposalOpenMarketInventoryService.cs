using System;
using System.Collections.Generic;
using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Linq;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.Entities.OpenMarketInventory;
using Tam.Maestro.Common;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProposalOpenMarketInventoryService : IApplicationService
    {
        ProposalDetailOpenMarketInventoryDto GetInventory(int proposalDetailId);
        ProposalDetailOpenMarketInventoryDto RefinePrograms(OpenMarketRefineProgramsRequest request);
        ProposalDetailOpenMarketInventoryDto SaveInventoryAllocations(OpenMarketAllocationSaveRequest request);
        ProposalDetailOpenMarketInventoryDto UpdateOpenMarketInventoryTotals(ProposalDetailOpenMarketInventoryDto proposalInventoryDto);
        ProposalDetailOpenMarketInventoryDto ApplyFilterOnOpenMarketInventory(ProposalDetailOpenMarketInventoryDto proposalInventoryDto);
    }

    public class ProposalOpenMarketInventoryService : BaseProposalInventoryService, IProposalOpenMarketInventoryService
    {
        private readonly IProposalProgramsCalculationEngine _ProposalProgramsCalculationEngine;
        private readonly IProposalOpenMarketsTotalsCalculationEngine _ProposalOpenMarketsTotalsCalculationEngine;
        private readonly IProposalPostingBooksEngine _ProposalPostingBooksEngine;
        internal static readonly string MissingGuaranteedAudienceErorMessage = "Unable to get proprietary inventory information due to null guaranteed audience";

        private const string HOUSEHOLD_AUDIENCE_CODE = "HH";

        public ProposalOpenMarketInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache, IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalProgramsCalculationEngine proposalProgramsCalculationEngine,
            IProposalOpenMarketsTotalsCalculationEngine proposalOpenMarketsTotalsCalculationEngine,
            IProposalPostingBooksEngine proposalPostingBooksEngine, IImpressionAdjustmentEngine impressionAdjustmentEngine,
            IProposalTotalsCalculationEngine proposalTotalsCalculationEngine)
            : base(broadcastDataRepositoryFactory, daypartCache, proposalMarketsCalculationEngine, impressionAdjustmentEngine, proposalTotalsCalculationEngine)
        {
            _ProposalProgramsCalculationEngine = proposalProgramsCalculationEngine;
            _ProposalOpenMarketsTotalsCalculationEngine = proposalOpenMarketsTotalsCalculationEngine;
            _ProposalPostingBooksEngine = proposalPostingBooksEngine;
        }

        public ProposalDetailOpenMarketInventoryDto GetInventory(int proposalDetailId)
        {
            return _GetProposalDetailOpenMarketInventoryDto(proposalDetailId, null);
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
            dto.DisplayFilter.Genres = stations.Where(p => p.Programs.Any())
                .SelectMany(p => p.Programs.Where(g => g != null && g.Genres.Any()).SelectMany(z => z.Genres))
                .GroupBy(g => new { g.Id, g.Display })
                .Distinct()
                .Select(a => a.First())
                .OrderBy(b => b.Display)
                .ToList();
            dto.DisplayFilter.Markets = dto.Markets
                .Select(a => new { a.MarketId, a.MarketName })
                .Distinct()
                .Select(y => new LookupDto { Id = y.MarketId, Display = y.MarketName })
                .OrderBy(n => n.Display)
                .ToList();
            dto.DisplayFilter.ProgramNames = stations.Where(p => p.Programs.Any())
                .SelectMany(p => p.Programs.Where(l => l != null).Select(z => z.ProgramName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(m => m)
                .ToList();
        }

        public ProposalDetailOpenMarketInventoryDto RefinePrograms(OpenMarketRefineProgramsRequest request)
        {
            if (request.Criteria.CpmCriteria.GroupBy(c => c.MinMax).Any(g => g.Count() > 1))
            {
                throw new ApplicationException("Only 1 Min CPM and 1 Max CPM criteria allowed.");
            }

            var dto = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>().GetOpenMarketProposalDetailInventory(request.ProposalDetailId);
            UpdateCriteria(dto, request.Criteria);
            _PopulateMarkets(dto, request.IgnoreExistingAllocation);
            _PopulateInventoryWeeks(dto);
            _SetProposalOpenMarketDisplayFilters(dto);
            _CalculateOpenMarketTotals(dto);
            return dto;
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

            // to be used to filter programs with/without pots
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
                                                    d =>
                                                        DisplayDaypart.Intersects(DaypartDto.ConvertDaypartDto(d),
                                                            DaypartCache.GetDisplayDaypart(a.Daypart.Id))))
                                        // filter program names
                                        .Where(
                                            p =>
                                                filter.ProgramNames == null || !filter.ProgramNames.Any() ||
                                                filter.ProgramNames.Any(
                                                    c =>
                                                        string.Compare(p.ProgramName, c,
                                                            StringComparison.OrdinalIgnoreCase) == 0))
                                        // filter genres
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
                                            ProgramName = pro.ProgramName,
                                            Daypart = pro.Daypart,
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
            _SetProposalInventoryDetailSpotLength(dto);
            _SetProposalInventoryDetailDaypart(dto);

            var proposalMarketIds =
                ProposalMarketsCalculationEngine.GetProposalMarketsList(dto.ProposalId, dto.ProposalVersion,
                    dto.DetailId).Select(m => m.Id).ToList();

            var programs = BroadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>()
                .GetStationProgramsForProposalDetail(dto.DetailFlightStartDate.Value, dto.DetailFlightEndDate.Value,
                    dto.DetailSpotLength, (int)RatesFile.RateSourceType.OpenMarket, proposalMarketIds, dto.DetailId);

            // represents the actual program names before any refine is applied
            dto.RefineFilterPrograms = programs.Where(l => l != null).Select(z => z.ProgramName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(m => m)
                .ToList();

            var proposalDetailDaypart = DaypartCache.GetDisplayDaypart(dto.DetailDaypartId.Value);
            var filteredProgramsWithAllocations = new List<int>();
            programs.RemoveAll(p =>
            {
                if (!DaypartCache.GetDisplayDaypart(p.DayPartId).Intersects(proposalDetailDaypart))
                    return true;

                if (FilterByGenreAndProgramNameCriteria(p, dto.Criteria))
                {
                    if (p.FlightWeeks.Any(fw => fw.Allocations.Any(a => a.Spots > 0)))
                    {
                        if (!ignoreExistingAllocation)
                        {
                            dto.NewCriteriaAffectsExistingAllocations = true;

                            return false;
                        }

                        filteredProgramsWithAllocations.Add(p.ProgramId);
                    }

                    return true;
                }

                return false;
            });

            if (filteredProgramsWithAllocations.Any())
                BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>()
                    .RemoveAllocations(filteredProgramsWithAllocations, dto.DetailId);

            if (programs.Count < 1)
            {
                return;
            }

            _ApplyDaypartNames(programs);
            _ApplyProgramImpressions(programs, dto);
            _ProposalProgramsCalculationEngine.ApplyBlendedCpmForEachProgram(programs, dto.DetailSpotLength);

            programs.RemoveAll(p => FilterByCpmCriteria(p, dto.Criteria.CpmCriteria));

            var inventoryMarkets = _GroupProgramsByMarketAndStation(programs);

            var postingBook = _ProposalPostingBooksEngine.GetPostingBookId(dto);
            _ApplyInventoryMarketSubscribers(postingBook, inventoryMarkets);
            _ApplyInventoryMarketRankings(postingBook, inventoryMarkets);

            dto.Markets.AddRange(inventoryMarkets.OrderBy(m => m.MarketRank).ToList());

            _ApplyDefaultSorting(dto);
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
                .Select(g => new { Type = g.Key, GenreIds = g.Select(gb => gb.GenreId) }))
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
                .Select(g => new { Type = g.Key, ProgramNames = g.Select(gb => gb.ProgramName) }))
            {
                var includeProgramName = criteria.ProgramNames.Any(c => string.Equals(program.ProgramName, c, StringComparison.CurrentCultureIgnoreCase));
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
                            var weekProgram = new ProposalOpenMarketInventoryWeekDto.InventoryWeekProgram();
                            var existingAllocation =
                                existingAllocations.SingleOrDefault(
                                    a => a.MediaWeekId == week.MediaWeekId && a.StationProgramId == program.ProgramId);
                            weekProgram.ProgramId = program.ProgramId;
                            weekProgram.Spots = existingAllocation == null ? 0 : existingAllocation.Spots;
                            weekProgram.UnitImpression = program.UnitImpressions;
                            weekProgram.UnitCost = programFlightweek.Rate;
                            weekProgram.TargetImpressions = program.TargetImpressions;
                            weekProgram.TotalImpressions = weekProgram.Spots > 0 ? program.UnitImpressions * weekProgram.Spots : program.UnitImpressions;
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

        private void _ApplyProgramImpressions(IEnumerable<ProposalProgramDto> programs,
            ProposalDetailInventoryBase proposalDetail)
        {
            var impressionRequests = new List<StationDetailDaypart>();
            var stationDetailImpressions = new Dictionary<int, ProposalProgramDto>();

            var proposalDetailDaypart = DaypartDto.ConvertDaypartDto(proposalDetail.DetailDaypart);
            foreach (var program in programs)
            {
                var intersectingDaypart =
                    DisplayDaypart.Intersect(
                        Common.Services.DaypartCache.Instance.GetDisplayDaypart(program.DayPart.Id),
                        proposalDetailDaypart);
                var stationDaypart = new StationDetailDaypart
                {
                    Code = program.Station.StationCode,
                    Id = program.ProgramId,
                    DisplayDaypart = intersectingDaypart
                };
                impressionRequests.Add(stationDaypart);
                stationDetailImpressions[program.ProgramId] = program;
            }

            var ratingAudiences = BroadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>()
                .GetRatingsAudiencesByMaestroAudience(
                    new List<int>
                    {
                        proposalDetail.GuaranteedAudience.Value
                    }).Select(r => r.rating_audience_id).Distinct().ToList();

            foreach (var imp in GetImpressions(proposalDetail, ratingAudiences, impressionRequests))
            {
                stationDetailImpressions[imp.id].UnitImpressions += imp.impressions;
            }
        }

        private void _ApplyDaypartNames(List<ProposalProgramDto> programs)
        {
            var programDaypartIds = programs.Select(p => p.DayPartId).Distinct().ToList();
            var programDayparts = DaypartCache.GetDisplayDayparts(programDaypartIds);

            foreach (var program in programs)
            {
                program.DayPart = new LookupDto(program.DayPartId, programDayparts[program.DayPartId].ToString());
            }
        }

        private void _ApplyInventoryMarketRankings(int mediaMonthId, IEnumerable<ProposalInventoryMarketDto> inventoryMarkets)
        {
            var marketRankings =
                BroadcastDataRepositoryFactory.GetDataRepository<INsiMarketRepository>()
                    .GetMarketRankingsByMediaMonth(mediaMonthId);

            foreach (var inventoryMarket in inventoryMarkets)
            {
                int rank;
                marketRankings.TryGetValue(inventoryMarket.MarketId, out rank);
                inventoryMarket.MarketRank = rank;
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
                            ProgramId = p.ProgramId,
                            ProgramName = p.ProgramName,
                            TargetCpm = p.TargetCpm,
                            UnitImpressions = p.UnitImpressions,
                            TargetImpressions = p.TargetImpressions,
                            Daypart = p.DayPart,
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
                double subscribers;
                marketSubscribers.TryGetValue((short)inventoryMarket.MarketId, out subscribers);
                inventoryMarket.MarketSubscribers = subscribers;
            }
        }

        public ProposalDetailOpenMarketInventoryDto SaveInventoryAllocations(OpenMarketAllocationSaveRequest request)
        {
            var openMarketInventoryRepository = BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>();
            var existingAllocations =
                openMarketInventoryRepository.GetProposalDetailAllocations(request.ProposalVersionDetailId);
            var allocationToRemove = _GetAllocationsToRemove(request, existingAllocations);
            var allocationsToUpdate = _GetAllocationsToUpdate(request, existingAllocations);
            var allocationToAdd = _GetAllocationsToCreate(request, existingAllocations);

            using (var transaction = new TransactionScopeWrapper())
            {
                openMarketInventoryRepository.RemoveAllocations(allocationToRemove);

                openMarketInventoryRepository.UpdateAllocations(allocationsToUpdate, request.Username);

                openMarketInventoryRepository.AddAllocations(allocationToAdd, request.Username);

                var inventoryDto = _GetProposalDetailOpenMarketInventoryDto(request.ProposalVersionDetailId, null);

                _ProposalOpenMarketsTotalsCalculationEngine.CalculatePartialOpenMarketTotals(inventoryDto);

                var proposalDetailTotals = _GetProposalDetailTotals(inventoryDto);

                BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                    .SaveProposalDetailOpenMarketInventoryTotals(inventoryDto.DetailId, proposalDetailTotals);

                BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                    .SaveProposalDetailOpenMarketWeekInventoryTotals(inventoryDto);

                _UpdateProposalTotals(inventoryDto.ProposalVersionId);

                _CalculateOpenMarketTotals(inventoryDto);

                transaction.Complete();

                return _GetProposalDetailOpenMarketInventoryDto(request.ProposalVersionDetailId, request.Filter);
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

        private ProposalDetailOpenMarketInventoryDto _GetProposalDetailOpenMarketInventoryDto(int proposalInventoryDetailId, ProposalOpenMarketFilter openMarketFilter)
        {
            var dto = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>().GetOpenMarketProposalDetailInventory(proposalInventoryDetailId);
            _PopulateMarkets(dto, true);
            _PopulateInventoryWeeks(dto);
            _SetProposalOpenMarketDisplayFilters(dto);
            if (openMarketFilter != null)
                dto.Filter = openMarketFilter;
            _ApplyProposalOpenMarketFilter(dto, true);
            _CalculateOpenMarketTotals(dto);
            return dto;
        }

        private static List<OpenMarketInventoryAllocation> _GetAllocationsToCreate(OpenMarketAllocationSaveRequest request, List<OpenMarketInventoryAllocation> existingAllocations)
        {
            var requestedAllocations =
                request.Weeks.SelectMany(
                    w => w.Programs.Select(
                        p => new OpenMarketInventoryAllocation
                        {
                            ProposalVersionDetailId = request.ProposalVersionDetailId,
                            MediaWeekId = w.MediaWeekId,
                            StationProgramId = p.ProgramId,
                            Spots = p.Spots,
                            Impressions = p.Impressions,
                            SpotCost = p.SpotCost
                        })).ToList();

            var newAllocations =
                requestedAllocations.Where(
                    ra =>
                        existingAllocations.All(
                            ea => ea.MediaWeekId != ra.MediaWeekId || ea.StationProgramId != ra.StationProgramId)).ToList();

            return newAllocations;
        }

        private static List<OpenMarketInventoryAllocation> _GetAllocationsToUpdate(OpenMarketAllocationSaveRequest request, List<OpenMarketInventoryAllocation> existingAllocations)
        {
            var requestedAllocations =
                request.Weeks.SelectMany(
                    w => w.Programs.Select(
                        p => new OpenMarketInventoryAllocation
                        {
                            ProposalVersionDetailId = request.ProposalVersionDetailId,
                            MediaWeekId = w.MediaWeekId,
                            StationProgramId = p.ProgramId,
                            Spots = p.Spots
                        })).Where(p => p.Spots > 0).ToList();

            var updateAllocations = new List<OpenMarketInventoryAllocation>();
            foreach (var requestedAllocation in requestedAllocations)
            {
                var existingAllocation = existingAllocations.SingleOrDefault(ea => ea.MediaWeekId == requestedAllocation.MediaWeekId && ea.StationProgramId == requestedAllocation.StationProgramId);
                if (existingAllocation != null)
                {
                    requestedAllocation.ProposalVersionDetailQuarterWeekId = existingAllocation.ProposalVersionDetailQuarterWeekId;
                    requestedAllocation.StationProgramFlightId = existingAllocation.StationProgramFlightId;
                    updateAllocations.Add(requestedAllocation);
                }
            }

            return updateAllocations;
        }

        private static List<OpenMarketInventoryAllocation> _GetAllocationsToRemove(OpenMarketAllocationSaveRequest request, IEnumerable<OpenMarketInventoryAllocation> existingAllocations)
        {
            var programFlightsWithZeroSpots = new List<Tuple<int, int>>();
            foreach (var week in request.Weeks)
            {
                foreach (var program in week.Programs)
                {
                    if (program.Spots == 0) //nothing allocated
                    {
                        var flight = new Tuple<int, int>(week.MediaWeekId, program.ProgramId);
                        programFlightsWithZeroSpots.Add(flight);
                    }
                }
            }

            var allocations = (from a in existingAllocations
                               join f in programFlightsWithZeroSpots
                               on new
                               {
                                   a.MediaWeekId,
                                   a.StationProgramId
                               } equals new
                               {
                                   MediaWeekId = f.Item1,
                                   StationProgramId = f.Item2
                               }
                               select a).ToList();

            return allocations;
        }
    }
}
