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
            if ((input.DeliveryImpressions.HasValue && input.DeliveryImpressions < 0)
                || (input.DeliveryRatingPoints.HasValue && input.DeliveryRatingPoints < 0)
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

            double universe = _NsiUniverseService.GetAudienceUniverseForMediaMonth(input.MediaMonthId, input.AudienceId);

            var result = input;

            if (input.DeliveryImpressions.HasValue && input.Budget.HasValue)
            {
                result.CPM = _CalculateCPM(input.Budget, input.DeliveryImpressions);
                result.DeliveryRatingPoints = _CalculateDeliveryRatingPoints(input.DeliveryImpressions, universe);
                result.CPP = _CalculateCPP(input.Budget, result.DeliveryRatingPoints);
                return result;
            }

            if (input.DeliveryImpressions.HasValue && input.CPM.HasValue)
            {
                result.Budget = _CalculateBudgetByCPM(input.DeliveryImpressions, input.CPM);
                result.DeliveryRatingPoints = _CalculateDeliveryRatingPoints(input.DeliveryImpressions, universe);
                result.CPP = _CalculateCPP(result.Budget, result.DeliveryRatingPoints);
                return result;
            }

            if (input.Budget.HasValue && input.CPM.HasValue)
            {
                result.DeliveryImpressions = _CalculateDeliveryImpressionsByCPM(input.Budget, input.CPM);
                result.DeliveryRatingPoints = _CalculateDeliveryRatingPoints(input.DeliveryImpressions, universe);
                result.CPP = _CalculateCPP(result.Budget, result.DeliveryRatingPoints);
                return result;
            }

            if (input.DeliveryRatingPoints.HasValue && input.Budget.HasValue)
            {
                result.DeliveryImpressions = _CalculateDeliveryImpressionsByUniverse(input.DeliveryRatingPoints, universe);
                result.CPM = _CalculateCPM(input.Budget, result.DeliveryImpressions);
                result.CPP = _CalculateCPP(input.Budget, input.DeliveryRatingPoints);
                return result;
            }

            if (input.DeliveryRatingPoints.HasValue && input.CPP.HasValue)
            {
                result.Budget = _CalculateBudgetByCPP(input.DeliveryRatingPoints, input.CPP);
                result.DeliveryImpressions = _CalculateDeliveryImpressionsByUniverse(input.DeliveryRatingPoints, universe);
                result.CPM = _CalculateCPM(result.Budget, result.DeliveryImpressions);
                return result;
            }

            if (input.Budget.HasValue && input.CPP.HasValue)
            {
                result.DeliveryRatingPoints = _CalculateDeliveryRatingPointsByCPP(input.Budget, input.CPP);
                result.DeliveryImpressions = _CalculateDeliveryImpressionsByUniverse(input.DeliveryRatingPoints, universe);
                result.CPM = _CalculateCPM(input.Budget, result.DeliveryImpressions);
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
            return (decimal)deliveryImpressions.Value * CPM.Value;
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
            //delivery impressions is the raw number, but CPM is cost per 1000
            return deliveryImpressions.Value == 0 ? 0 : (budget.Value / (decimal)deliveryImpressions.Value) * 1000;
        }

        private static double _CalculateDeliveryRatingPointsByCPP(decimal? budget, decimal? cpp)
        {
            return cpp == 0 ? 0 : (double)(budget.Value / cpp.Value);
        }
    }
}
