using Newtonsoft.Json;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers.Json;
using System;
using System.Linq;
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

            new ComparisonMinorProperty(typeof(PlanMarketDto), "Id"),

            new ComparisonMinorProperty(typeof(PlanDaypartDto), "PlanDaypartId"),
            new ComparisonMinorProperty(typeof(PlanDaypartDto), "CustomName"),
            new ComparisonMinorProperty(typeof(PlanDaypartDto), "DaypartOrganizationId"),
            new ComparisonMinorProperty(typeof(PlanDaypartDto), "DaypartOrganizationName"),
            new ComparisonMinorProperty(typeof(PlanDaypartDto), "DaypartTypeId"),
            new ComparisonMinorProperty(typeof(PlanDaypartDto), "DaypartUniquekey"),
            new ComparisonMinorProperty(typeof(PlanDaypartDto), "IsEndTimeModified"),
            new ComparisonMinorProperty(typeof(PlanDaypartDto), "IsStartTimeModified"),
            new ComparisonMinorProperty(typeof(PlanDaypartDto), "VpvhForAudiences"),

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
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "IsLocked"),
            new ComparisonMinorProperty(typeof(PlanDto), "RawWeeklyBreakdownWeeks"),
            new ComparisonMinorProperty(typeof(PlanDto), "CustomDayparts"),
            new ComparisonMinorProperty(typeof(PlanDto), "PricingBudgetCpmLever"),
            new ComparisonMinorProperty(typeof(PlanDto), "BuyingBudgetCpmLever"),

            // ignored - not affecting pricing goals or inventory
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "AduImpressions"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "CustomName"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "DaypartOrganizationId"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "DaypartOrganizationName"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "DaypartUniquekey"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "EndDate"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "IsUpdated"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "PercentageOfWeek"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "PlanDaypartId"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "UnitImpressions"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "WeeklyAdu"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "WeeklyCpm"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "WeeklyImpressionsPercentage"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "WeeklyRatings"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "WeeklyUnits"),
            new ComparisonMinorProperty(typeof(WeeklyBreakdownWeek), "WeekNumber"),

        };

        private static PlanDto _OrderListForCompare(PlanDto toOrder)
        {
            var ordered = toOrder.DeepCloneUsingSerialization();

            // for static meta data we can use the id.
            ordered.FlightDays = ordered.FlightDays.OrderBy(s => s).ToList();
            ordered.FlightHiatusDays = ordered.FlightHiatusDays.OrderBy(s => s).ToList();
            ordered.SecondaryAudiences = ordered.SecondaryAudiences.OrderBy(s => s.AudienceId).ToList();
            ordered.AvailableMarkets = ordered.AvailableMarkets.OrderBy(s => s.Rank).ToList();
            ordered.BlackoutMarkets = ordered.BlackoutMarkets.OrderBy(s => s.Rank).ToList();
            ordered.CreativeLengths = ordered.CreativeLengths.OrderBy(s => s.SpotLengthId).ToList();

            // for plan data the id orders can't be trusted, so use a unique key
            ordered.Dayparts = ordered.Dayparts.OrderBy(s => s.DaypartUniquekey).ToList();
            ordered.WeeklyBreakdownWeeks = ordered.WeeklyBreakdownWeeks.OrderBy(s => $"{s.MediaWeekId}:{s.SpotLengthId}:{s.DaypartUniquekey}").ToList();

            return ordered;
        }

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

            // make sure the lists are ordered the same for comparison.
            var orderedBeforePlan = _OrderListForCompare(beforePlan);
            var orderedAfterPlan = _OrderListForCompare(afterPlan);

            /*** Compare the rest ***/
            var serializedBefore = _GetSerializedPlanPricingInputs(orderedBeforePlan);
            var serializedAfter = _GetSerializedPlanPricingInputs(orderedAfterPlan);

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