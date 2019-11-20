using Services.Broadcast.Entities.Plan.Pricing;

namespace Services.Broadcast.Validators
{
    public interface IPricingCalculationValidator
    {
        void ValidateCalculationRequest(PlanPricingParametersDto request);
    }

    public class PricingCalculationValidator : IPricingCalculationValidator
    {
        public void ValidateCalculationRequest(PlanPricingParametersDto request)
        {
            // TODO: validate something and throw an exception if it fails.
        }
    }
}