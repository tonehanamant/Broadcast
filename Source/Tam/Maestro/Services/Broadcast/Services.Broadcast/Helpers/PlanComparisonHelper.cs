using Newtonsoft.Json;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers.Json;
using System;
using Tam.Maestro.Common;

namespace Services.Broadcast.Helpers
{
    public static class PlanComparisonHelper
    {
        internal static ComparisonMinorProperty[] MinorProperties { get; } = new []
        {
            // instance items - not pricing inputs
            new ComparisonMinorProperty(typeof(PlanDto), "Id"),
            new ComparisonMinorProperty(typeof(PlanDto), "VersionId"),
            new ComparisonMinorProperty(typeof(PlanDto), "VersionNumber"),
            new ComparisonMinorProperty(typeof(PlanDto), "ModifiedBy"),
            new ComparisonMinorProperty(typeof(PlanDto), "ModifiedDate"),
            new ComparisonMinorProperty(typeof(PlanDto), "JobId"),
            new ComparisonMinorProperty(typeof(PlanDto), "IsOutOfSync"),
            new ComparisonMinorProperty(typeof(PlanDto), "AvailableMarketsWithSovCount"),
            new ComparisonMinorProperty(typeof(PlanDto), "BlackoutMarketCount"),
            new ComparisonMinorProperty(typeof(PlanDto), "BlackoutMarketTotalUsCoveragePercent"),
            new ComparisonMinorProperty(typeof(PlanDto), "IsDraft"),
            new ComparisonMinorProperty(typeof(PlanDto), "CustomDayparts"),
            new ComparisonMinorProperty(typeof(PlanMarketDto), "Id"),
            // ignored - not pricing inputs
            new ComparisonMinorProperty(typeof(PlanDto), "Name"),
            new ComparisonMinorProperty(typeof(PlanDto), "ProductId"),
            new ComparisonMinorProperty(typeof(PlanDto), "FlightNotes"),
            new ComparisonMinorProperty(typeof(PlanDto), "FlightNotesInternal"),
            new ComparisonMinorProperty(typeof(PlanDto), "Status"),
            new ComparisonMinorProperty(typeof(PlanDto), "GoalBreakdownType"),
            // ignored - out of scope for this compare
            new ComparisonMinorProperty(typeof(PlanDto), "PricingParameters"),
            new ComparisonMinorProperty(typeof(PlanDto), "BuyingParameters"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "IsLocked")
        };

        public static bool DidPlanPricingInputsChange(PlanDto beforePlan, PlanDto afterPlan)
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
            MinorProperties.ForEach(p =>
            {
                jsonResolver.Ignore(p.ClassType, p.PropertyName);
            });

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            var serializedPlan = JsonSerializerHelper.ConvertToJson(plan, jsonSettings);
            return serializedPlan;
        }

        internal class ComparisonMinorProperty
        {
            public Type ClassType { get; private set; }
            public string PropertyName { get; private set; }

            public ComparisonMinorProperty(Type classType, string propertyName)
            {
                ClassType = classType;
                PropertyName = propertyName;
            }
        }
    }
}