﻿using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.DTO.PricingGuide;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using static Services.Broadcast.Entities.OpenMarketInventory.OpenMarketAllocationSaveRequest;

namespace Services.Broadcast.ApplicationServices
{
    public interface IPricingGuideService : IApplicationService
    {
        /// <summary>
        /// Gets the pricing guide for a proposal
        /// </summary>
        /// <param name="proposalDetailId">Proposal detail id</param>
        /// <returns>ProposalDetailPricingGuideDto object</returns>
        PricingGuideDto GetPricingGuideForProposalDetail(int proposalDetailId);

        /// <summary>
        /// Saves the pricing guide data and distribution
        /// </summary>
        /// <param name="model">ProposalDetailPricingGuideSaveRequest object</param>
        /// <param name="username">User requesting the save</param>
        /// <returns>ProposalDetailDto object</returns>
        ProposalDetailDto SaveDistribution(ProposalDetailPricingGuideSaveRequest model, string username);

        PricingGuideDto SaveAllocations(PricingGuideDto dto);

        /// <summary>
        /// Distribution
        /// </summary>
        /// <param name="request">PricingGuideOpenMarketInventoryRequestDto request object</param>
        /// <returns>PricingGuideDto object</returns>
        PricingGuideDto GetOpenMarketInventory(PricingGuideOpenMarketInventoryRequestDto request);

        PricingGuideDto ApplyFilterOnOpenMarketGrid(PricingGuideDto dto);

        PricingGuideDto UpdateOpenMarketMarkets(PricingGuideDto dto);

        PricingGuideDto UpdateProprietaryCpms(PricingGuideDto dto);

        bool HasSpotsAllocated(int proposalDetailId);

        /// <summary>
        /// Copies the saved allocated spots in the distribution to Open Market Buy grid
        /// </summary>
        /// <param name="proposalDetailId">Proposal detail id</param>
        /// <returns>bool</returns>
        bool CopyPricingGuideAllocationsToOpenMarket(int proposalDetailId);
    }

    public class PricingGuideService : BaseProposalInventoryService, IPricingGuideService
    {
        private readonly IProposalRepository _ProposalRepository;
        private readonly IPricingGuideRepository _PricingGuideRepository;
        private readonly IProposalProgramsCalculationEngine _ProposalProgramsCalculationEngine;
        private readonly IDaypartCache _DaypartCache;
        private readonly IStationProgramRepository _StationProgramRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IPricingGuideDistributionEngine _PricingGuideDistributionEngine;
        private readonly IProposalOpenMarketInventoryService _ProposalOpenMarketInventoryService;

        /// <summary>
        /// Constructor
        /// </summary>
        public PricingGuideService(IDataRepositoryFactory broadcastDataRepositoryFactory
            , IProposalProgramsCalculationEngine proposalProgramsCalculationEngine
            , IDaypartCache daypartCache
            , IProposalMarketsCalculationEngine proposalMarketsCalculationEngine
            , IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache
            , IProposalTotalsCalculationEngine proposalTotalsCalculationEngine
            , IPricingGuideDistributionEngine pricingGuideDistributionEngine
            , IImpressionAdjustmentEngine impressionAdjustmentEngine
            , IProposalOpenMarketInventoryService proposalOpenMarketInventoryService)
            : base(broadcastDataRepositoryFactory, daypartCache, proposalMarketsCalculationEngine
                , proposalTotalsCalculationEngine, mediaMonthAndWeekAggregateCache, impressionAdjustmentEngine)
        {
            _PricingGuideRepository = broadcastDataRepositoryFactory.GetDataRepository<IPricingGuideRepository>();
            _ProposalRepository = broadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
            _StationProgramRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _ProposalProgramsCalculationEngine = proposalProgramsCalculationEngine;
            _DaypartCache = daypartCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _PricingGuideDistributionEngine = pricingGuideDistributionEngine;
            _ProposalOpenMarketInventoryService = proposalOpenMarketInventoryService;
        }

        /// <summary>
        /// Gets the pricing guide for a proposal
        /// </summary>
        /// <param name="proposalDetailId">Proposal detail id</param>
        /// <returns>PricingGuideDto object</returns>
        public PricingGuideDto GetPricingGuideForProposalDetail(int proposalDetailId)
        {
            var distribution = _PricingGuideRepository.GetPricingGuideForProposalDetail(proposalDetailId);
            if (distribution.DistributionId > 0)
            {
                var proposal = _ProposalRepository.GetProposalById(distribution.ProposalId);
                distribution.PricingTotals = _SumPricingTotals(distribution.OpenMarketTotals, distribution.ProprietaryTotals);
                distribution.Markets = _LoadMarketsForDistribution(distribution.DistributionId, proposal.Details.Single(x=>x.Id == proposalDetailId));
                distribution.AllMarkets = _LoadAllMarketsForDistribution(proposal, distribution.ProposalDetailId, distribution.Markets);
                _SetProposalOpenMarketPricingGuideGridDisplayFilters(distribution);
                _SumTotalsForMarketsAndStations(distribution.Markets);
            }

            return distribution;
        }

