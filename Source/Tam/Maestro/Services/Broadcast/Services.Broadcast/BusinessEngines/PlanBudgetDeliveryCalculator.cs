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
        ///<inheritdoc/>
        public PlanDeliveryBudget CalculateBudget(PlanDeliveryBudget input)
        {
            if((input.Delivery.HasValue && input.Delivery < 0)
                || (input.Budget.HasValue && input.Budget < 0)
                || (input.CPM.HasValue && input.CPM < 0))
            {
                throw new Exception("Invalid budget values passed");
            }

            if (input.Delivery.HasValue && input.Budget.HasValue)
            {
                return new PlanDeliveryBudget
                {
                    Budget = input.Budget,
                    Delivery = input.Delivery,
                    CPM = input.Budget.Value == 0 ? 0 :  (decimal)input.Delivery.Value / input.Budget.Value
                };
            }

            if (input.Delivery.HasValue && input.CPM.HasValue)
            {
                return new PlanDeliveryBudget
                {
                    Budget = input.CPM.Value == 0 ? 0 : (decimal)input.Delivery.Value / input.CPM.Value,
                    Delivery = input.Delivery,
                    CPM = input.CPM
                };
            }

            if (input.Budget.HasValue && input.CPM.HasValue)
            {
                return new PlanDeliveryBudget
                {
                    Budget = input.Budget,
                    Delivery = (double)(input.CPM.Value * input.Budget.Value),
                    CPM = input.CPM
                };
            }
            throw new Exception("Need at least 2 values for budget to calculate the third");
        }
    }
}
