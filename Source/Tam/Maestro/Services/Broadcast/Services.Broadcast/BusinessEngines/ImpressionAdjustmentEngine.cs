using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.BusinessEngines
{
    public interface IImpressionAdjustmentEngine : IApplicationService
    {
        double AdjustImpression(double impression, SchedulePostType? postType, int schedulePostingBook, bool applyAnnualAdjustment = true);
        double AdjustImpression(double impression, bool? isEquivilized, int spotLength, SchedulePostType? postType, int schedulePostingBook, bool applyAnnualAdjustment = true);
        double AdjustImpression(double impression, bool? isEquivalized, int spotLength);
    }

    public class ImpressionAdjustmentEngine : IImpressionAdjustmentEngine
    {
        private readonly Lazy<Dictionary<int, float>> _SpotLengthMultipliers;
        private readonly Lazy<Dictionary<int, RatingAdjustmentsDto>> _RatingAdjustments;

        public ImpressionAdjustmentEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _SpotLengthMultipliers = new Lazy<Dictionary<int, float>>(() => broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthMultipliers());
            _RatingAdjustments = new Lazy<Dictionary<int, RatingAdjustmentsDto>>(() => broadcastDataRepositoryFactory.GetDataRepository<IRatingAdjustmentsRepository>().GetRatingAdjustments().ToDictionary(ra => ra.MediaMonthId));
        }

        public double AdjustImpression(double impression, SchedulePostType? postType, int schedulePostingBook, bool applyAnnualAdjustment = true)
        {
            var result = impression;

            RatingAdjustmentsDto adjustments;
            if (_RatingAdjustments.Value.TryGetValue(schedulePostingBook, out adjustments))
            {
                if (applyAnnualAdjustment)
                {
                    result = result * (double)(1 - adjustments.AnnualAdjustment / 100);
                }
                if (postType == SchedulePostType.NTI)
                {
                    result = result * (double)(1 - adjustments.NtiAdjustment / 100);
                }
            }

            return result;
        }

        public double AdjustImpression(double impression, bool? isEquivilized, int spotLength, SchedulePostType? postType, int schedulePostingBook, bool applyAnnualAdjustment = true)
        {
            var result = AdjustImpression(impression, postType, schedulePostingBook, applyAnnualAdjustment);

            if (isEquivilized == true)
            {
                float multiplier;
                if (_SpotLengthMultipliers.Value.TryGetValue(spotLength, out multiplier))
                {
                    return result * multiplier;
                }

                throw new ApplicationException(string.Format("Unknown spot length {0} found while adjusting impression", spotLength));
            }

            return result;
        }

        public double AdjustImpression(double impression, bool? isEquivalized, int spotLength)
        {
            if (isEquivalized == true)
            {
                float multiplier;
                if (_SpotLengthMultipliers.Value.TryGetValue(spotLength, out multiplier))
                {
                    return impression * multiplier;
                }

                throw new ApplicationException(string.Format("Unknown spot length {0} found while adjusting impression", spotLength));
            }

            return impression;
        }
    }
}