        private List<PricingGuideMarketTotalsDto> _LoadAllMarketsForDistribution(ProposalDto proposal, int detailId, List<PricingGuideMarketDto> selectedMarkets)
        {
            var programs = _GetPrograms(new ProposalDetailInventoryBase
            {
                ProposalId = proposal.Id.Value,
                ProposalVersion = proposal.Version.Value,
                DetailFlightStartDate = proposal.FlightStartDate.Value,
                DetailFlightEndDate = proposal.FlightEndDate.Value,
                DetailSpotLengthId = proposal.Details.First(x => x.Id == detailId).SpotLengthId
            });
            _FilterProgramsByDaypart(new ProposalDetailInventoryBase
            {
                DetailDaypartId = proposal.Details.Single(x => x.Id == detailId).DaypartId
            }, programs);
            ApplyDaypartNames(programs);
            var markets = _GroupProgramsByMarketAndStationForPricingGuide(programs);
            var marketCoverageDto = _GetMarketCoverages(markets);
            _ApplyInventoryMarketCoverages(markets, marketCoverageDto);
            var allMarkets = _MapAllMarketsObject(markets);
            _SetMarketCpms(allMarkets, selectedMarkets);
            _SetSelectedMarketsInAllMarketsObject(selectedMarkets, allMarkets);
            return allMarkets;
        }

        private void _SetMarketCpms(List<PricingGuideMarketTotalsDto> allMarkets, List<PricingGuideMarketDto> selectedMarkets)
        {
            foreach (var selectedMarket in selectedMarkets)
            {
                var market = allMarkets.Single(x => x.Id == selectedMarket.MarketId);

                market.MinCpm = selectedMarket.MinCpm;
                market.AvgCpm = selectedMarket.AvgCpm;
                market.MaxCpm = selectedMarket.MaxCpm;
            }
        }

