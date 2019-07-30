using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Validators
{
    public interface IPlanValidator
    {
        void ValidateNewPlan(CreatePlanDto plan);
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

        public void ValidateNewPlan(CreatePlanDto plan)
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
        }
    }
}
