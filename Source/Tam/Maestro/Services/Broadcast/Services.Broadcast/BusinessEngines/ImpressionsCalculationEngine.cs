using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Repositories;
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
        private readonly IImpressionAdjustmentEngine _ImpressionAdjustmentEngine;
        private readonly ISpotLengthEngine _SpotLengthEngine;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly IRatingForecastRepository _RatingForecastRepository;

        public ImpressionsCalculationEngine(
            IDataRepositoryFactory broadcastDataRepositoryFactory,
            IImpressionAdjustmentEngine impressionAdjustmentEngine,
            ISpotLengthEngine spotLengthEngine)
        {
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
                    var stationDaypart = new ManifestDetailDaypart
                    {
                        LegacyCallLetters = program.Station.LegacyCallLetters,
                        Id = manifestDaypart.Id,
                        DisplayDaypart = manifestDaypart.Daypart
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

            if (impressionsRequest.HutProjectionBookId.HasValue)
            {
                impressions = _RatingForecastRepository.GetImpressionsDaypart(
                    (short)impressionsRequest.HutProjectionBookId.Value,
                    (short)impressionsRequest.ShareProjectionBookId.Value,
                    ratingAudiences,
                    impressionRequests,
                    impressionsRequest.PlaybackType).Impressions;
            }
            else
            {
                impressions = _RatingForecastRepository.GetImpressionsDaypart(
                    impressionsRequest.ShareProjectionBookId.Value,
                    ratingAudiences,
                    impressionRequests,
                    impressionsRequest.PlaybackType).Impressions;
            }

            var ratingAdjustmentMonth = impressionsRequest.HutProjectionBookId ?? impressionsRequest.ShareProjectionBookId.Value;

            impressions.ForEach(i => i.Impressions = _ImpressionAdjustmentEngine.AdjustImpression(i.Impressions, impressionsRequest.Equivalized, spotLength, impressionsRequest.PostType, ratingAdjustmentMonth, false));

            return impressions;
        }
    }
}