        private List<PricingGuideMarketDto> _LoadMarketsForDistribution(int distributionId, ProposalDetailDto proposalDetail)
        {
            var distributionMarkets = _PricingGuideRepository.GetDistributionMarkets(distributionId);
            var inventoryMarkets = distributionMarkets.GroupBy(p => p.Market.Id).Select(
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
                            Programs = s.Select(x => new PricingGuideProgramDto
                            {
                                ProgramId = x.ProgramId,
                                Spots = x.Spots,
                                SpotsEditedManually = x.SpotsEditedManually,
                                BlendedCpm = x.BlendedCpm,
                                CostPerSpot = x.CostPerSpot,
                                Daypart = x.Daypart,
                                ImpressionsPerSpot = x.ImpressionsPerSpot,
                                ProgramName = x.ManifestDaypart.Display,
                                ManifestDaypartId = x.ManifestDaypart.Id,
                                StationImpressionsPerSpot = x.StationImpressionsPerSpot,
                                Genres = x.Genres
                            }).ToList()
                        }).ToList()
                }).ToList();
            inventoryMarkets.ForEach(x => x.Stations.ForEach(y => y.Programs.ForEach(p =>
            {
                p.Impressions = p.EffectiveImpressionsPerSpot * p.Spots;
                p.Cost = p.CostPerSpot * p.Spots;
            })));
            var postingBookId = ProposalServiceHelper.GetBookId(proposalDetail);
            ApplyInventoryMarketRankings(postingBookId, inventoryMarkets);
            var marketCoverageDto = _GetMarketCoverages(inventoryMarkets);
            _ApplyInventoryMarketCoverages(inventoryMarkets, marketCoverageDto);
            _CalculateCpmForMarkets(inventoryMarkets);
            return inventoryMarkets;
        }

        public PricingGuideDto SaveAllocations(PricingGuideDto dto)
        {
            var programs = dto.Markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs).ToList();
            _ValidateAllocations(programs);

            _ProposalProgramsCalculationEngine.CalculateTotalCostForPrograms(programs);
            _ProposalProgramsCalculationEngine.CalculateTotalImpressionsForPrograms(programs);
            ApplyFilterOnOpenMarketGrid(dto);
            dto.ProprietaryPricing = _GetProprietaryPricings(dto.ProprietaryPricing, dto.ProposalDetailId);
            _SumTotalsForPricingGuide(dto);

            return dto;
        }

        public PricingGuideDto ApplyFilterOnOpenMarketGrid(PricingGuideDto dto)
        {
            _ApplyFilterForProposalOpenMarketPricingGuideGrid(dto);
            _SumTotalsForMarketsAndStations(dto.Markets);
            dto.Markets = ApplyDefaultSortingForPricingGuideMarkets(dto.Markets);

            return dto;
        }

        /// <summary>
        /// Saves the pricing guide data and distribution
        /// </summary>
        /// <param name="model">ProposalDetailPricingGuideSaveRequest object</param>
        /// <param name="username">User requesting the save</param>
        /// <returns>ProposalDetailDto object</returns>
        public ProposalDetailDto SaveDistribution(ProposalDetailPricingGuideSaveRequest model, string username)
        {
            _ValidateProprietaryPricing(model.ProprietaryPricing);
            _PricingGuideRepository.SavePricingGuideDistribution(model, username);
            return _ProposalRepository.GetProposalDetail(model.ProposalDetailId);
        }

        /// <summary>
        /// Distribution
        /// </summary>
        /// <param name="request">PricingGuideOpenMarketInventoryRequestDto request object</param>
        /// <returns>PricingGuideDto object</returns>
        public PricingGuideDto GetOpenMarketInventory(PricingGuideOpenMarketInventoryRequestDto request)
        {
            PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory = null;
            PricingGuideDto distributionDto = new PricingGuideDto
            {
                ProposalId = request.ProposalId,
                ProposalDetailId = request.ProposalDetailId,
                ProprietaryPricing = request.ProprietaryPricing,
                OpenMarketPricing = request.OpenMarketPricing
            };

            pricingGuideOpenMarketInventory = GetPricingGuideInventory(request.ProposalDetailId);
            distributionDto = _MapToPricingGuideDto(request, pricingGuideOpenMarketInventory);
            _PopulatePricingGuideMarketsAndFilterByIds(pricingGuideOpenMarketInventory, null, distributionDto, true);

            //I need the second mapping because the objects changed
            distributionDto = _MapToPricingGuideDto(request, pricingGuideOpenMarketInventory);
            var dto =  _GetPricingGuideOpenMarketInventoryDto(distributionDto, _GetProprietaryPricings(distributionDto.ProprietaryPricing, request.ProposalDetailId));
            return dto;
        }

        public PricingGuideDto UpdateOpenMarketMarkets(PricingGuideDto dto)
        {
            var programSpots = dto.Markets
                .SelectMany(x => x.Stations)
                .SelectMany(x => x.Programs)
                .Select(x => new KeyValuePair<int, int>(x.ManifestDaypartId, x.Spots))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var selectedMarketIds = dto.AllMarkets.Where(x => x.Selected).Select(x => x.Id).ToList();
            var inventory = GetPricingGuideInventory(dto.ProposalDetailId);

            _PopulatePricingGuideMarketsAndFilterByIds(inventory, selectedMarketIds, dto, false);
            _SetAllocatedSpots(inventory.Markets, programSpots);
            dto.Markets = inventory.Markets;
            return _GetPricingGuideOpenMarketInventoryDto(dto, _GetProprietaryPricings(dto.ProprietaryPricing, dto.ProposalDetailId));
        }

        public PricingGuideDto UpdateProprietaryCpms(PricingGuideDto dto)
        {
            _SumTotalsForPricingGuide(dto);

            return dto;
        }

        private void _ApplyFilterForProposalOpenMarketPricingGuideGrid(PricingGuideDto dto)
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

        private static void _ApplyMarketsFilter(PricingGuideDto dto, OpenMarketPricingGuideGridFilterDto filter)
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
                                          .Where(p => airtimes.Any(a => DisplayDaypart.Intersects(DaypartDto.ConvertDaypartDto(a), _DaypartCache.GetDisplayDaypart(p.Daypart.Id))))
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

        private void _ValidateAllocations(List<PricingGuideProgramDto> programs)
        {
            foreach (var program in programs)
            {                
                if (program.EffectiveImpressionsPerSpot == 0 && program.Spots > 0)
                {
                    throw new Exception("Cannot allocate spots that have zero impressions");
                }
            }
        }

        private void _SetAllocatedSpots(List<PricingGuideMarketDto> markets, Dictionary<int, int> programSpots)
        {
            var programs = markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs).ToList();
            _ValidateAllocations(programs);

            foreach (var program in programs)
            {
                if (programSpots.ContainsKey(program.ManifestDaypartId))
                {
                    program.Spots = programSpots[program.ManifestDaypartId];
                }
            }

            _ProposalProgramsCalculationEngine.CalculateTotalCostForPrograms(programs);
            _ProposalProgramsCalculationEngine.CalculateTotalImpressionsForPrograms(programs);
        }

        private PricingGuideDto _GetPricingGuideOpenMarketInventoryDto(
            PricingGuideDto pricingGuideDto,
            List<ProprietaryPricingDto> proprietaryPricingValues)
        {
            pricingGuideDto.Markets = ApplyDefaultSortingForPricingGuideMarkets(pricingGuideDto.Markets);
            _SetProposalOpenMarketPricingGuideGridDisplayFilters(pricingGuideDto);
            _SumTotalsForMarketsAndStations(pricingGuideDto.Markets);
            pricingGuideDto.ProprietaryPricing = _GetProprietaryPricings(pricingGuideDto.ProprietaryPricing, pricingGuideDto.ProposalDetailId);
            _SumTotalsForPricingGuide(pricingGuideDto);
            return pricingGuideDto;
        }

        private void _SumTotalsForPricingGuide(PricingGuideDto pricingGuideDto)
        {
            pricingGuideDto.OpenMarketTotals = _SumTotalsForOpenMarketSection(pricingGuideDto.Markets);
            pricingGuideDto.ProprietaryTotals = _SumTotalsForProprietarySection(pricingGuideDto.OpenMarketTotals.Impressions, pricingGuideDto.ProprietaryPricing);
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
            result.Cpm = (double)ProposalMath.CalculateCpm(result.Cost, result.Impressions);

            return result;
        }

        private ProprietaryTotalsDto _SumTotalsForProprietarySection(double openMarketImpressions, List<ProprietaryPricingDto> proprietaryPricingValues)
        {
            var proprietaryImpressionsPercent = proprietaryPricingValues.Sum(x => x.ImpressionsBalance);
            var result = new ProprietaryTotalsDto();
            if (openMarketImpressions == 0 || proprietaryImpressionsPercent == 0)
            {
                return result;
            }

            var openMarketImpressionsPercent = 1 - proprietaryImpressionsPercent;
            var impressionsPerOnePercentage = openMarketImpressions / openMarketImpressionsPercent;
            result.Impressions = proprietaryImpressionsPercent * impressionsPerOnePercentage;
            result.Cpm = (proprietaryPricingValues.Sum(x => (decimal)x.ImpressionsBalance * x.Cpm) / (decimal)proprietaryImpressionsPercent);
            result.Cost = ProposalMath.CalculateCost(result.Cpm, result.Impressions);

            return result;
        }

        private OpenMarketTotalsDto _SumTotalsForOpenMarketSection(List<PricingGuideMarketDto> markets)
        {
            var result = new OpenMarketTotalsDto
            {
                Cost = markets.Sum(x => x.TotalCost),
                Impressions = markets.Sum(x => x.TotalImpressions)
            };
            // Coverage: sum of coverage for each market with a spot allocated, only count each market once.
            result.Coverage = markets.Where(x => x.TotalCost > 0).Sum(x => x.MarketCoverage);
            result.Cpm = ProposalMath.CalculateCpm(result.Cost, result.Impressions);

            return result;
        }

        private List<ProprietaryPricingDto> _GetProprietaryPricings(List<ProprietaryPricingDto> proprietaryPricing, int proposalDetailId)
        {
            if (proprietaryPricing == null)
            {
                return _PricingGuideRepository.GetProprietaryPricings(proposalDetailId);
            }

            return proprietaryPricing;
        }

        private void _SumTotalsForMarketsAndStations(List<PricingGuideMarketDto> markets)
        {
            markets.ForEach(m =>
            {
                _SumTotalsForStations(m.Stations);
                m.TotalCost = m.Stations.Sum(s => s.TotalCost);
                m.TotalSpots = m.Stations.Sum(s => s.TotalSpots);
                m.TotalImpressions = m.Stations.Sum(s => s.TotalImpressions);
                m.CPM = ProposalMath.CalculateCpm(m.TotalCost, m.TotalImpressions);
            });
        }

        private void _SumTotalsForStations(List<PricingGuideStationDto> stations)
        {
            stations.ForEach(s => s.TotalCost = s.Programs.Sum(p => p.Cost));
            stations.ForEach(s => s.TotalSpots = s.Programs.Sum(p => p.Spots));
            stations.ForEach(s => s.TotalImpressions = s.Programs.Sum(p => p.Impressions));
            stations.ForEach(s => s.CPM = ProposalMath.CalculateCpm(s.TotalCost, s.TotalImpressions));
        }

        private static void _SetProposalOpenMarketPricingGuideGridDisplayFilters(PricingGuideDto dto)
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

            stations.ForEach(s => s.Programs.ForEach(p => p.Genres.RemoveAll(g => g == null)));
            dto.DisplayFilter.Genres = stations
                .SelectMany(s => s.Programs.Where(p => p != null).SelectMany(p => p.Genres))
                .Select(g => new { g.Id, g.Display })
                .Distinct()
                .Select(g => new LookupDto { Id = g.Id, Display = g.Display })
                .OrderBy(g => g.Display)
                .ToList();
        }

        private PricingGuideDto _MapToPricingGuideDto(PricingGuideOpenMarketInventoryRequestDto dto
            , PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory)
        {
            return new PricingGuideDto
            {
                ProposalDetailId = dto.ProposalDetailId,
                MarketCoverageFileId = pricingGuideOpenMarketInventory.MarketCoverageFileId,
                BudgetGoal = dto.BudgetGoal,
                ImpressionGoal = dto.ImpressionGoal,
                Margin = dto.Margin,
                Inflation = dto.Inflation,
                ImpressionLoss = dto.ImpressionLoss,
                OpenMarketPricing = dto.OpenMarketPricing,
                OpenMarketShare = dto.OpenMarketShare,
                ProprietaryPricing = dto.ProprietaryPricing,
                Markets = pricingGuideOpenMarketInventory.Markets,
                AllMarkets = pricingGuideOpenMarketInventory.AllMarkets,
                MaintainManuallyEditedSpots = dto.MaintainManuallyEditedSpots
            };
        }

        private void _PopulatePricingGuideMarketsAndFilterByIds(
            PricingGuideOpenMarketInventory inventory,
            List<int> marketIdsFilter,
            PricingGuideDto pricingGuideDto,
            bool shouldRunDistribution)
        {
            var programs = _GetPricingGuidePrograms(inventory, pricingGuideDto);
            var markets = _GroupProgramsByMarketAndStationForPricingGuide(programs);
            var postingBookId = ProposalServiceHelper.GetBookId(inventory);

            ApplyInventoryMarketRankings(postingBookId, markets);

            var marketCoverageDto = _GetMarketCoverages(markets);

            _ApplyInventoryMarketCoverages(markets, marketCoverageDto);

            inventory.MarketCoverageFileId = marketCoverageDto.MarketCoverageFileId;

            _CalculateCpmForMarkets(markets);

            inventory.AllMarkets = _MapAllMarketsObject(markets);

            markets = _FilterMarketsByIds(markets, marketIdsFilter);
            markets = markets.OrderBy(m => m.MarketRank).ToList();
            markets = ApplyDefaultSortingForPricingGuideMarkets(markets);
            _ApplyProgramAndGenreFilterForMarkets(markets, inventory.Criteria);

            inventory.Markets = markets;

            _FilterMarketProgramsByCpm(inventory.Markets, pricingGuideDto.OpenMarketPricing);
            _RemoveEmptyStationsAndMarkets(inventory);
            _SetPricingGuideMarkets(inventory, _MapToPricingGuideOpenMarketInventoryRequestDto(pricingGuideDto), shouldRunDistribution);
            _AllocateSpots(inventory, pricingGuideDto);
            _SetSelectedMarketsInAllMarketsObject(inventory.Markets, inventory.AllMarkets);
        }

        private void _SetPricingGuideMarkets(
            PricingGuideOpenMarketInventory inventory,
            PricingGuideOpenMarketInventoryRequestDto request,
            bool shouldRunDistribution)
        {
            // Only markets in the list are allowed.
            var allowedMarkets = _ProposalMarketsCalculationEngine.GetProposalMarketsList(inventory.ProposalId, inventory.ProposalVersion);
            var allowedMarketsIds = allowedMarkets.Select(x => x.Id);
            inventory.Markets.RemoveAll(x => !allowedMarketsIds.Contains(x.MarketId));

            if (shouldRunDistribution)
            {
                // Markets that need to be included.
                var mustIncludedMarketCodes = inventory.ProposalMarkets
                    .Where(x => !x.IsBlackout)
                    .Select(x => (int)x.Id)
                    .ToList();

                if (request.MaintainManuallyEditedSpots)
                {
                    _SetSpotsEditedManuallyProperty(inventory);
                    mustIncludedMarketCodes = mustIncludedMarketCodes
                        .Union(inventory.Markets.Where(x => x.HasEditedManuallySpots).Select(x => x.MarketId))
                        .ToList();
                }

                var includedMarkets = inventory.Markets.Where(x => mustIncludedMarketCodes.Contains(x.MarketId)).ToList();
                var includedMarketsCoverage = includedMarkets.Sum(x => x.MarketCoverage);

                if (includedMarketsCoverage >= (inventory.MarketCoverage * 100))
                {
                    inventory.Markets.Clear();
                    inventory.Markets.AddRange(includedMarkets);
                    return;
                }

                inventory.Markets.RemoveAll(x => mustIncludedMarketCodes.Contains(x.MarketId));

                var desiredCoverage = inventory.MarketCoverage - (includedMarketsCoverage / 100);
                _PricingGuideDistributionEngine.CalculateMarketDistribution(inventory, request, desiredCoverage);

                inventory.Markets.AddRange(includedMarkets);
            }
        }

        private void _SetSpotsEditedManuallyProperty(PricingGuideOpenMarketInventory inventory)
        {
            var savedDistributionPrograms = _PricingGuideRepository.GetDistributionMarketsByProposalDetailId(inventory.DetailId);
            var programsWithEditedManuallySpots = savedDistributionPrograms
                .Where(x => x.SpotsEditedManually && x.Spots > 0)
                .ToDictionary(x => x.ManifestDaypart.Id, x => x.Spots);
            var inventoryPrograms = inventory.Markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs);

            foreach(var program in inventoryPrograms)
            {
                if (programsWithEditedManuallySpots.TryGetValue(program.ManifestDaypartId, out var spots))
                {
                    program.Spots = spots;
                    program.SpotsEditedManually = true;
                }
            }
        }

        private void _AllocateSpots(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory, PricingGuideDto pricingGuideDto)
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

        private void _AllocateSpotsWithoutGoals(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory, PricingGuideDto pricingGuideDto)
        {
            var cpmTarget = pricingGuideDto.OpenMarketPricing.OpenMarketCpmTarget ?? OpenMarketCpmTarget.Min;

            foreach (var market in pricingGuideOpenMarketInventory.Markets)
            {
                if (pricingGuideDto.MaintainManuallyEditedSpots && market.HasEditedManuallySpots)
                {
                    market.Stations
                        .SelectMany(x => x.Programs)
                        .Where(x => x.SpotsEditedManually)
                        .ForEach(_CalculateImpressionsAndCost);
                }
                else
                {
                    var program = _GetProgramForTarget(market.Stations, cpmTarget, pricingGuideDto.MaintainManuallyEditedSpots);

                    if (program == null)
                        continue;

                    _SetOneSpotProgram(program);
                }
            }
        }

        private void _SetOneSpotProgram(PricingGuideProgramDto program)
        {
            program.Spots = 1;
            _CalculateImpressionsAndCost(program);
        }

        private void _CalculateImpressionsAndCost(PricingGuideProgramDto program)
        {
            program.Impressions = _ProposalProgramsCalculationEngine.CalculateSpotImpressions(program.Spots, program.EffectiveImpressionsPerSpot);
            program.Cost = _ProposalProgramsCalculationEngine.CalculateSpotCost(program.Spots, program.CostPerSpot);
        }

        private void _AllocateSpotsWithGoals(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory, PricingGuideDto pricingGuideDto)
        {
            var unitCapPerStation = pricingGuideDto.OpenMarketPricing.UnitCapPerStation ?? 0;
            var budgetGoal = pricingGuideDto.BudgetGoal ?? Decimal.MaxValue;
            var impressionsGoal = pricingGuideDto.ImpressionGoal ?? Double.MaxValue;
            var cpmTarget = pricingGuideDto.OpenMarketPricing.OpenMarketCpmTarget ?? OpenMarketCpmTarget.Min;
            var markets = pricingGuideOpenMarketInventory.Markets;
            var shouldMaintainManuallyEditedSpots = pricingGuideDto.MaintainManuallyEditedSpots;

            if (pricingGuideDto.BudgetGoal.HasValue)
                budgetGoal *= pricingGuideDto.OpenMarketShare;
            if (pricingGuideDto.ImpressionGoal.HasValue)
                impressionsGoal *= (double)pricingGuideDto.OpenMarketShare;

            if (shouldMaintainManuallyEditedSpots)
            {
                // We extract budget and impressions of edited manually spots from the goal
                var programsWithEditedManuallySpots = pricingGuideOpenMarketInventory.Markets
                    .Where(x => x.HasEditedManuallySpots)
                    .SelectMany(x => x.Stations)
                    .SelectMany(x => x.Programs)
                    .Where(x => x.SpotsEditedManually);

                foreach (var program in programsWithEditedManuallySpots)
                {
                    _CalculateImpressionsAndCost(program);
                    impressionsGoal -= program.Impressions;
                    budgetGoal -= program.Cost;
                }

                if (_IsGoalAchieved(budgetGoal, impressionsGoal))
                    return;

                markets = markets.Where(x => !x.HasEditedManuallySpots).ToList();
            }

            // We allocate one spot for a program in each market         
            foreach (var market in markets)
            {
                var program = _GetProgramForTarget(market.Stations, cpmTarget, shouldMaintainManuallyEditedSpots);

                if (program == null)
                    continue;
                if (_IsGoalAchieved(budgetGoal, impressionsGoal))
                    break;

                _SetOneSpotProgram(program);
                impressionsGoal -= program.EffectiveImpressionsPerSpot;
                budgetGoal -= program.CostPerSpot;
            }

            // We allocate the rest of the budget one or multiple programs, taking the unit cap per station into consideration.
            var allocatableProgram = _GetNextGoalAllocatableProgram(pricingGuideOpenMarketInventory.Markets, unitCapPerStation, cpmTarget, shouldMaintainManuallyEditedSpots);

            while (!_IsGoalAchieved(budgetGoal, impressionsGoal) && allocatableProgram != null)
            {
                allocatableProgram.Spots = allocatableProgram.Spots + 1;
                _CalculateImpressionsAndCost(allocatableProgram);
                impressionsGoal -= allocatableProgram.EffectiveImpressionsPerSpot;
                budgetGoal -= allocatableProgram.CostPerSpot;
                allocatableProgram = _GetNextGoalAllocatableProgram(pricingGuideOpenMarketInventory.Markets, unitCapPerStation, cpmTarget, shouldMaintainManuallyEditedSpots);
            }
        }

        private bool _IsGoalAchieved(decimal budgetGoal, double impressionsGoal)
        {
            return budgetGoal <= 0 || impressionsGoal <= 0;
        }

        private PricingGuideProgramDto _GetNextGoalAllocatableProgram(List<PricingGuideMarketDto> markets, int stationCap, OpenMarketCpmTarget cpmTarget, bool shouldMaintainManuallyEditedSpots)
        {
            IEnumerable<PricingGuideStationDto> allocatableStations;

            if (stationCap > 0)
            {
                allocatableStations = markets.SelectMany(m => m.Stations).Where(
                s => s.Programs.Sum(p => p.Spots) < stationCap);
            }
            else
            {
                allocatableStations = markets.SelectMany(m => m.Stations);
            }

            var program = _GetProgramForTarget(allocatableStations.ToList(), cpmTarget, shouldMaintainManuallyEditedSpots);

            return program;
        }

        private PricingGuideProgramDto _GetProgramForTarget(List<PricingGuideStationDto> stations, OpenMarketCpmTarget cpmTarget, bool shouldMaintainManuallyEditedSpots)
        {
            if (cpmTarget == OpenMarketCpmTarget.Max)
            {
                return _GetMaxProgram(stations, shouldMaintainManuallyEditedSpots);
            }

            if (cpmTarget == OpenMarketCpmTarget.Avg)
            {
                return _GetAvgProgram(stations, shouldMaintainManuallyEditedSpots);
            }

            return _GetMinProgram(stations, shouldMaintainManuallyEditedSpots);
        }

        private PricingGuideProgramDto _GetAvgProgram(List<PricingGuideStationDto> stations, bool shouldMaintainManuallyEditedSpots)
        {
            var marketPrograms = stations.SelectMany(s => s.Programs);

            if (shouldMaintainManuallyEditedSpots)
            {
                marketPrograms = marketPrograms.Where(x => !x.SpotsEditedManually);
            }

            var avgPrograms = marketPrograms.Where(x => x.BlendedCpm != 0);
            var count = avgPrograms.Count();

            if (count == 0)
                return null;

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

            return programClosestToAvg;
        }

        private PricingGuideProgramDto _GetMaxProgram(List<PricingGuideStationDto> stations, bool shouldMaintainManuallyEditedSpots)
        {
            var marketPrograms = stations.SelectMany(s => s.Programs);

            if (shouldMaintainManuallyEditedSpots)
            {
                marketPrograms = marketPrograms.Where(x => !x.SpotsEditedManually);
            }

            return marketPrograms.Where(x => x.BlendedCpm != 0).OrderByDescending(x => x.BlendedCpm).FirstOrDefault();
        }

        private PricingGuideProgramDto _GetMinProgram(List<PricingGuideStationDto> stations, bool shouldMaintainManuallyEditedSpots)
        {
            var marketPrograms = stations.SelectMany(s => s.Programs);
            
            if (shouldMaintainManuallyEditedSpots)
            {
                marketPrograms = marketPrograms.Where(x => !x.SpotsEditedManually);
            }
            
            return marketPrograms.Where(x => x.BlendedCpm != 0).OrderBy(x => x.BlendedCpm).FirstOrDefault();
        }

        private void _RemoveEmptyStationsAndMarkets(PricingGuideOpenMarketInventory pricingGuideOpenMarketInventory)
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
            }
        }

        private MarketCoverageDto _GetMarketCoverages(IEnumerable<IInventoryMarket> inventoryMarkets)
        {
            var marketsList = inventoryMarkets.ToList();

            var marketCoverageRepository = BroadcastDataRepositoryFactory.GetDataRepository<IMarketCoverageRepository>();

            return marketCoverageRepository.GetLatestMarketCoverages(marketsList.Select(x => x.MarketId));
        }

        private void _ApplyInventoryMarketCoverages(IEnumerable<IInventoryMarket> inventoryMarkets, MarketCoverageDto marketCoverageDto)
        {
            var marketsList = inventoryMarkets.ToList();

            foreach (var inventoryMarket in marketsList)
            {
                marketCoverageDto.MarketCoveragesByMarketCode.TryGetValue(inventoryMarket.MarketId, out var coverage);
                inventoryMarket.MarketCoverage = coverage;
            }
        }

        private void _SetSelectedMarketsInAllMarketsObject(List<PricingGuideMarketDto> markets, List<PricingGuideMarketTotalsDto> allMarkets)
        {
            foreach (var selectedMarket in markets)
            {
                var market = allMarkets.Single(x => x.Id == selectedMarket.MarketId);
                market.Selected = true;
            }
        }

        private List<PricingGuideMarketDto> _FilterMarketsByIds(List<PricingGuideMarketDto> markets, List<int> marketIds)
        {
            return marketIds == null || !marketIds.Any()
                ? markets
                : markets.Where(x => marketIds.Contains(x.MarketId)).ToList();
        }

        private PricingGuideOpenMarketInventoryRequestDto _MapToPricingGuideOpenMarketInventoryRequestDto(PricingGuideDto dto)
        {
            return new PricingGuideOpenMarketInventoryRequestDto
            {
                ProposalDetailId = dto.ProposalDetailId,
                BudgetGoal = dto.BudgetGoal,
                ImpressionGoal = dto.ImpressionGoal,
                OpenMarketPricing = dto.OpenMarketPricing,
                MaintainManuallyEditedSpots = dto.MaintainManuallyEditedSpots
            };
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
                    ProgramId = _GetProgramIdForCheapestProgram(p.ToList()),
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

        private int _GetProgramIdForCheapestProgram(List<ProposalProgramDto> programs)
        {
            var nonZeroCpmPrograms = programs.Where(x => x.TargetCpm != 0);

            if (!nonZeroCpmPrograms.Any())
                return programs.First().ManifestId;

            return nonZeroCpmPrograms.OrderBy(x => x.TargetCpm).First().ManifestId;
        }

        private List<ProposalProgramDto> _GetPricingGuidePrograms(PricingGuideOpenMarketInventory inventory, PricingGuideDto pricingGuideDto)
        {
            var programs = _GetPrograms(inventory);
            _FilterProgramsByDaypart(inventory, programs);
            ApplyDaypartNames(programs);
            _ApplyProjectedImpressions(programs, inventory);
            ApplyStationImpressions(programs, inventory);
            _CalculateIndexingForPrograms(programs, pricingGuideDto);
            _CalculateProgramsCost(programs);
            _CalculateProgramsBlendedCpm(programs);
            _CalculateProgramsTotals(programs);
            return programs;
        }

        private void _CalculateIndexingForPrograms(List<ProposalProgramDto> programs, PricingGuideDto pricingGuideDto)
        {
            var hasIndexingAdjustment = pricingGuideDto.Margin.HasValue ||
                                        pricingGuideDto.Inflation.HasValue ||
                                        pricingGuideDto.ImpressionLoss.HasValue;

            if (!hasIndexingAdjustment)
                return;

            foreach (var program in programs)
            {
                if (pricingGuideDto.Margin.HasValue || pricingGuideDto.Inflation.HasValue)
                {
                    foreach(var flightWeek in program.FlightWeeks)
                    {
                        if (pricingGuideDto.Margin.HasValue)
                        {
                            var marginIncrease = flightWeek.Rate * (decimal)pricingGuideDto.Margin.Value;
                            flightWeek.Rate += marginIncrease;
                        }

                        if (pricingGuideDto.Inflation.HasValue)
                        {
                            var inflationIncrease = flightWeek.Rate * (decimal)pricingGuideDto.Inflation.Value;
                            flightWeek.Rate += inflationIncrease;
                        }
                    }
                }

                if (pricingGuideDto.ImpressionLoss.HasValue)
                {
                    var unitImpressionsDecrease = program.UnitImpressions * pricingGuideDto.ImpressionLoss.Value;

                    program.UnitImpressions -= unitImpressionsDecrease;
                }
            }
        }

        private void _CalculateProgramsCost(List<ProposalProgramDto> programs)
        {
            _ProposalProgramsCalculationEngine.CalculateAvgCostForPrograms(programs);
        }

        private void _CalculateProgramsBlendedCpm(List<ProposalProgramDto> programs)
        {
            _ProposalProgramsCalculationEngine.CalculateBlendedCpmForProgramsRaw(programs);
        }

        private void _CalculateProgramsTotals(List<ProposalProgramDto> programs)
        {
            _ProposalProgramsCalculationEngine.CalculateTotalCostForPrograms(programs);
            _ProposalProgramsCalculationEngine.CalculateTotalImpressionsForPrograms(programs);
        }

        private void _FilterProgramsByDaypart(ProposalDetailInventoryBase pricingGuideOpenMarketDto, List<ProposalProgramDto> programs)
        {
            if (pricingGuideOpenMarketDto.DetailDaypartId == null)
                return;

            var proposalDetailDaypart = _DaypartCache.GetDisplayDaypart(pricingGuideOpenMarketDto.DetailDaypartId.Value);

            programs.RemoveAll(p =>
            {
                return
                    p.ManifestDayparts.All(
                        d => !_DaypartCache.GetDisplayDaypart(d.DaypartId).Intersects(proposalDetailDaypart));
            });
        }

        private List<ProposalProgramDto> _GetPrograms(ProposalDetailInventoryBase inventory)
        {
            var proposalMarketIds = _ProposalMarketsCalculationEngine.GetProposalMarketsList(inventory.ProposalId, inventory.ProposalVersion).Select(m => m.Id).ToList();
            var programs = _StationProgramRepository.GetStationProgramsForProposalDetail(inventory.DetailFlightStartDate,
                                                                                        inventory.DetailFlightEndDate,
                                                                                        inventory.DetailSpotLengthId,
                                                                                        BroadcastConstants.OpenMarketSourceId,
                                                                                        proposalMarketIds);

            SetFlightWeeks(programs);

            return programs;
        }

        private void _ValidateProprietaryPricing(List<ProprietaryPricingDto> proprietaryPricingList)
        {
            var proprietaryInventorySources = EnumHelper.GetProprietaryInventorySources();

            foreach (var proprietaryPricingDto in proprietaryPricingList)
            {
                if (!proprietaryInventorySources.Contains(proprietaryPricingDto.InventorySource))
                    throw new Exception($"Cannot save proposal detail that contains invalid inventory source for proprietary pricing: {proprietaryPricingDto.InventorySource}");
            }

            foreach (var proprietaryPricingInventorySource in proprietaryInventorySources)
            {
                if (proprietaryPricingList.Count(g => g.InventorySource == proprietaryPricingInventorySource) > 1)
                    throw new Exception("Cannot save proposal detail that contains duplicated inventory sources in proprietary pricing data");
            }
        }

        public bool HasSpotsAllocated(int proposalDetailId)
        {
            var openMarketInventoryRepository = 
                BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>();

            var existingAllocations =
                openMarketInventoryRepository.GetProposalDetailAllocations(proposalDetailId);

            return existingAllocations.Any();
        }

        public bool CopyPricingGuideAllocationsToOpenMarket(int proposalDetailId)
        {
            var proposal = _ProposalRepository.GetProposalByDetailId(proposalDetailId);

            if (proposal.Status != ProposalEnums.ProposalStatusType.AgencyOnHold)
                throw new Exception("Proposal must be in Agency on Hold status to copy the pricing model");

            var pricingGuide = GetPricingGuideForProposalDetail(proposalDetailId);

            if (pricingGuide.DistributionId == 0)
                throw new Exception("Pricing guide must be saved to copy spots");

            var proposalDetailWeeks = _ProposalRepository.GetProposalDetailWeeks(proposalDetailId);
            var pricingGuidePrograms = pricingGuide.Markets.SelectMany(x => x.Stations).SelectMany(x => x.Programs).Where(x => x.Spots != 0).ToList();
            var stationProgramRepository = BroadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            var programs = stationProgramRepository.GetStationPrograms(pricingGuidePrograms.Select(x => x.ProgramId).ToList());

            foreach (var program in programs)
            {
                var pricingGuideProgram = pricingGuidePrograms.First(x => x.ProgramId == program.ManifestId);
                program.TotalSpots = pricingGuideProgram.Spots;
                program.TotalImpressions = pricingGuideProgram.DisplayStationImpressions == 0 ? 
                    pricingGuideProgram.DisplayImpressions : pricingGuideProgram.DisplayStationImpressions;
                program.SpotCost = pricingGuideProgram.CostPerSpot;
                program.UnitImpressions = pricingGuideProgram.ImpressionsPerSpot;
            }

            SetFlightWeeks(programs);

            var request = new OpenMarketAllocationSaveRequest
            {
                ProposalVersionDetailId = proposalDetailId,
                Weeks = proposalDetailWeeks.Where(x => !x.IsHiatus).Select(x => new OpenMarketAllocationWeek()
                {
                    MediaWeekId = x.MediaWeekId,
                    Programs = new List<OpenMarketAllocationWeekProgram>()
                }).ToList()
            };

            foreach (var week in request.Weeks)
            {
                foreach (var program in programs)
                {
                    var programFlightweek =
                              program.FlightWeeks.SingleOrDefault(
                                  f => f.MediaWeekId == week.MediaWeekId && f.IsHiatus == false);

                    if (programFlightweek == null)
                        continue;

                    week.Programs.Add(new OpenMarketAllocationWeekProgram()
                    {
                        ProgramId = program.ManifestId,
                        Spots = program.TotalSpots,
                        TotalImpressions = program.TotalImpressions,
                        UnitCost = program.SpotCost,
                        UnitImpressions = program.UnitImpressions
                    });
                }
            }

            _ProposalOpenMarketInventoryService.AllocateOpenMarketSpots(request);

            return true;
        }
    }
}
