using Services.Broadcast.Entities.Plan.Pricing;

namespace Services.Broadcast.Validators
{
    public interface IPricingCalculationValidator
    {
        void ValidateCalculationRequest(PlanPricingRequestDto request);
    }

    public class PricingCalculationValidator : IPricingCalculationValidator
    {
        public void ValidateCalculationRequest(PlanPricingRequestDto request)
        {
            // TODO: validate something and throw an exception if it fails.
        }
    }
}