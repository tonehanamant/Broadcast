using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO.PricingGuide;
using Services.Broadcast.Entities.OpenMarketInventory;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices
{
    public class BaseProposalInventoryService
    {
        internal static string MissingBooksErrorMessage = "Unable to get proprietary inventory information due. Proposal must have both Share and Hut Book or Sweeps Book defined.";
        protected readonly IDataRepositoryFactory BroadcastDataRepositoryFactory;
        private readonly IDaypartCache DaypartCache;
        protected readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine;
        protected readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IProposalTotalsCalculationEngine _ProposalTotalsCalculationEngine;

        public BaseProposalInventoryService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache, IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalTotalsCalculationEngine proposalTotalsCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IImpressionAdjustmentEngine impressionAdjustmentEngine)
        {
            BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            DaypartCache = daypartCache;
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _ProposalTotalsCalculationEngine = proposalTotalsCalculationEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
        }

        protected IEnumerable<StationImpressions> GetImpressions(ProposalDetailInventoryBase proposalDetailInventory, List<int> ratingAudiences, List<ManifestDetailDaypart> impressionRequests)
        {
            var repository = BroadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
            List<StationImpressionsWithAudience> impressions = null;

            if (proposalDetailInventory.ShareProjectionBookId.HasValue && proposalDetailInventory.HutProjectionBookId.HasValue)
            {
                impressions = repository.GetImpressionsDaypart(
                    (short)proposalDetailInventory.HutProjectionBookId.Value, 
                    (short)proposalDetailInventory.ShareProjectionBookId.Value, 
                    ratingAudiences, 
                    impressionRequests, 
                    proposalDetailInventory.PlaybackType).Impressions;
            }
            else if (proposalDetailInventory.SingleProjectionBookId.HasValue)
            {
                impressions = repository.GetImpressionsDaypart(
                    proposalDetailInventory.SingleProjectionBookId.Value,
                    ratingAudiences,
                    impressionRequests,
                    proposalDetailInventory.PlaybackType).Impressions;
            }

            if (impressions != null)
            {
                var ratingAdjustmentMonth = GetRatingAdjustmentMonth(proposalDetailInventory);

                impressions.ForEach(i => i.Impressions = _ImpressionAdjustmentEngine.AdjustImpression(i.Impressions, proposalDetailInventory.Equivalized, proposalDetailInventory.DetailSpotLength, proposalDetailInventory.PostType, ratingAdjustmentMonth, false));

                return impressions;
            }

            throw new ApplicationException(MissingBooksErrorMessage);
        }

        internal static int GetRatingAdjustmentMonth(ProposalDetailInventoryBase proposalDetailInventory)
        {
            int ratingAdjustmentMonth;
            if (proposalDetailInventory.HutProjectionBookId.HasValue && proposalDetailInventory.ShareProjectionBookId.HasValue)
                ratingAdjustmentMonth = proposalDetailInventory.HutProjectionBookId.Value;
            else
                ratingAdjustmentMonth = proposalDetailInventory.SingleProjectionBookId.Value;
            return ratingAdjustmentMonth;
        }

        protected void _SetProposalInventoryDetailDaypart(ProposalDetailInventoryBase proposalInventory)
        {
            if (proposalInventory == null || !proposalInventory.DetailDaypartId.HasValue) return;
            proposalInventory.DetailDaypart = DaypartDto.ConvertDisplayDaypart(DaypartCache.GetDisplayDaypart(proposalInventory.DetailDaypartId.Value));
        }

        protected void _UpdateProposalTotals(int proposalVersionId)
        {
            var allProposalDetailsTotals = BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                        .GetAllProposalDetailsTotals(proposalVersionId);
            var proposalTotals = _ProposalTotalsCalculationEngine.SumAllDetailsTotals(allProposalDetailsTotals);
            BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>()
                .SaveProposalTotals(proposalVersionId, proposalTotals);
        }

        protected void SetProposalInventoryDetailSpotLength(ProposalDetailInventoryBase proposalInventory)
        {
            if (proposalInventory == null) return;
            proposalInventory.DetailSpotLength = BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthById(proposalInventory.DetailSpotLengthId);
        }

        protected void SetFlightWeeks(IEnumerable<ProposalProgramDto> programs)
        {
            if (programs.Any())
            {
                var startDate = programs.Min(p => p.StartDate);
                var endDate = programs.Max(p => p.EndDate) ?? DateTime.MaxValue;

                var mediaWeeksToUse = _MediaMonthAndWeekAggregateCache.GetMediaWeeksByFlight(startDate, endDate);

                foreach (var program in programs)
                {
                    program.FlightWeeks = _GetFlightWeeks(program, mediaWeeksToUse);
                }
            }
        }

        protected static List<PricingGuideMarketDto> ApplyDefaultSortingForPricingGuideMarkets(List<PricingGuideMarketDto> markets)
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

        protected void ApplyInventoryMarketRankings(IInventoryMarket inventoryMarket, Dictionary<int, int> marketRankings)
        {
            marketRankings.TryGetValue(inventoryMarket.MarketId, out var rank);
            inventoryMarket.MarketRank = rank;
        }
        
        protected void _ApplyProjectedImpressions(IEnumerable<ProposalProgramDto> programs, ProposalDetailInventoryBase proposalDetail)
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

            var ratingAudiences = BroadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>().GetRatingsAudiencesByMaestroAudience(
                    new List<int>
                    {
                        proposalDetail.GuaranteedAudience.Value
                    }).Select(r => r.rating_audience_id).Distinct().ToList();

            var programImpressions = GetImpressions(proposalDetail, ratingAudiences, impressionRequests);

            foreach (var imp in programImpressions)
            {
                manifestDaypartImpressions[imp.Id] += imp.Impressions;
            }

            foreach (var program in programs)
            {
                var programManifestDaypartIds = program.ManifestDayparts.Select(d => d.Id).ToList();
                var programDaypartImpressions = programManifestDaypartIds
                    .Where(d => manifestDaypartImpressions.ContainsKey(d))
                    .Select(d => manifestDaypartImpressions[d])
                    .ToList();
                var daypartCount = programManifestDaypartIds.Count;
                if (daypartCount > 0)
                {
                    program.UnitImpressions = programDaypartImpressions.Sum(i => i) / daypartCount;
                }
            }

        }

        protected void ApplyDaypartNames(List<ProposalProgramDto> programs)
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

        protected void ApplyStationImpressions(List<ProposalProgramDto> programs, ProposalDetailInventoryBase proposalDetail)
        {
            foreach (var program in programs)
            {
                var manifestAudienceForProposal =
                    program.ManifestAudiences.SingleOrDefault(x => x.AudienceId == proposalDetail.GuaranteedAudience);
                var hasManifestAudiences = manifestAudienceForProposal != null &&
                                           manifestAudienceForProposal.Impressions.HasValue;

                if (hasManifestAudiences)
                {
                    program.ProvidedUnitImpressions =
                        _ImpressionAdjustmentEngine.AdjustImpression(manifestAudienceForProposal.Impressions.Value, proposalDetail.Equivalized, proposalDetail.DetailSpotLength);
                }
            }
        }

        protected PricingGuideOpenMarketInventory GetPricingGuideInventory(int proposalDetailId)
        {
            var inventory = BroadcastDataRepositoryFactory.GetDataRepository<IPricingGuideRepository>().GetProposalDetailPricingGuideInventory(proposalDetailId);
            SetProposalInventoryDetailSpotLength(inventory);
            return inventory;
        }

        private List<ProposalProgramFlightWeek> _GetFlightWeeks(ProposalProgramDto programDto, List<MediaWeek> mediaWeeksToUse = null)
        {
            var nonNullableEndDate = programDto.EndDate ?? programDto.StartDate.AddYears(1);

            var displayFlighWeeks = _MediaMonthAndWeekAggregateCache.GetDisplayMediaWeekByFlight(programDto.StartDate, nonNullableEndDate, mediaWeeksToUse);

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
    }
}