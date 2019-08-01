using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using System;
using System.Linq;

namespace Services.Broadcast.Validators
{
    public interface IPlanValidator
    {
        /// <summary>
        /// Validates the new plan.
        /// </summary>
        /// <param name="plan">The plan.</param>
        void ValidatePlan(PlanDto plan);
    }

    public class PlanValidator : IPlanValidator
    {
        private ISpotLengthEngine _SpotLengthEngine;

        public PlanValidator(ISpotLengthEngine spotLengthEngine)
        {
            _SpotLengthEngine = spotLengthEngine;
        }

        const string INVALID_PLAN_NAME = "Invalid plan name";
        const string INVALID_SPOT_LENGTH = "Invalid spot length";
        const string INVALID_PRODUCT = "Invalid product";
        const string INVALID_FLIGHT_DATES = "Invalid flight dates.  The end date cannot be before the start date.";
        const string INVALID_FLIGHT_HIATUS_DAY = "Invalid flight hiatus day.  All days must be within the flight date range.";

        public void ValidatePlan(PlanDto plan)
        {
            if (string.IsNullOrWhiteSpace(plan.Name) || plan.Name.Length > 255)
            {
                throw new Exception(INVALID_PLAN_NAME);
            }
            if (!_SpotLengthEngine.SpotLengthIdExists(plan.SpotLengthId))
            {
                throw new Exception(INVALID_SPOT_LENGTH);
            }
            if(plan.ProductId <= 0)
            {
                throw new Exception(INVALID_PRODUCT);
            }

            _ValidateFlightAndHiatusDates(plan);
        }

        #region Helpers

        private void _ValidateFlightAndHiatusDates(PlanDto plan)
        {
            if (plan.FlightStartDate.HasValue == false || plan.FlightEndDate.HasValue == false)
            {
                return;
            }

            if (plan.FlightStartDate > plan.FlightEndDate)
            {
                throw new Exception(INVALID_FLIGHT_DATES);
            }

            if (plan.FlightHiatusDays?.Any() == true)
            {
                var hasInvalids =
                    plan.FlightHiatusDays.Any(h => h.Date < plan.FlightStartDate || h.Date > plan.FlightEndDate);
                if (hasInvalids)
                {
                    throw new Exception(INVALID_FLIGHT_HIATUS_DAY);
                }
            }
        }

        #endregion // #region Helpers
    }
}
