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
        void ApplyProjectedImpressions(
            IEnumerable<PlanPricingInventoryProgram> programs, 
            ImpressionsRequestDto impressionsRequest, 
            int audienceId, 
            bool isProprietary);
        void ApplyProvidedImpressions(List<PlanPricingInventoryProgram> programs, int audienceId, int spotLengthId, bool equivalized);
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
            foreach (var program in programs)
            {
                var manifestAudienceForProgram = program.ManifestAudiences.FirstOrDefault(x => x.AudienceId == audienceId && x.IsReference);
                var hasProvidedImpressions = manifestAudienceForProgram != null && manifestAudienceForProgram.Impressions.HasValue;

                if (hasProvidedImpressions)
                {
                    var spotLength = _SpotLengthEngine.GetSpotLengthValueById(spotLengthId);

                    program.ProvidedImpressions = _ImpressionAdjustmentEngine.AdjustImpression(manifestAudienceForProgram.Impressions.Value, equivalized, spotLength);
                }
            }
        }

        public void ApplyProjectedImpressions(
            IEnumerable<PlanPricingInventoryProgram> programs, 
            ImpressionsRequestDto impressionsRequest, 
            int audienceId,
            bool isProprietary)
        {
            var impressionRequests = new List<ManifestDetailDaypart>();
            var manifestDaypartImpressions = new Dictionary<int, double>();
            var manifestDaypartSpotLengthIds = new Dictionary<int, int>();

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
                    manifestDaypartSpotLengthIds[manifestDaypart.Id] = program.SpotLengthId;
                }
            }

            var ratingAudiences = _BroadcastAudienceRepository.GetRatingsAudiencesByMaestroAudience(
                    new List<int>
                    {
                        audienceId
                    }).Select(r => r.rating_audience_id).Distinct().ToList();

            var programImpressions = _GetImpressions(impressionsRequest, ratingAudiences, impressionRequests);
            _AdjustImpressions(impressionsRequest, programImpressions, manifestDaypartSpotLengthIds, isProprietary);

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

        private void _AdjustImpressions(
            ImpressionsRequestDto impressionsRequest,
            List<StationImpressionsWithAudience> programImpressions,
            Dictionary<int, int> programSpotLengthIds,
            bool isProprietary)
        {
            var ratingAdjustmentMonth = impressionsRequest.HutProjectionBookId ?? impressionsRequest.ShareProjectionBookId.Value;

            foreach (var program in programImpressions)
            {
                // for Proprietary we always use the spot length, inventory has been uploaded with
                // for OpenMarket
                // if pricing version is 2, we use the first SpotLengthId from a plan
                // if pricing version is 3, SpotLengthId does not matter because we do not do the equivalization, it`s done by the DS endpoint
                var spotLengthId = isProprietary ? programSpotLengthIds[program.Id] : impressionsRequest.SpotLengthId;
                var spotLength = _SpotLengthEngine.GetSpotLengthValueById(spotLengthId);

                program.Impressions = _ImpressionAdjustmentEngine.AdjustImpression(
                    program.Impressions,
                    impressionsRequest.Equivalized,
                    spotLength,
                    impressionsRequest.PostType,
                    ratingAdjustmentMonth,
                    applyAnnualAdjustment: false);
            }
        }

        private List<StationImpressionsWithAudience> _GetImpressions(ImpressionsRequestDto impressionsRequest, List<int> ratingAudiences, List<ManifestDetailDaypart> impressionRequests)
        {
            if (impressionsRequest.HutProjectionBookId.HasValue)
            {
                return _RatingForecastRepository.GetImpressionsDaypart(
                    (short)impressionsRequest.HutProjectionBookId.Value,
                    (short)impressionsRequest.ShareProjectionBookId.Value,
                    ratingAudiences,
                    impressionRequests,
                    impressionsRequest.PlaybackType).Impressions;
            }
            else
            {
                return _RatingForecastRepository.GetImpressionsDaypart(
                    impressionsRequest.ShareProjectionBookId.Value,
                    ratingAudiences,
                    impressionRequests,
                    impressionsRequest.PlaybackType).Impressions;
            }
        }
    }
}
