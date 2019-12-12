using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.BusinessEngines
{
    public interface IImpressionsCalculationEngine : IApplicationService
    {
        void ApplyProjectedImpressions(IEnumerable<PlanPricingInventoryProgram> programs, ImpressionsRequestDto impressionsRequest, int audienceId);
        void ApplyProvidedImpressions(List<PlanPricingInventoryProgram> programs, int audienceId, int spotLengthId, bool equivalized);
        IEnumerable<StationImpressionsWithAudience> GetImpressions(ImpressionsRequestDto impressionsRequest, List<int> ratingAudiences, List<ManifestDetailDaypart> impressionRequests);
    }

    public class ImpressionsCalculationEngine : IImpressionsCalculationEngine
    {
        private const string MissingBooksErrorMessage = "Impressions request must have both Share and Hut Book or Sweeps Book defined.";

        private readonly IDaypartCache _DaypartCache;
        private readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine;
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IProposalTotalsCalculationEngine _ProposalTotalsCalculationEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly IRatingForecastRepository _RatingForecastRepository;

        public ImpressionsCalculationEngine(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache, IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalTotalsCalculationEngine proposalTotalsCalculationEngine,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IImpressionAdjustmentEngine impressionAdjustmentEngine,
            ISpotLengthEngine spotLengthEngine)
        {
            _DaypartCache = daypartCache;
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _ProposalTotalsCalculationEngine = proposalTotalsCalculationEngine;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _ImpressionAdjustmentEngine = impressionAdjustmentEngine;
            _SpotLengthEngine = spotLengthEngine;
            _BroadcastAudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _RatingForecastRepository = broadcastDataRepositoryFactory.GetDataRepository<IRatingForecastRepository>();
        }

        public void ApplyProvidedImpressions(List<PlanPricingInventoryProgram> programs, int audienceId, int spotLengthId, bool equivalized)
        {
            var spotLength = _SpotLengthEngine.GetSpotLengthValueById(spotLengthId);

            foreach (var program in programs)
            {
                var manifestAudienceForProgram = program.ManifestAudiences.FirstOrDefault(x => x.AudienceId == audienceId && x.IsReference);
                var hasProvidedImpressions = manifestAudienceForProgram != null && manifestAudienceForProgram.Impressions.HasValue;

                if (hasProvidedImpressions)
                {
                    program.ProvidedImpressions = _ImpressionAdjustmentEngine.AdjustImpression(manifestAudienceForProgram.Impressions.Value, equivalized, spotLength);
                }
            }
        }

        public void ApplyProjectedImpressions(IEnumerable<PlanPricingInventoryProgram> programs, ImpressionsRequestDto impressionsRequest, int audienceId)
        {
            var impressionRequests = new List<ManifestDetailDaypart>();
            var manifestDaypartImpressions = new Dictionary<int, double>();

            foreach (var program in programs)
            {
                foreach (var manifestDaypart in program.ManifestDayparts)
                {
                    var manifestDisplayDaypart = DaypartCache.Instance.GetDisplayDaypart(manifestDaypart.DaypartId);
                    var stationDaypart = new ManifestDetailDaypart
                    {
                        LegacyCallLetters = program.StationLegacyCallLetters,
                        Id = manifestDaypart.Id,
                        DisplayDaypart = manifestDisplayDaypart
                    };

                    impressionRequests.Add(stationDaypart);

                    manifestDaypartImpressions[manifestDaypart.Id] = 0;
                }
            }

            var ratingAudiences = _BroadcastAudienceRepository.GetRatingsAudiencesByMaestroAudience(
                    new List<int>
                    {
                        audienceId
                    }).Select(r => r.rating_audience_id).Distinct().ToList();

            var programImpressions = GetImpressions(impressionsRequest, ratingAudiences, impressionRequests);

            foreach (var programImpression in programImpressions)
            {
                manifestDaypartImpressions[programImpression.Id] += programImpression.Impressions;
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
                    program.ProjectedImpressions = programDaypartImpressions.Sum(i => i) / daypartCount;
                }
            }
        }

        public IEnumerable<StationImpressionsWithAudience> GetImpressions(ImpressionsRequestDto impressionsRequest, List<int> ratingAudiences, List<ManifestDetailDaypart> impressionRequests)
        {
            List<StationImpressionsWithAudience> impressions = null;
            var spotLength = _SpotLengthEngine.GetSpotLengthValueById(impressionsRequest.SpotLengthId);

            if (impressionsRequest.ShareProjectionBookId.HasValue && impressionsRequest.HutProjectionBookId.HasValue)
            {
                impressions = _RatingForecastRepository.GetImpressionsDaypart(
                    (short)impressionsRequest.HutProjectionBookId.Value,
                    (short)impressionsRequest.ShareProjectionBookId.Value,
                    ratingAudiences,
                    impressionRequests,
                    impressionsRequest.PlaybackType).Impressions;
            }
            else if (impressionsRequest.SingleProjectionBookId.HasValue)
            {
                impressions = _RatingForecastRepository.GetImpressionsDaypart(
                    impressionsRequest.SingleProjectionBookId.Value,
                    ratingAudiences,
                    impressionRequests,
                    impressionsRequest.PlaybackType).Impressions;
            }

            if (impressions != null)
            {
                var ratingAdjustmentMonth = GetRatingAdjustmentMonth(impressionsRequest);

                impressions.ForEach(i => i.Impressions = _ImpressionAdjustmentEngine.AdjustImpression(i.Impressions, impressionsRequest.Equivalized, spotLength, impressionsRequest.PostType, ratingAdjustmentMonth, false));

                return impressions;
            }

            throw new ApplicationException(MissingBooksErrorMessage);
        }

        private int GetRatingAdjustmentMonth(ImpressionsRequestDto impressionsRequest)
        {
            int ratingAdjustmentMonth;
            if (impressionsRequest.HutProjectionBookId.HasValue && impressionsRequest.ShareProjectionBookId.HasValue)
                ratingAdjustmentMonth = impressionsRequest.HutProjectionBookId.Value;
            else
                ratingAdjustmentMonth = impressionsRequest.SingleProjectionBookId.Value;
            return ratingAdjustmentMonth;
        }
    }
}
