using System;
using System.Collections.Generic;
using System.Linq;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Repositories;

namespace Services.Broadcast.BusinessEngines
{
    public interface IImpressionAdjustmentEngine : IApplicationService
    {
        double AdjustImpression(double impression, PostingTypeEnum? postType, int schedulePostingBook, bool applyAnnualAdjustment = true);
        double AdjustImpression(double impression, bool? isEquivilized, int spotLength, PostingTypeEnum? postType, int schedulePostingBook, bool applyAnnualAdjustment = true);

        /// <summary>
        /// Adjust impressions by applying equivalization
        /// </summary>
        /// <param name="impression">Impressions to adjust</param>
        /// <param name="isEquivalized">Equivalized or not</param>
        /// <param name="spotLength">Spot length</param>
        /// <returns>Equivalized impressions</returns>
        double AdjustImpression(double impression, bool? isEquivalized, int spotLength);

        /// <summary>
        /// Adjust impression using the NTI Conversion factor
        /// </summary>
        /// <param name="impressions">Impressions</param>
        /// <param name="ntiConversionFactor">NTI Conversion Factor</param>
        /// <returns>Adjusted impressions</returns>
        double AdjustImpression(double impressions, double ntiConversionFactor);

        /// <summary>
        /// Converts NTI impressions to NSI
        /// </summary>
        /// <param name="impressions">NTI Impressions</param>
        /// <param name="ntiToNsiIncrease">NTI to NSI increase</param>
        /// <returns>NSI impressions</returns>
        double ConvertNtiImpressionsToNsi(double impressions, double ntiToNsiIncrease);
    }

    public class ImpressionAdjustmentEngine : IImpressionAdjustmentEngine
    {
        private readonly Lazy<Dictionary<int, double>> _SpotLengthMultipliers;
        private readonly Lazy<Dictionary<int, int>> _SpotLengths;
        private readonly Lazy<Dictionary<int, RatingAdjustmentsDto>> _RatingAdjustments;

        public ImpressionAdjustmentEngine(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _SpotLengths = new Lazy<Dictionary<int, int>>(() => broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds());
            _SpotLengthMultipliers = new Lazy<Dictionary<int, double>>(() => broadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthMultipliers());
            _RatingAdjustments = new Lazy<Dictionary<int, RatingAdjustmentsDto>>(() => broadcastDataRepositoryFactory.GetDataRepository<IRatingAdjustmentsRepository>().GetRatingAdjustments().ToDictionary(ra => ra.MediaMonthId));
        }

        public double AdjustImpression(double impression, PostingTypeEnum? postType, int schedulePostingBook, bool applyAnnualAdjustment = true)
        {
            var result = impression;

            if (_RatingAdjustments.Value.TryGetValue(schedulePostingBook, out RatingAdjustmentsDto adjustments))
            {
                if (applyAnnualAdjustment)
                {
                    result = result * (double)(1 - adjustments.AnnualAdjustment / 100);
                }
                if (postType == PostingTypeEnum.NTI)
                {
                    result = result * (double)(1 - adjustments.NtiAdjustment / 100);
                }
            }

            return result;
        }

        public double AdjustImpression(double impression, bool? isEquivilized, int spotLength, PostingTypeEnum? postType, int schedulePostingBook, bool applyAnnualAdjustment = true)
        {
            var result = AdjustImpression(impression, postType, schedulePostingBook, applyAnnualAdjustment);

            if (isEquivilized == true)
            {
                if (_SpotLengthMultipliers.Value.TryGetValue(spotLength , out double multiplier))
                {
                    return result * multiplier;
                }

                throw new ApplicationException(string.Format("Unknown spot length {0} found while adjusting impression", spotLength));
            }

            return result;
        }

        /// <summary>
        /// Adjust impressions by applying equivalization
        /// </summary>
        /// <param name="impression">Impressions to adjust</param>
        /// <param name="isEquivalized">Equivalized or not</param>
        /// <param name="spotLength">Spot length</param>
        /// <returns>Equivalized impressions</returns>
        public double AdjustImpression(double impression, bool? isEquivalized, int spotLength)
        {
            if (isEquivalized == true)
            {
                if (_SpotLengthMultipliers.Value.TryGetValue(spotLength, out double multiplier))
                {
                    return impression * multiplier;
                }

                throw new ApplicationException(string.Format("Unknown spot length {0} found while adjusting impression", spotLength));
            }

            return impression;
        }

        /// <summary>
        /// Adjust impression using the NTI Conversion factor
        /// </summary>
        /// <param name="impressions">Impressions</param>
        /// <param name="ntiConversionFactor">NTI Conversion Factor</param>
        /// <returns>Adjusted impressions</returns>
        public double AdjustImpression(double impressions, double ntiConversionFactor)
        {
            return impressions * (1 - ntiConversionFactor);
        }

        public double ConvertNtiImpressionsToNsi(double impressions, double ntiToNsiIncrease)
        {
            return impressions / (1 - ntiToNsiIncrease);
        }
    }
}
