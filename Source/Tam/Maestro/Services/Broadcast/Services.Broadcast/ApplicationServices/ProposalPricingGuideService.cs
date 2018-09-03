using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Repositories;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProposalPricingGuideService : IApplicationService
    {
        PricingGuideOpenMarketDto GetPricingGuideOpenMarketInventory(int proposalDetailId);
    }

    public class PricingGuideOpenMarketDto : IHaveSingleSharedPostingBooks
    {
        public int ProposalId { get; set; }
        public int ProposalVersion { get; set; }
        public int ProposalDetailId { get; set; }
        public DateTime ProposalDetailFlightStartDate { get; set; }
        public DateTime ProposalDetailFlightEndDate { get; set; }
        public int ProposalDetailSpotLengthId { get; set; }
        public List<PricingGuideMarket> Markets { get; set; } = new List<PricingGuideMarket>();
        public int? SingleProjectionBookId { get; set; }
        public int? ShareProjectionBookId { get; set; }
    }

    public class PricingGuideMarket
    {
        public string MarketName { get; set; }
        public int MarketId { get; set; }
        public double MarketCoverage { get; set; }
        public int MarketRank { get; set; }
        public List<PricingGuideStation> Stations { get; set; } = new List<PricingGuideStation>();
    }

    public class PricingGuideStation
    {
        public int StationCode { get; set; }
        public string CallLetters { get; set; }
        public string LegacyCallLetters { get; set; }
        public string Affiliation { get; set; }
        public List<PricingGuideProgram> Programs { get; set; } = new List<PricingGuideProgram>();
    }

    public class PricingGuideProgram
    {
        public int ProgramId { get; set; }
        public List<string> ProgramNames { get; set; } = new List<string>();
        public List<LookupDto> Dayparts { get; set; } = new List<LookupDto>();
        public decimal TargetCpm { get; set; }
        public double TargetImpressions { get; set; }
        public double UnitImpressions { get; set; }
        public List<LookupDto> Genres { get; set; } = new List<LookupDto>();
        public int Spots { get; set; }
        public List<ProposalProgramFlightWeek> FlightWeeks { get; set; } = new List<ProposalProgramFlightWeek>();
        public short StationCode { get; set; }
    }

    public class ProposalPricingGuideGuideService : IProposalPricingGuideService
    {
        protected readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        protected readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IProposalProgramsCalculationEngine _ProposalProgramsCalculationEngine;

        public ProposalPricingGuideGuideService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IProposalProgramsCalculationEngine proposalProgramsCalculationEngine)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _ProposalProgramsCalculationEngine = proposalProgramsCalculationEngine;

        }

        public PricingGuideOpenMarketDto GetPricingGuideOpenMarketInventory(int proposalDetailId)
        {
            var proposalRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
            var stationProgramRepository =
                _BroadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            var pricingGuideDto = proposalRepository.GetPricingGuideRepresentionalWeek(proposalDetailId);
            var proposalMarketIds = _ProposalMarketsCalculationEngine
                .GetProposalMarketsList(pricingGuideDto.ProposalId, pricingGuideDto.ProposalVersion, pricingGuideDto.ProposalDetailId)
                .Select(m => (short)m.Id).ToList();
            var programs = stationProgramRepository
                .GetStationProgramsForProposalDetail(pricingGuideDto.ProposalDetailFlightStartDate, pricingGuideDto.ProposalDetailFlightEndDate,
                    pricingGuideDto.ProposalDetailSpotLengthId, BroadcastConstants.OpenMarketSourceId, proposalMarketIds, pricingGuideDto.ProposalDetailId);

            _SetFlightWeeks(programs);

            _ProposalProgramsCalculationEngine.CalculateBlendedCpmForPrograms(programs, pricingGuideDto.ProposalDetailSpotLengthId);

            var inventoryMarkets = _GroupProgramsByMarketAndStation(programs);
            var postingBook = PropoeralsServiceHelper.GetBookId(pricingGuideDto);

            _ApplyInventoryMarketRankings(postingBook, inventoryMarkets);

            pricingGuideDto.Markets.AddRange(inventoryMarkets.OrderBy(m => m.MarketRank).ToList());

            return pricingGuideDto;
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

        private void _ApplyInventoryMarketRankings(int mediaMonthId, IEnumerable<PricingGuideMarket> inventoryMarkets)
        {
            var marketRankings =
                _BroadcastDataRepositoryFactory.GetDataRepository<INsiMarketRepository>()
                    .GetMarketRankingsByMediaMonth(mediaMonthId);

            foreach (var inventoryMarket in inventoryMarkets)
            {
                marketRankings.TryGetValue(inventoryMarket.MarketId, out var rank);
                inventoryMarket.MarketRank = rank;
            }
        }

        private List<PricingGuideMarket> _GroupProgramsByMarketAndStation(IEnumerable<ProposalProgramDto> programs)
        {
            var programsByMarket = programs.GroupBy(p => p.Market.Id);
            var inventoryMarkets = programsByMarket.Select(
                g => new PricingGuideMarket
                {
                    MarketId = g.Key,
                    MarketName = g.First().Market.Display,
                    Stations = g.GroupBy(p => p.Station.StationCode).Select(s => new PricingGuideStation
                    {
                        Affiliation = s.First().Station.Affiliation,
                        CallLetters = s.First().Station.CallLetters,
                        LegacyCallLetters = s.First().Station.LegacyCallLetters,
                        StationCode = s.First().Station.StationCode,
                        Programs = s.Select(p => new PricingGuideProgram
                        {
                            ProgramId = p.ManifestId,
                            ProgramNames = p.ManifestDayparts.Select(md => md.ProgramName).ToList(),
                            TargetCpm = p.TargetCpm,
                            UnitImpressions = p.UnitImpressions,
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
    }
}
