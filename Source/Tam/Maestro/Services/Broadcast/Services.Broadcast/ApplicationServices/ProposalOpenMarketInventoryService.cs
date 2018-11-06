using System;
using System.Collections.Generic;
using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Linq;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.DTO;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Extensions;
using Tam.Maestro.Common;
using Services.Broadcast.Entities.DTO.PricingGuide;
using static Services.Broadcast.Entities.DTO.PricingGuide.PricingGuideOpenMarketDistributionDto;

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
        PricingGuideOpenMarketDistributionDto GetPricingGuideOpenMarketInventory(PricingGuideOpenMarketInventoryRequestDto request);
        void SavePricingGuideOpenMarketInventory(int proposalDetailId, PricingGuideOpenMarketDistributionDto pricingGuide);
        void DeleteExistingGeneratedPricingGuide(int proposalDetailId);
        PricingGuideOpenMarketDistributionDto ApplyFilterOnOpenMarketPricingGuideGrid(PricingGuideOpenMarketDistributionDto dto);
        PricingGuideOpenMarketDistributionDto SavePricingGuideAllocations(PricingGuideAllocationSaveRequestDto request);
        PricingGuideOpenMarketDistributionDto UpdateOpenMarketPricingGuideMarkets(PricingGuideOpenMarketDistributionDto dto);
    }

    public class ProposalOpenMarketInventoryService : BaseProposalInventoryService, IProposalOpenMarketInventoryService
    {
        private readonly IProposalProgramsCalculationEngine _ProposalProgramsCalculationEngine;
        private readonly IProposalOpenMarketsTotalsCalculationEngine _ProposalOpenMarketsTotalsCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IProposalRepository _ProposalRepository;
        private readonly IProposalOpenMarketInventoryRepository _ProposalOpenMarketInventoryRepository;
        private readonly IPricingGuideDistributionEngine _PricingGuideDistributionEngine;

        private const string HOUSEHOLD_AUDIENCE_CODE = "HH";

        public ProposalOpenMarketInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache, IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalProgramsCalculationEngine proposalProgramsCalculationEngine,
            IProposalOpenMarketsTotalsCalculationEngine proposalOpenMarketsTotalsCalculationEngine,
            IImpressionAdjustmentEngine impressionAdjustmentEngine,
            IProposalTotalsCalculationEngine proposalTotalsCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IPricingGuideDistributionEngine pricingGuideDistributionEngine)
            : base(broadcastDataRepositoryFactory, daypartCache, proposalMarketsCalculationEngine,
                impressionAdjustmentEngine, proposalTotalsCalculationEngine)
        {
            _ProposalProgramsCalculationEngine = proposalProgramsCalculationEngine;
            _ProposalOpenMarketsTotalsCalculationEngine = proposalOpenMarketsTotalsCalculationEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _ProposalRepository = broadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
            _ProposalOpenMarketInventoryRepository = broadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>();
            _PricingGuideDistributionEngine = pricingGuideDistributionEngine;
        }

        public ProposalDetailOpenMarketInventoryDto GetInventory(int proposalDetailId,
            ProposalOpenMarketFilter openMarketFilter = null)
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

        public ProposalDetailOpenMarketInventoryDto RefinePrograms(OpenMarketRefineProgramsRequest request,
            ProposalOpenMarketFilter openMarketFilter)
        {
            if (request.Criteria.CpmCriteria.GroupBy(c => c.MinMax).Any(g => g.Count() > 1))
            {
                throw new ApplicationException("Only 1 Min CPM and 1 Max CPM criteria allowed.");
            }

            var dto = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                .GetOpenMarketProposalDetailInventory(request.ProposalDetailId);
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
            List<ProposalInventoryMarketDto.InventoryMarketStationProgram> programsToExclude =
                new List<ProposalInventoryMarketDto.InventoryMarketStationProgram>();
            var programNamesToExclude = criteria.ProgramNameSearchCriteria
                .Where(x => x.Contain == ContainTypeEnum.Exclude).Select(x => x.Program.Display).ToList();
            var genreIdsToInclude = criteria.GenreSearchCriteria.Where(x => x.Contain == ContainTypeEnum.Include)
                .Select(x => x.Genre.Id).ToList();
            var genreIdsToExclude = criteria.GenreSearchCriteria.Where(x => x.Contain == ContainTypeEnum.Exclude)
                .Select(x => x.Genre.Id).ToList();
            dto.Markets.ForEach(market => market.Stations.ForEach(station => station.Programs.ForEach(program =>
            {
                if (genreIdsToInclude.Any() && program.Genres.All(g => !genreIdsToInclude.Contains(g.Id)))
                {
                    programsToExclude.Add(program);
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
                    if (program.ProgramNames.Any(x => name.Equals(x, StringComparison.InvariantCultureIgnoreCase)))
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
                            a.Markets.SelectMany(b => b.Stations.SelectMany(c => c.Programs.Where(d => d != null))))
                    .ToList();

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
                                    filter.Affiliations == null || !filter.Affiliations.Any() ||
                                    filter.Affiliations.Any(
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
                                                    filtersDaypart => a.Dayparts.Any(dp =>
                                                        DisplayDaypart.Intersects(
                                                            DaypartDto.ConvertDaypartDto(filtersDaypart),
                                                            DaypartCache.GetDisplayDaypart(dp.Id)))))
                                        // filter program names
                                        .Where(
                                            p =>
                                                filter.ProgramNames == null || !filter.ProgramNames.Any() ||
                                                filter.ProgramNames.Any(
                                                    filterProgramName =>
                                                        p.ProgramNames.Any(manifestProgramName =>
                                                            string.Compare(manifestProgramName, filterProgramName,
                                                                StringComparison.OrdinalIgnoreCase) ==
                                                            0))) // filter genres
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
                                var weekProgram =
                                    weekPrograms.SingleOrDefault(a => a != null && a.ProgramId == program.ProgramId);
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

            var proposalMarketIds = ProposalMarketsCalculationEngine.GetProposalMarketsList(dto.ProposalId, dto.ProposalVersion)
                .Select(m => m.Id).ToList();
            var programs = BroadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>()
                .GetStationProgramsForProposalDetail(dto.DetailFlightStartDate, dto.DetailFlightEndDate,
                    dto.DetailSpotLengthId, BroadcastConstants.OpenMarketSourceId, proposalMarketIds);

            _SetFlightWeeks(programs);

            //// represents the actual program names before any refine is applied
            dto.RefineFilterPrograms = programs.Where(l => l != null)
                .SelectMany(z => z.ManifestDayparts.Select(md => md.ProgramName))
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
                var manifestAudienceForProposal =
                    program.ManifestAudiences.SingleOrDefault(x => x.AudienceId == proposalDetail.GuaranteedAudience);
                var hasManifestAudiences = manifestAudienceForProposal != null &&
                                           manifestAudienceForProposal.Impressions.HasValue;

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

            var displayFlighWeeks =
                _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(programDto.StartDate, nonNullableEndDate);

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

        internal static bool FilterByGenreAndProgramNameCriteria(ProposalProgramDto program,
            OpenMarketCriterion marketCriterion)
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
                var includeProgramName = criteria.ProgramNames.Any(c =>
                    program.ManifestDayparts.Select(md => md.ProgramName).Any(pn =>
                        string.Equals(pn, c, StringComparison.CurrentCultureIgnoreCase)));
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
                            if (weekProgram.ProvidedUnitImpressions.HasValue)
                            {
                                weekProgram.TotalImpressions = weekProgram.Spots == 0 ? weekProgram.ProvidedUnitImpressions.Value : weekProgram.Spots * weekProgram.ProvidedUnitImpressions.Value;
                            }
                            else
                            {
                                weekProgram.TotalImpressions = weekProgram.Spots == 0 ? weekProgram.UnitImpression : weekProgram.Spots * weekProgram.UnitImpression;
                            }

                            weekProgram.HasImpressions = weekProgram.UnitImpression > 0 ||
                                (weekProgram.ProvidedUnitImpressions.HasValue && weekProgram.ProvidedUnitImpressions.Value > 0);

                            weekProgram.Cost = weekProgram.Spots > 0
                                ? weekProgram.Spots * weekProgram.UnitCost
                                : weekProgram.UnitCost;

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
            var deleteCpmCriterion = dto.Criteria.CpmCriteria
                .Where(oc => oc.Id.HasValue && !newCriterion.CpmCriteria.Select(c => c.Id).Contains(oc.Id))
                .Select(oc => oc.Id.Value).ToList();

            var newGenreCriteria = newCriterion.GenreSearchCriteria.Where(c => !c.Id.HasValue).ToList();
            var deleteGenreCriteria = dto.Criteria.GenreSearchCriteria
                .Where(oc => oc.Id.HasValue && !newCriterion.GenreSearchCriteria.Select(c => c.Id).Contains(oc.Id))
                .Select(oc => oc.Id.Value).ToList();

            var newProgramNameCriteria = newCriterion.ProgramNameSearchCriteria.Where(c => !c.Id.HasValue).ToList();
            var oldProgramNameCriteria = dto.Criteria.ProgramNameSearchCriteria
                .Where(oc =>
                    oc.Id.HasValue && !newCriterion.ProgramNameSearchCriteria.Select(c => c.Id).Contains(oc.Id))
                .Select(oc => oc.Id.Value).ToList();

            var criteriaRepository =
                BroadcastDataRepositoryFactory.GetDataRepository<IProposalProgramsCriteriaRepository>();
            var criteria = criteriaRepository.UpdateCriteria(dto.DetailId, newCpmCriterion, deleteCpmCriterion,
                newGenreCriteria, deleteGenreCriteria, newProgramNameCriteria, oldProgramNameCriteria);

            dto.Criteria = criteria;
        }

        private static void _ApplyDefaultSorting(ProposalDetailOpenMarketInventoryDto proposalInventory)
        {
            var sortedMarkets = proposalInventory.Markets.OrderBy(m => m.MarketRank);
            foreach (var market in sortedMarkets)
            {
                market.Stations = market.Stations.OrderBy(s => s.Programs.Min(p => p.TargetCpm))
                    .ThenBy(s => s.LegacyCallLetters).ToList();
                foreach (var station in market.Stations)
                {
                    station.Programs = station.Programs.OrderBy(p => p.TargetCpm).ToList();
                }
            }

            proposalInventory.Markets = sortedMarkets.ToList();
        }

        private void _ApplyProjectedImpressions(IEnumerable<ProposalProgramDto> programs, ProposalDetailInventoryBase proposalDetail)
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
            var programDaypartIds = programs.SelectMany(p => p.ManifestDayparts.Select(md => md.DaypartId)).Distinct()
                .ToList();
            var programDayparts = DaypartCache.GetDisplayDayparts(programDaypartIds);

            foreach (var program in programs)
            {
                program.DayParts =
                    program.ManifestDayparts.Select(md => md.DaypartId).Select(daypartId =>
                            new LookupDto(daypartId, programDayparts[daypartId].ToString()))
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

            var marketCoverages = BroadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>().GetMarketCoverages(marketsList.Select(x => x.MarketId));

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
                    Stations = g.GroupBy(p => p.Station.StationCode).Select(s =>
                        new ProposalInventoryMarketDto.InventoryMarketStation
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
            var householdAudienceId = BroadcastDataRepositoryFactory.GetDataRepository<IAudienceRepository>()
                .GetDisplayAudienceByCode(HOUSEHOLD_AUDIENCE_CODE).Id;
            var marketSubscribers = BroadcastDataRepositoryFactory.GetDataRepository<INsiUniverseRepository>()
                .GetUniverseDataByAudience(monthId, new List<int> { householdAudienceId });

            foreach (var inventoryMarket in inventoryMarkets)
            {
                marketSubscribers.TryGetValue((short)inventoryMarket.MarketId, out double subscribers);
                inventoryMarket.MarketSubscribers = subscribers;
            }
        }

        public List<OpenMarketInventoryAllocation> GetProposalInventoryAllocations(int proposalVersionDetailId)
        {
            var openMarketInventoryRepository =
                BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>();
            return openMarketInventoryRepository.GetProposalDetailAllocations(proposalVersionDetailId);
        }

        public ProposalDetailOpenMarketInventoryDto SaveInventoryAllocations(OpenMarketAllocationSaveRequest request)
        {
            var proposalRepository = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
            var openMarketInventoryRepository =
                BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>();
            var existingAllocations =
                openMarketInventoryRepository.GetProposalDetailAllocations(request.ProposalVersionDetailId);
            var guaranteedAudienceId =
                proposalRepository.GetProposalDetailGuaranteedAudienceId(request.ProposalVersionDetailId);
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

        private List<PricingGuideMarketDto> _GroupProgramsByMarketAndStationForPricingGuide(IEnumerable<ProposalProgramDto> programs)
        {
            var inventoryMarkets = programs.GroupBy(p => p.Market.Id).Select(
                g => new PricingGuideMarketDto
                {
                    MarketId = g.Key,
                    MarketName = g.First().Market.Display,
                    Stations = g.GroupBy(p => p.Station.StationCode).Select(s =>
                        new PricingGuideStationDto
                        {
                            Affiliation = s.First().Station.Affiliation,
                            CallLetters = s.First().Station.CallLetters,
                            LegacyCallLetters = s.First().Station.LegacyCallLetters,
                            StationCode = s.First().Station.StationCode,
                            Programs = _GroupProgramsByDaypart(s)
                        }).ToList()
                }).ToList();

            return inventoryMarkets;
        }

        private List<PricingGuideProgramDto> _GroupProgramsByDaypart(IGrouping<short, ProposalProgramDto> programs)
        {
            return programs.Where(x => x.DayParts.Any()).GroupBy(s => s.DayParts.Single().Id)
                .Select(p => new PricingGuideProgramDto
                {
                    ProgramId = p.First().ManifestId,
                    ProgramName = string.Join("|", p.Select(x => x.ManifestDayparts.First().ProgramName).Distinct()),
                    BlendedCpm = p.Average(x => x.TargetCpm),
                    ImpressionsPerSpot = p.Average(x => x.UnitImpressions),
                    StationImpressionsPerSpot = p.Average(x => x.ProvidedUnitImpressions ?? 0),
                    Daypart = p.First().DayParts.Single(),
                    CostPerSpot = p.Average(x => x.SpotCost),
                    Cost = p.Average(x => x.TotalCost),
                    Impressions = p.Average(x => x.TotalImpressions),
                    ManifestDaypartId = p.First().ManifestDayparts.Single().Id,
                    Spots = p.Sum(x => x.TotalSpots),
                    Genres = p.SelectMany(x => x.Genres).ToList()
                }).ToList();
        }

        private static List<PricingGuideMarketDto> _ApplyDefaultSortingForPricingGuideMarkets(List<PricingGuideMarketDto> markets)
        {
            var sortedMarkets = markets.OrderBy(m => m.MarketRank).ToList();

            foreach (var market in sortedMarkets)
            {
                market.Stations = market.Stations
                    .OrderBy(s => s.MinProgramsBlendedCpm)
                    .ThenBy(s => s.LegacyCallLetters)
                    .ToList();

                foreach (var station in market.Stations)
                {
                    station.Programs = station.Programs.OrderBy(p => p.BlendedCpm).ToList();
                }
            }

            return sortedMarkets;
        }

        private void _ApplyProgramAndGenreFilterForMarkets(List<PricingGuideMarketDto> markets, OpenMarketCriterion criterion)
        {
            var programsToExclude = new List<PricingGuideProgramDto>();
            var genreIdsToInclude = criterion.GenreSearchCriteria
                .Where(x => x.Contain == ContainTypeEnum.Include)
                .Select(x => x.Genre.Id)
                .ToList();
            var genreIdsToExclude = criterion.GenreSearchCriteria
                .Where(x => x.Contain == ContainTypeEnum.Exclude)
                .Select(x => x.Genre.Id)
                .ToList();
            var programNamesToExclude = criterion.ProgramNameSearchCriteria
                .Where(x => x.Contain == ContainTypeEnum.Exclude)
                .Select(x => x.Program.Display)
                .ToList();

            markets.ForEach(market => market.Stations.ForEach(station => station.Programs.ForEach(program =>
            {
                var hasIncludedGenre = false || genreIdsToInclude.IsEmpty();

                foreach (var genre in program.Genres)
                {
                    if (genreIdsToInclude.Contains(genre.Id))
                    {
                        hasIncludedGenre = true;
                        break;
                    }
                }

                if (!hasIncludedGenre)
                    programsToExclude.Add(program);

                foreach (var id in genreIdsToExclude)
                {
                    if (program.Genres.Any(x => x.Id == id))
                    {
                        programsToExclude.Add(program);
                    }
                }

                foreach (var name in programNamesToExclude)
                {
                    var programNames = program.ProgramName.Split('|');

                    if (programNames.Any(x => x.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        programsToExclude.Add(program);
                    }
                }
            })));

            markets.ForEach(x => x.Stations.ForEach(y => y.Programs.RemoveAll(z => programsToExclude.Contains(z))));
        }

        public static PricingGuideOpenMarketInventory MapToPricingGuideOpenMarketInventory(open_market_pricing_guide pricingGuide)
        {
            if (pricingGuide == null)
                return null;

            var pricingGuideInventory = new PricingGuideOpenMarketInventory();



            return pricingGuideInventory;
        }

        private List<open_market_pricing_guide> _GetProposalDetailPricingGuide(int proposalId, int proposalDetailId)
        {
            var repository = BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>();
            var guide = repository.GetProposalDetailPricingGuide(proposalDetailId);

            return guide?.Count() == 0 ? null : guide;
        }

        public void SavePricingGuideOpenMarketInventory(int proposalDetailId,PricingGuideOpenMarketDistributionDto pricingGuide)
        {
            var repository = BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>();

            // adjust necessary cost/impressions
            pricingGuide.Markets.ForEach(m =>
            {
                m.Stations.ForEach(s => s.Programs.ForEach(p =>
                {
                    p.Cost = _ProposalProgramsCalculationEngine.CalculateSpotCost(p.Spots, p.CostPerSpot);
                    p.Impressions = _ProposalProgramsCalculationEngine.CalculateSpotImpressions(p.Spots, p.EffectiveImpressionsPerSpot);
                }));
            });

            repository.SaveProposalDetailPricingGuide(proposalDetailId, pricingGuide.Markets);
        }

        public void DeleteExistingGeneratedPricingGuide(int proposalDetailId)
        {
            var repository = BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>();
            repository.DeleteProposalDetailPricingGuide(proposalDetailId);
        }

        public PricingGuideOpenMarketDistributionDto GetPricingGuideOpenMarketInventory(PricingGuideOpenMarketInventoryRequestDto request)
        {
            PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory = null;
            var existingPricingGuide = _GetProposalDetailPricingGuide(request.ProposalId, request.ProposalDetailId);
            var generatePricingGuide = request.HasPricingGuideChanges ?? true;
            var detail = _ProposalRepository.GetProposalDetail(request.ProposalDetailId);

            if (existingPricingGuide == null || generatePricingGuide)
            {
                pricingGuideOpenMarketInventory = _GetPricingGuideInventory(request.ProposalDetailId);
                _SetPricingGuideMarketsFilteredByIds(pricingGuideOpenMarketInventory, null, request, true);
            }
            else
            {
                var proposalDetail = _ProposalRepository.GetProposalDetail(request.ProposalDetailId);
                var postingBookId = ProposalServiceHelper.GetBookId(proposalDetail);

                pricingGuideOpenMarketInventory = _GetExistingPricingGuideOpenMarketInventory(existingPricingGuide, postingBookId);
            }

            var pricingGuideDto = _GetPricingGuideOpenMarketInventoryDto(request, pricingGuideOpenMarketInventory, detail.PricingGuide.ProprietaryPricing);

            // since the pricing guide was never save, lets save it now
            if (existingPricingGuide == null)
            {
                SavePricingGuideOpenMarketInventory(request.ProposalDetailId, pricingGuideDto);
            }

            return pricingGuideDto;
        }

        private void _SumTotalsForPricingGuide(PricingGuideOpenMarketDistributionDto pricingGuideDto, List<ProprietaryPricingDto> proprietaryPricingValues)
        {
            pricingGuideDto.OpenMarketTotals = _SumTotalsForOpenMarketSection(pricingGuideDto.Markets);
            pricingGuideDto.ProprietaryTotals = _SumTotalsForProprietarySection(pricingGuideDto.OpenMarketTotals.Impressions, proprietaryPricingValues);
            pricingGuideDto.PricingTotals = _SumPricingTotals(pricingGuideDto.OpenMarketTotals, pricingGuideDto.ProprietaryTotals);
        }

        private PricingTotalsDto _SumPricingTotals(
            OpenMarketTotalsDto openMarketTotals,
            ProprietaryTotalsDto proprietaryTotals)
        {
            var result = new PricingTotalsDto
            {
                Impressions = openMarketTotals.Impressions + proprietaryTotals.Impressions,
                Cost = openMarketTotals.Cost + proprietaryTotals.Cost,
                Coverage = openMarketTotals.Coverage
            };

            if (result.Impressions > 0)
            {
                result.Cpm = result.Cost / (decimal)result.Impressions * 1000;
            }

            return result;
        }

        private ProprietaryTotalsDto _SumTotalsForProprietarySection(double openMarketImpressions, List<ProprietaryPricingDto> proprietaryPricingValues)
        {
            var proprietaryBalance = proprietaryPricingValues.Sum(x => x.ImpressionsBalance) * 100;
            var result = new ProprietaryTotalsDto();

            if (openMarketImpressions == 0 || proprietaryBalance == 0)
            {
                return result;
            }

            var openMarketBalance = 100 - proprietaryBalance;
            var impressionsPerOnePercentage = openMarketImpressions / openMarketBalance;
            result.Impressions = proprietaryBalance * impressionsPerOnePercentage;
            result.Cpm = (decimal)(proprietaryPricingValues.Sum(x => (x.ImpressionsBalance * 100) * (double)x.Cpm) / proprietaryBalance);
            result.Cost = (decimal)(result.Impressions / 1000) * result.Cpm;

            return result;
        }

        private OpenMarketTotalsDto _SumTotalsForOpenMarketSection(List<PricingGuideMarketDto> markets)
        {
            List<PricingGuideMarketDto> marketsWithSpots = new List<PricingGuideMarketDto>();           
            markets.ForEach(x =>
            {
                if( x.Stations.Any(y => y.Programs.Where(z => z.Spots != 0).Any()))
                {
                    marketsWithSpots.Add(x);
                }
            });

            var result = new OpenMarketTotalsDto
            {
                Cost = markets.Sum(x => x.TotalCost),
                Coverage = marketsWithSpots.Sum(x => x.MarketCoverage),
                Impressions = markets.Sum(x => x.TotalImpressions)
            };
            result.Cpm = result.Impressions == 0 ? 0 : result.Cost / (decimal)result.Impressions * 1000;

            return result;
        }

        private PricingGuideOpenMarketInventory _GetExistingPricingGuideOpenMarketInventory(List<open_market_pricing_guide> existingPricingGuide, int postingBookId)
        {
            var response = new PricingGuideOpenMarketInventory();

            var existingMarketPriceGuides = existingPricingGuide.GroupBy(pg => new { pg.market });

            var marketRankings =
                BroadcastDataRepositoryFactory.GetDataRepository<INsiMarketRepository>()
                    .GetMarketRankingsByMediaMonth(postingBookId);

            foreach (var marketPricingGuide in existingMarketPriceGuides)
            {
                var market = marketPricingGuide.Key.market;
                var pricingGuideMarket = new PricingGuideMarketDto()
                {
                    MarketId = market.market_code,
                    MarketCoverage = market.market_coverages.First().percentage_of_us,
                    MarketRank = marketRankings[market.market_code],
                    MarketName = market.geography_name,
                    Stations = new List<PricingGuideStationDto>()
                };

                var stationPriceGuide = marketPricingGuide.GroupBy(mpg => mpg.station);

                foreach (var stationGuide in stationPriceGuide)
                {
                    var station = stationGuide.Key;
                    var pricingGuideStation = new PricingGuideStationDto()
                    {
                        StationCode = station.station_code,
                        Affiliation = station.affiliation,
                        CallLetters = station.station_call_letters,
                        LegacyCallLetters = station.legacy_call_letters,
                        Programs = new List<PricingGuideProgramDto>()
                    };
                    var programPriceGuide = stationGuide.GroupBy(spg => spg.station_inventory_manifest_dayparts);
                    foreach (var programGuide in programPriceGuide)
                    {
                        var program = programGuide.Key;
                        var guide = programGuide.First();
                        var pricingGuideProgram = new PricingGuideProgramDto()
                        {
                            ProgramId = program.station_inventory_manifest_id,
                            ProgramName = program.program_name,
                            ManifestDaypartId = program.id,
                            Impressions = guide.impressions,
                            StationImpressionsPerSpot = guide.station_impressions,
                            BlendedCpm = guide.blended_cpm,
                            Cost = guide.cost,
                            CostPerSpot = guide.cost_per_spot,
                            ImpressionsPerSpot = guide.impressions_per_spot,
                            Spots = guide.spots,
                            Daypart = new LookupDto(program.daypart_id, program.daypart.daypart_text)
                        };
                        pricingGuideStation.Programs.Add(pricingGuideProgram);
                    }
                    pricingGuideMarket.Stations.Add(pricingGuideStation);
                }
                response.Markets.Add(pricingGuideMarket);
            }

            response.Markets = _ApplyDefaultSortingForPricingGuideMarkets(response.Markets);

            return response;
        }

        private PricingGuideOpenMarketInventory _GetPricingGuideInventory(int proposalDetailId)
        {
            var inventory = _ProposalOpenMarketInventoryRepository.GetProposalDetailPricingGuideInventory(proposalDetailId);
            _SetProposalInventoryDetailSpotLength(inventory);
            return inventory;
        }

        private List<ProposalProgramDto> _GetPricingGuidePrograms(PricingGuideOpenMarketInventory inventory)
        {
            var programs = _GetPrograms(inventory);
            _FilterProgramsByDaypart(inventory, programs);
            _ApplyDaypartNames(programs);
            _ApplyProjectedImpressions(programs, inventory);
            _ApplyStationImpressions(programs, inventory);
            _CalculateProgramsCostsAndTotals(programs);
            return programs;
        }

        private void _SetPricingGuideMarketsFilteredByIds(
            PricingGuideOpenMarketInventory inventory, 
            List<int> marketIdsFilter,
            BasePricingGuideDto pricingGuideDto,
            bool shouldRunDistribution)
        {
            var programs = _GetPricingGuidePrograms(inventory);
            var markets = _GroupProgramsByMarketAndStationForPricingGuide(programs);
            var postingBookId = ProposalServiceHelper.GetBookId(inventory);
            _ApplyInventoryMarketRankings(postingBookId, markets);
            _ApplyInventoryMarketCoverages(markets);

            inventory.AllMarkets = _MapAllMarketsObject(markets);
            
            markets = _FilterMarketsByIds(markets, marketIdsFilter);
            markets = markets.OrderBy(m => m.MarketRank).ToList();
            _SumTotalsForMarkets(markets);
            _CalculateCpmForMarkets(markets);
            markets = _ApplyDefaultSortingForPricingGuideMarkets(markets);
            _ApplyProgramAndGenreFilterForMarkets(markets, inventory.Criteria);

            inventory.Markets = markets;

            if (shouldRunDistribution)
            {
                var distributionRequest = GetDistributionRequest(pricingGuideDto);
                _PricingGuideDistributionEngine.CalculateMarketDistribution(inventory, distributionRequest);
            }

            _FilterMarketProgramsByCpm(inventory.Markets, pricingGuideDto.OpenMarketPricing);
            _RemoveEmptyStationsAndMarkets(inventory);
            _AllocateSpots(inventory, pricingGuideDto);
            _SetSelectedMarketsInAllMarketsObject(inventory);
        }

        private PricingGuideOpenMarketInventoryRequestDto GetDistributionRequest(BasePricingGuideDto dto)
        {
            return new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalDetailId = dto.ProposalDetailId,
                BudgetGoal = dto.BudgetGoal,
                ImpressionGoal = dto.ImpressionGoal,
                OpenMarketPricing = dto.OpenMarketPricing
            };
        }

        private List<PricingGuideMarketDto> _FilterMarketsByIds(List<PricingGuideMarketDto> markets, List<int> marketIds)
        {
            return marketIds == null || !marketIds.Any()
                ? markets
                : markets.Where(x => marketIds.Contains(x.MarketId)).ToList();
        }

        private void _SetSelectedMarketsInAllMarketsObject(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory)
        {
            foreach (var selectedMarket in pricingGuideOpenMarketInventory.Markets)
            {
                var market = pricingGuideOpenMarketInventory.AllMarkets.Single(x => x.Id == selectedMarket.MarketId);
                market.Selected = true;
            }
        }

        private List<PricingGuideMarketTotalsDto> _MapAllMarketsObject(List<PricingGuideMarketDto> inventoryMarkets)
        {
            return inventoryMarkets.Select(x => new PricingGuideMarketTotalsDto
            {
                Id = x.MarketId,
                Coverage = x.MarketCoverage,
                Name = x.MarketName,
                Stations = x.Stations.Count(),
                Programs = x.Stations.Select(station => station.Programs.Count()).Sum(),
                MinCpm = x.MinCpm,
                MaxCpm = x.MaxCpm,
                AvgCpm = x.AvgCpm
            }).ToList();
        }

        public void _RemoveEmptyStationsAndMarkets(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory)
        {
            foreach (var market in pricingGuideOpenMarketInventory.Markets)
            {
                var emptyStations = market.Stations.Where(y => !y.Programs.Any()).ToList();

                market.Stations.RemoveAll(x => emptyStations.Contains(x));
            }

            var emptyMarkets = pricingGuideOpenMarketInventory.Markets.Where(x => !x.Stations.Any());

            pricingGuideOpenMarketInventory.Markets.RemoveAll(x => emptyMarkets.Contains(x));
        }

        private void _FilterMarketProgramsByCpm(List<PricingGuideMarketDto> markets, OpenMarketPricingGuideDto openMarketPricing)
        {
            var cpmMaxHasValue = openMarketPricing.CpmMax.HasValue && openMarketPricing.CpmMax != 0;
            var cpmMinHasValue = openMarketPricing.CpmMin.HasValue && openMarketPricing.CpmMin != 0;

            if (!cpmMaxHasValue && !cpmMinHasValue)
                return;

            foreach (var market in markets)
            {
                foreach (var station in market.Stations)
                {
                    var programs = station.Programs;

                    if (cpmMaxHasValue)
                    {
                        programs = programs.Where(x => x.BlendedCpm < openMarketPricing.CpmMax.Value).ToList();
                    }

                    if (cpmMinHasValue)
                    {
                        programs = programs.Where(x => x.BlendedCpm > openMarketPricing.CpmMin.Value).ToList();
                    }

                    station.Programs = programs;
                }
            }
        }

        private void _AllocateSpots(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory, BasePricingGuideDto pricingGuideDto)
        {
            if (pricingGuideDto.BudgetGoal == null && pricingGuideDto.ImpressionGoal == null)
            {
                _AllocateSpotsWithoutGoals(pricingGuideOpenMarketInventory, pricingGuideDto);
            }
            else
            {
                _AllocateSpotsWithGoals(pricingGuideOpenMarketInventory, pricingGuideDto);
            }
        }

        private void _AllocateSpotsWithoutGoals(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory, BasePricingGuideDto pricingGuideDto)
        {
            var openMarketPricing = pricingGuideDto.OpenMarketPricing;

            if (openMarketPricing.OpenMarketCpmTarget == OpenMarketCpmTarget.Max)
                _AllocateMaxSpots(pricingGuideOpenMarketInventory);
            else if (openMarketPricing.OpenMarketCpmTarget == OpenMarketCpmTarget.Min)
                _AllocateMinSpots(pricingGuideOpenMarketInventory);
            else if (openMarketPricing.OpenMarketCpmTarget == OpenMarketCpmTarget.Avg)
                _AllocateAvgSpots(pricingGuideOpenMarketInventory);
        }

        private void _AllocateSpotsWithGoals(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory, BasePricingGuideDto pricingGuideDto)
        {
            var unitCapPerStation = pricingGuideDto.OpenMarketPricing.UnitCapPerStation ?? 1;
            var budgetGoal = pricingGuideDto.BudgetGoal ?? Decimal.MaxValue;
            var impressionsGoal = pricingGuideDto.ImpressionGoal ?? Double.MaxValue;

            foreach (var market in pricingGuideOpenMarketInventory.Markets)
            {
                var marketPrograms = market.Stations.SelectMany(s => s.Programs);
                var minProgram = marketPrograms.Where(x => x.BlendedCpm != 0).OrderBy(x => x.BlendedCpm).FirstOrDefault();

                if (minProgram != null)
                {
                    minProgram.Spots = 1;
                    minProgram.Impressions = minProgram.Spots * minProgram.EffectiveImpressionsPerSpot;
                    minProgram.Cost = minProgram.Spots * minProgram.CostPerSpot;
                    impressionsGoal -= minProgram.EffectiveImpressionsPerSpot;
                    budgetGoal -= minProgram.CostPerSpot;
                }
            }

            var allocatableProgram = _GetNextGoalAllocatableProgram(pricingGuideOpenMarketInventory.Markets, unitCapPerStation);
            while (budgetGoal > 0 && impressionsGoal > 0 && allocatableProgram != null)
            {
                allocatableProgram.Spots = allocatableProgram.Spots + 1;
                allocatableProgram.Impressions = allocatableProgram.Spots * allocatableProgram.EffectiveImpressionsPerSpot;
                allocatableProgram.Cost = allocatableProgram.Spots * allocatableProgram.CostPerSpot;
                impressionsGoal -= allocatableProgram.EffectiveImpressionsPerSpot;
                budgetGoal -= allocatableProgram.CostPerSpot;
                allocatableProgram = _GetNextGoalAllocatableProgram(pricingGuideOpenMarketInventory.Markets, unitCapPerStation);
            }
        }

        private static PricingGuideProgramDto _GetNextGoalAllocatableProgram(List<PricingGuideMarketDto> markets, int stationCap)
        {
            var allocatableStations = markets.SelectMany(m => m.Stations).Where(
                s => s.Programs.Sum(p => p.Spots) < stationCap);
            var program = allocatableStations.SelectMany(
                s => s.Programs.Where(
                    p => p.BlendedCpm != 0)).OrderBy(x => x.BlendedCpm).FirstOrDefault();
            return program;
        }

        private void _AllocateMinSpots(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory)
        {
            foreach (var market in pricingGuideOpenMarketInventory.Markets)
            {
                var marketPrograms = market.Stations.SelectMany(s => s.Programs);
                var minProgram = marketPrograms.Where(x => x.BlendedCpm != 0).OrderBy(x => x.BlendedCpm).FirstOrDefault();

                if (minProgram != null)
                {
                    minProgram.Spots = 1;
                    minProgram.Impressions = minProgram.Spots * minProgram.EffectiveImpressionsPerSpot;
                    minProgram.Cost = minProgram.Spots * minProgram.CostPerSpot;
                }

            }
        }

        private void _AllocateAvgSpots(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory)
        {
            foreach (var market in pricingGuideOpenMarketInventory.Markets)
            {
                var marketPrograms = market.Stations.SelectMany(s => s.Programs);
                var avgPrograms = marketPrograms.Where(x => x.BlendedCpm != 0);
                var count = avgPrograms.Count();

                if (count == 0)
                    continue;

                var totalCpm = avgPrograms.Sum(x => x.BlendedCpm);
                var averageCpm = totalCpm / count;
                var programClosestToAvg = avgPrograms.First();
                var distanceToAverage = Math.Abs(programClosestToAvg.BlendedCpm - averageCpm);

                foreach (var program in avgPrograms)
                {
                    if (Math.Abs(program.BlendedCpm - averageCpm) < distanceToAverage)
                    {
                        programClosestToAvg = program;
                        distanceToAverage = Math.Abs(program.BlendedCpm - averageCpm);
                    }
                }

                programClosestToAvg.Spots = 1;
                programClosestToAvg.Impressions = programClosestToAvg.Spots * programClosestToAvg.EffectiveImpressionsPerSpot;
                programClosestToAvg.Cost = programClosestToAvg.Spots * programClosestToAvg.CostPerSpot;
            }
        }

        private void _AllocateMaxSpots(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory)
        {
            foreach (var market in pricingGuideOpenMarketInventory.Markets)
            {
                var marketPrograms = market.Stations.SelectMany(s => s.Programs);
                var maxProgram = marketPrograms.Where(x => x.BlendedCpm != 0).OrderByDescending(x => x.BlendedCpm).FirstOrDefault();

                if (maxProgram != null)
                {
                    maxProgram.Spots = 1;
                    maxProgram.Impressions = maxProgram.Spots * maxProgram.EffectiveImpressionsPerSpot;
                    maxProgram.Cost = maxProgram.Spots * maxProgram.CostPerSpot;
                }
            }
        }

        private void _CalculateCpmForMarkets(List<PricingGuideMarketDto> markets)
        {
            foreach (var market in markets)
            {
                var programs = market.Stations.SelectMany(s => s.Programs.Where(y => y.BlendedCpm != 0));

                if (programs.Any())
                {
                    market.MinCpm = programs.Min(y => y.BlendedCpm);
                    market.AvgCpm = programs.Average(y => y.BlendedCpm);
                    market.MaxCpm = programs.Max(y => y.BlendedCpm);
                }
                else
                {
                    market.MinCpm = 0;
                    market.AvgCpm = 0;
                    market.MaxCpm = 0;
                }
            }
        }

        private PricingGuideOpenMarketDistributionDto _MapToPricingGuideOpenMarketInventoryDto(BasePricingGuideDto dto, PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory)
        {
            return new PricingGuideOpenMarketDistributionDto
            {
                ProposalDetailId = dto.ProposalDetailId,
                BudgetGoal = dto.BudgetGoal,
                ImpressionGoal = dto.ImpressionGoal,
                OpenMarketPricing = dto.OpenMarketPricing,
                Markets = pricingGuideOpenMarketInventory.Markets,
                AllMarkets = pricingGuideOpenMarketInventory.AllMarkets
            };
        }

        private PricingGuideOpenMarketDistributionDto _ConvertPricingGuideOpenMarketDistributionDto(PricingGuideAllocationSaveRequestDto dto)
        {
            return new PricingGuideOpenMarketDistributionDto()
            {
                Markets = dto.Markets,
                Filter = dto.Filter,
                DisplayFilter = dto.DisplayFilter
            };
        }
        private void _CalculateProgramsCostsAndTotals(List<ProposalProgramDto> programs)
        {
            _ProposalProgramsCalculationEngine.CalculateBlendedCpmForProgramsRaw(programs);
            _ProposalProgramsCalculationEngine.CalculateAvgCostForPrograms(programs);
            _ProposalProgramsCalculationEngine.CalculateTotalCostForPrograms(programs);
            _ProposalProgramsCalculationEngine.CalculateTotalImpressionsForPrograms(programs);
        }

        private void _SumTotalsForMarkets(List<PricingGuideMarketDto> markets)
        {
            markets.ForEach(m => m.TotalCost = m.Stations.Sum(s => s.Programs.Sum(p => p.Cost)));
            markets.ForEach(m => m.TotalSpots = m.Stations.Sum(s => s.Programs.Sum(p => p.Spots)));
            markets.ForEach(m => m.TotalImpressions = m.Stations.Sum(s => s.Programs.Sum(p => p.Impressions)));
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

        private List<ProposalProgramDto> _GetPrograms(ProposalDetailInventoryBase inventory)
        {
            var proposalMarketIds = ProposalMarketsCalculationEngine.GetProposalMarketsList(inventory.ProposalId, inventory.ProposalVersion).Select(m => m.Id).ToList();
            var stationProgramRepository = BroadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            var programs = stationProgramRepository.GetStationProgramsForProposalDetail(inventory.DetailFlightStartDate,
                                                                                        inventory.DetailFlightEndDate,
                                                                                        inventory.DetailSpotLengthId,
                                                                                        BroadcastConstants.OpenMarketSourceId,
                                                                                        proposalMarketIds);

            _SetFlightWeeks(programs);

            return programs;
        }

        private List<ProposalProgramDto> _GetProgramsFromPricingGuide(ProposalDetailInventoryBase pricingGuideOpenMarketDto, List<open_market_pricing_guide> exstingPricingGuide)
        {
            return null;
        }

        public PricingGuideOpenMarketDistributionDto ApplyFilterOnOpenMarketPricingGuideGrid(PricingGuideOpenMarketDistributionDto dto)
        {
            _ApplyFilterForProposalOpenMarketPricingGuideGrid(dto);
            _SumTotalsForMarkets(dto.Markets);
            dto.Markets = _ApplyDefaultSortingForPricingGuideMarkets(dto.Markets);

            return dto;
        }

        private static void _SetProposalOpenMarketPricingGuideGridDisplayFilters(PricingGuideOpenMarketDistributionDto dto)
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

            dto.DisplayFilter.Markets = dto.Markets
                .Select(a => new { a.MarketId, a.MarketName })
                .Distinct()
                .Select(y => new LookupDto { Id = y.MarketId, Display = y.MarketName })
                .OrderBy(n => n.Display)
                .ToList();

            dto.DisplayFilter.Affiliations = stations
               .Select(s => s.Affiliation)
               .Distinct(StringComparer.OrdinalIgnoreCase)
               .OrderBy(a => a)
               .ToList();

            dto.DisplayFilter.Genres = stations
                .SelectMany(s => s.Programs.Where(p => p != null).SelectMany(p => p.Genres))
                .Select(g => new { g.Id, g.Display })
                .Distinct()
                .Select(g => new LookupDto { Id = g.Id, Display = g.Display })
                .OrderBy(g => g.Display)
                .ToList();
        }

        private void _ApplyFilterForProposalOpenMarketPricingGuideGrid(PricingGuideOpenMarketDistributionDto dto)
        {
            if (dto.Filter == null)
            {
                return;
            }

            var filter = dto.Filter;

            _ApplyMarketsFilter(dto, filter);

            foreach (var market in dto.Markets)
            {
                _ApplyAffiliationsFilter(market, filter);

                foreach (var station in market.Stations)
                {
                    _ApplyProgramNamesFilter(station, filter);
                    _ApplyGenresFilter(station, filter);
                    _ApplyAirtimesFilter(station, filter);
                    _ApplySpotFilter(station, filter);
                }
            }
        }

        private static void _ApplyProgramNamesFilter(PricingGuideStationDto station, OpenMarketPricingGuideGridFilterDto filter)
        {
            var programNames = filter.ProgramNames;

            if (programNames != null && programNames.Any())
            {
                station.Programs = station.Programs.Where(p => programNames.Contains(p.ProgramName, StringComparer.OrdinalIgnoreCase)).ToList();
            }
        }

        private static void _ApplyGenresFilter(PricingGuideStationDto station, OpenMarketPricingGuideGridFilterDto filter)
        {
            var genres = filter.Genres;

            if (genres != null && genres.Any())
            {
                station.Programs = station.Programs
                                          .Where(p => p.Genres.Any(genre => genres.Contains(genre.Id)))
                                          .ToList();
            }
        }

        private static void _ApplyMarketsFilter(PricingGuideOpenMarketDistributionDto dto, OpenMarketPricingGuideGridFilterDto filter)
        {
            var markets = filter.Markets;

            if (markets != null && markets.Any())
            {
                dto.Markets = dto.Markets.Where(m => markets.Contains(m.MarketId)).ToList();
            }
        }

        private static void _ApplyAffiliationsFilter(PricingGuideMarketDto market, OpenMarketPricingGuideGridFilterDto filter)
        {
            var affiliations = filter.Affiliations;

            if (affiliations != null && affiliations.Any())
            {
                market.Stations = market.Stations.Where(s => affiliations.Contains(s.Affiliation, StringComparer.OrdinalIgnoreCase)).ToList();
            }
        }

        private void _ApplyAirtimesFilter(PricingGuideStationDto station, OpenMarketPricingGuideGridFilterDto filter)
        {
            var airtimes = filter.DayParts;

            if (airtimes != null && airtimes.Any())
            {
                station.Programs = station.Programs
                                          .Where(p => airtimes.Any(a => DisplayDaypart.Intersects(DaypartDto.ConvertDaypartDto(a), DaypartCache.GetDisplayDaypart(p.Daypart.Id))))
                                          .ToList();
            }
        }

        private static void _ApplySpotFilter(PricingGuideStationDto station, OpenMarketPricingGuideGridFilterDto filter)
        {
            var spotFilter = filter.SpotFilter;

            if (spotFilter.HasValue && spotFilter.Value != OpenMarketPricingGuideGridFilterDto.OpenMarketSpotFilter.AllPrograms)
            {
                station.Programs = (spotFilter.Value == OpenMarketPricingGuideGridFilterDto.OpenMarketSpotFilter.ProgramWithSpots
                    ? station.Programs.Where(p => p.Spots > 0)
                    : station.Programs.Where(p => p.Spots == 0)).ToList();
            }
        }

        private void _SetCanEditSpotsForPrograms(PricingGuideOpenMarketDistributionDto dto)
        {
            var programs = dto.Markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs);

            foreach (var program in programs)
            {
                program.HasImpressions = program.EffectiveImpressionsPerSpot > 0;
            }
        }

        public PricingGuideOpenMarketDistributionDto SavePricingGuideAllocations(PricingGuideAllocationSaveRequestDto request)
        {
            var dto = _ConvertPricingGuideOpenMarketDistributionDto(request);
            var programs = dto.Markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs).ToList();
            _ValidateAllocations(programs);

            _ProposalProgramsCalculationEngine.CalculateTotalCostForPrograms(programs);
            _ProposalProgramsCalculationEngine.CalculateTotalImpressionsForPrograms(programs);
            ApplyFilterOnOpenMarketPricingGuideGrid(dto);

            var detail = _ProposalRepository.GetProposalDetail(request.ProposalDetailId);
            _SumTotalsForPricingGuide(dto, detail.PricingGuide.ProprietaryPricing);

            return dto;
        }

        private void _ValidateAllocations(List<PricingGuideProgramDto> programs)
        {
            foreach(var program in programs)
            {
                var hasPositiveImpressions = program.EffectiveImpressionsPerSpot > 0;

                if (!hasPositiveImpressions && program.Spots > 0)
                {
                    throw new Exception("Cannot allocate spots that have zero impressions");
                }
            }
        }

        public PricingGuideOpenMarketDistributionDto UpdateOpenMarketPricingGuideMarkets(PricingGuideOpenMarketDistributionDto dto)
        {
            var programSpots = dto.Markets
                .SelectMany(x => x.Stations)
                .SelectMany(x => x.Programs)
                .Select(x => new KeyValuePair<int, int>(x.ProgramId, x.Spots))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var selectedMarketIds = dto.AllMarkets.Where(x => x.Selected).Select(x => x.Id).ToList();
            var detail = _ProposalRepository.GetProposalDetail(dto.ProposalDetailId);
            var inventory = _GetPricingGuideInventory(dto.ProposalDetailId);
            _SetPricingGuideMarketsFilteredByIds(inventory, selectedMarketIds, dto, false);
            _SetAllocatedSpots(inventory.Markets, programSpots);
            return _GetPricingGuideOpenMarketInventoryDto(dto, inventory, detail.PricingGuide.ProprietaryPricing);
        }

        private PricingGuideOpenMarketDistributionDto _GetPricingGuideOpenMarketInventoryDto(
            BasePricingGuideDto baseDto, 
            PricingGuideOpenMarketInventory inventory,
            List<ProprietaryPricingDto> proprietaryPricingValues)
        {
            var pricingGuideDto = _MapToPricingGuideOpenMarketInventoryDto(baseDto, inventory);
            pricingGuideDto.Markets = _ApplyDefaultSortingForPricingGuideMarkets(pricingGuideDto.Markets);
            _SetCanEditSpotsForPrograms(pricingGuideDto);
            _SetProposalOpenMarketPricingGuideGridDisplayFilters(pricingGuideDto);
            _SumTotalsForMarkets(pricingGuideDto.Markets);
            _SumTotalsForPricingGuide(pricingGuideDto, proprietaryPricingValues);
            return pricingGuideDto;
        }

        private void _SetAllocatedSpots(List<PricingGuideMarketDto> markets, Dictionary<int, int> programSpots)
        {
            var programs = markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs).ToList();
            _ValidateAllocations(programs);

            foreach (var program in programs)
            {
                if (programSpots.ContainsKey(program.ProgramId))
                {
                    program.Spots = programSpots[program.ProgramId];
                }
            }

            _ProposalProgramsCalculationEngine.CalculateTotalCostForPrograms(programs);
            _ProposalProgramsCalculationEngine.CalculateTotalImpressionsForPrograms(programs);
            _SumTotalsForMarkets(markets);
        }
    }
}
