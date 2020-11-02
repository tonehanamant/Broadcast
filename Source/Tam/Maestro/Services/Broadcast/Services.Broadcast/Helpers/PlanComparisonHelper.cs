using Newtonsoft.Json;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers.Json;
using Tam.Maestro.Common;

namespace Services.Broadcast.Helpers
{
    public static class PlanComparisonHelper
    {
        public static bool IsCreatingNewPLan(PlanDto plan)
        {
            if (plan.VersionId == 0 || plan.Id == 0)
            {
                return true;
            }

            return false;
        }

        public static bool PlanPricingInputsAreOutOfSync(PlanDto beforePlan, PlanDto afterPlan)
        {
            if (beforePlan == null)
            {
                // hopefully this wasn't a cloned plan...
                if (afterPlan.JobId.HasValue)
                {
                    return false;
                }

                return true;
            }

            /*** Compare the rest ***/
            var serializedBefore = _GetSerializedPlanPricingInputs(beforePlan);
            var serializedAfter = _GetSerializedPlanPricingInputs(afterPlan);

            // this isn't so much needed for runtime
            // but it makes debug a lot easier so it stays
            var hashBefore = _GetHashFromSerialized(serializedBefore);
            var hashAfter = _GetHashFromSerialized(serializedAfter);

            if (hashAfter.Equals(hashBefore) == false)
            {
                return true;
            }

            return false;
        }

        private static string _GetHashFromSerialized(string serialized)
        {
            var stream = BroadcastStreamHelper.CreateStreamFromString(serialized);
            var hash = HashGenerator.ComputeHash(StreamHelper.ReadToEnd(stream));
            return hash;
        }

        private static string _GetSerializedPlanPricingInputs(PlanDto plan)
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            // instance items - not pricing inputs
            jsonResolver.Ignore(typeof(PlanDto), "Id");
            jsonResolver.Ignore(typeof(PlanDto), "VersionId");
            jsonResolver.Ignore(typeof(PlanDto), "VersionNumber");
            jsonResolver.Ignore(typeof(PlanDto), "ModifiedBy");
            jsonResolver.Ignore(typeof(PlanDto), "ModifiedDate");
            jsonResolver.Ignore(typeof(PlanMarketDto), "Id");
            jsonResolver.Ignore(typeof(PlanDto), "JobId");
            jsonResolver.Ignore(typeof(PlanDto), "IsOutOfSync");            
            jsonResolver.Ignore(typeof(PlanDto), "AvailableMarketsWithSovCount");

            // ignored - not pricing inputs
            jsonResolver.Ignore(typeof(PlanDto), "Name");
            jsonResolver.Ignore(typeof(PlanDto), "ProductId");
            jsonResolver.Ignore(typeof(PlanDto), "FlightNotes");
            jsonResolver.Ignore(typeof(PlanDto), "Status");

            // ignored - out of scope for this compare
            jsonResolver.Ignore(typeof(PlanDto), "PricingParameters");
            jsonResolver.Ignore(typeof(PlanDto), "BuyingParameters");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var serializedPlan = JsonSerializerHelper.ConvertToJson(plan, jsonSettings);
            return serializedPlan;
        }
    }
}