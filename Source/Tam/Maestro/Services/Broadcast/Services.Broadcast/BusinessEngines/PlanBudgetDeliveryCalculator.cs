using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities.Plan;
using System;

namespace Services.Broadcast.BusinessEngines
{
    public interface IPlanBudgetDeliveryCalculator
    {
        /// <summary>
        /// Calculates the budget based on 2 input parameters.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>PlanDeliveryBudget object containing the calculated value</returns>
        PlanDeliveryBudget CalculateBudget(PlanDeliveryBudget input);
    }

    public class PlanBudgetDeliveryCalculator : IPlanBudgetDeliveryCalculator
    {

        private readonly NsiUniverseService _NsiUniverseService;

        public PlanBudgetDeliveryCalculator(NsiUniverseService nsiUniverseService)
        {
            _NsiUniverseService = nsiUniverseService;
        }

        ///<inheritdoc/>
        public PlanDeliveryBudget CalculateBudget(PlanDeliveryBudget input)
        {
            if ((input.Impressions.HasValue && input.Impressions < 0)
                || (input.RatingPoints.HasValue && input.RatingPoints < 0)
                || (input.Budget.HasValue && input.Budget < 0)
                || (input.CPM.HasValue && input.CPM < 0)
                || (input.CPP.HasValue && input.CPP < 0))
            {
                throw new Exception("Invalid budget values passed");
            }

            if (input.MediaMonthId <= 0 || input.AudienceId <= 0)
            {
                throw new Exception("Cannot calculate goal without media month and audience");
            }

            input.Universe = _NsiUniverseService.GetAudienceUniverseForMediaMonth(input.MediaMonthId, input.AudienceId);

            var result = input;

            if (input.Impressions.HasValue && input.Budget.HasValue)
            {
                result.CPM = _CalculateCPM(input.Budget, input.Impressions);
                result.RatingPoints = _CalculateDeliveryRatingPoints(input.Impressions, input.Universe.Value);
                result.CPP = _CalculateCPP(input.Budget, result.RatingPoints);
                return result;
            }

            if (input.Impressions.HasValue && input.CPM.HasValue)
            {
                result.Budget = _CalculateBudgetByCPM(input.Impressions, input.CPM);
                result.RatingPoints = _CalculateDeliveryRatingPoints(input.Impressions, input.Universe.Value);
                result.CPP = _CalculateCPP(result.Budget, result.RatingPoints);
                return result;
            }

            if (input.Budget.HasValue && input.CPM.HasValue)
            {
                result.Impressions = _CalculateDeliveryImpressionsByCPM(input.Budget, input.CPM);
                result.RatingPoints = _CalculateDeliveryRatingPoints(input.Impressions, input.Universe.Value);
                result.CPP = _CalculateCPP(result.Budget, result.RatingPoints);
                return result;
            }

            if (input.RatingPoints.HasValue && input.Budget.HasValue)
            {
                result.Impressions = _CalculateDeliveryImpressionsByUniverse(input.RatingPoints, input.Universe.Value);
                result.CPM = _CalculateCPM(input.Budget, result.Impressions);
                result.CPP = _CalculateCPP(input.Budget, input.RatingPoints);
                return result;
            }

            if (input.RatingPoints.HasValue && input.CPP.HasValue)
            {
                result.Budget = _CalculateBudgetByCPP(input.RatingPoints, input.CPP);
                result.Impressions = _CalculateDeliveryImpressionsByUniverse(input.RatingPoints, input.Universe.Value);
                result.CPM = _CalculateCPM(result.Budget, result.Impressions);
                return result;
            }

            if (input.Budget.HasValue && input.CPP.HasValue)
            {
                result.RatingPoints = _CalculateDeliveryRatingPointsByCPP(input.Budget, input.CPP);
                result.Impressions = _CalculateDeliveryImpressionsByUniverse(input.RatingPoints, input.Universe.Value);
                result.CPM = _CalculateCPM(input.Budget, result.Impressions);
                return result;
            }
            throw new Exception("At least 2 values needed to calculate goal amount");
        }

        private static double? _CalculateDeliveryRatingPoints(double? deliveryImpressions, double universe)
        {
            return (deliveryImpressions / universe) * 100;
        }

        private static decimal _CalculateBudgetByCPP(double? deliveryRatingPoints, decimal? CPP)
        {
            return (decimal)deliveryRatingPoints.Value * CPP.Value;
        }

        private static decimal _CalculateBudgetByCPM(double? deliveryImpressions, decimal? CPM)
        {
            return (decimal)deliveryImpressions.Value / 1000 * CPM.Value;
        }

        private static decimal _CalculateCPP(decimal? budget, double? deliveryRatingPoints)
        {
            return deliveryRatingPoints.Value == 0 ? 0 : budget.Value / (decimal)deliveryRatingPoints.Value;
        }

        private double? _CalculateDeliveryImpressionsByCPM(decimal? budget, decimal? CPM)
        {
            return CPM.Value == 0 ? 0 : (double)(budget.Value / CPM.Value);
        }

        private static double? _CalculateDeliveryImpressionsByUniverse(double? deliveryRatingPoints, double universe)
        {
            return (deliveryRatingPoints * universe) / 100;
        }

        private static decimal _CalculateCPM(decimal? budget, double? deliveryImpressions)
        {
            return deliveryImpressions.Value == 0 ? 0 : (budget.Value / (decimal)deliveryImpressions.Value) * 1000;
        }

        private static double _CalculateDeliveryRatingPointsByCPP(decimal? budget, decimal? cpp)
        {
            return cpp == 0 ? 0 : (double)(budget.Value / cpp.Value);
        }
    }
}
