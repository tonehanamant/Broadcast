using Common.Services.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.ApplicationServices.Plan
{

    public interface IPlanBuyingService : IApplicationService
    {
        /// <summary>
        /// Gets plans buying list.
        /// </summary>
        /// <returns></returns>
        List<PlanBuyingListingItem> GetPlansBuying(int StatusFilter, int FlightFilter);

        /// <summary>
        /// Gets the plan buying.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        PlanBuying GetPlanBuyingById(int planId);

        /// <summary>
        /// Save the plan buying data.
        /// </summary>
        /// <param name="plan">Save The plan data.</param>
        /// <returns></returns>
        bool SavePlanBuying(int planId, PlanBuyingRequest plan);
    }

    public class PlanBuyingService : IPlanBuyingService
    {
        public List<PlanBuyingListingItem> GetPlansBuying(int StatusFilter, int FlightFilter)
        {
            var result = new List<PlanBuyingListingItem>();

            result.Add(new PlanBuyingListingItem
            {
                Id = 123,
                BookedBudget = 198000,
                BookedImpressions = 36600000,
                BookedCPM = 5.41,
                GoalBudget = 199836,
                GoalImpressions = 34313000,
                GoalCPM = 7.28,
                BookedMarginPercent = 26.15,
                GoalMarginPercent = 20,
                BookedImpressionsPercent = 133,
                GoalImpressionsPercent = 125,
                BookedCPMMarginPercent = 31.76,
                GoalCPMMarginPercent = 20,
                Status = 2,
                ModifiedDate = new DateTime(2020, 1, 7, 8, 30, 52),
                ModifiedBy = "Johnny Rockets"
            });

            result.Add(new PlanBuyingListingItem
            {
                Id = 459,
                BookedBudget = 567000,
                BookedImpressions = 98800000,
                BookedCPM = 2.11,
                GoalBudget = 122678,
                GoalImpressions = 45938000,
                GoalCPM = 5.88,
                BookedMarginPercent = 22.09,
                GoalMarginPercent = 34,
                BookedImpressionsPercent = 177,
                GoalImpressionsPercent = 159,
                BookedCPMMarginPercent = 45.09,
                GoalCPMMarginPercent = 26,
                Status = 2,
                ModifiedDate = new DateTime(2020, 1, 6, 6, 22, 33),
                ModifiedBy = "Michael Jordan"
            });

            return result;
        }

        public PlanBuying GetPlanBuyingById(int planId)
        {
            var result = new PlanBuying
            {
                Id = 123,
                Campaign = new PlanBuying.PlanBuyingCampaign
                {
                    Id = 456,   
                    Name = "Some Campaign"
                },
                Plan = new PlanBuying.PlanBuyingDetails
                {
                    Id = 123,
                    Name = "Some Plan",
                    FlightStartDate = new DateTime(2020, 3, 4, 8, 30, 52),
                    FlightEndDate = new DateTime(2020, 2, 4, 9, 30, 52),
                    FlightActiveDays = 14,
                    FlightHiatusDays = 3,
                    Budget = 250000,
                    Impressions = 27450000,
                    CPM = 9.10,
                    Dayparts = new List<PlanBuying.PlanBuyingDetails.PlanBuyingDetailsDaypart>
                    {
                        new PlanBuying.PlanBuyingDetails.PlanBuyingDetailsDaypart
                        {
                            DaypartCodeId = 7,
                            DaypartTypeId = 2,
                            StartTimeSeconds = 64800,
                            EndTimeSeconds = 72000,
                            IsStartTimeModified = false,
                            IsEndTimeModified = false,
                            HasRestrictions = true
                        }
                    },
                    CoverageGoalPercent = 80,
                    AvailableMarketsCount = 20,
                    BlackoutMarketsCount = 3,
                    AudienceId = 31,
                    PostingType = 1,
                    SpotLengthId = 1   
                },
                InventoryPlanner = new PlanBuying.PlanBuyingInventoryPlanner
                {
                    Id = 123,
                    Details = new List<PlanBuying.PlanBuyingInventoryPlanner.PlanBuyingInventoryPlannerDetail>
                    {
                        new PlanBuying.PlanBuyingInventoryPlanner.PlanBuyingInventoryPlannerDetail
                        {
                            InventorySourceId =  1,
                            Budget = 18300,
                            CPM = 5.00,
                            Impressions = 3660000,
                            SharePercent = 10,
                            EstimateId = "123A",
                            Notes = "Some notes",
                            IsBooked = true
                         },
                         new PlanBuying.PlanBuyingInventoryPlanner.PlanBuyingInventoryPlannerDetail
                         {
                            InventorySourceId =  2,
                            Budget = 20222,
                            CPM = 4.25,
                            Impressions = 4758000,
                            SharePercent = 13,
                            EstimateId = "456",
                            Notes = "",
                            IsBooked = true
                         }
                    }
                },
                BookedBudget = 197000,
                BookedImpressions = 36600000,
                BookedCPM = 5.41,
                GoalBudget = 199836,
                GoalImpressions = 34313000,
                GoalCPM = 7.28,
                BookedMarginPercent = 26.15,
                GoalMarginPercent = 20,
                BookedImpressionsPercent = 133,
                GoalImpressionsPercent = 125,
                BookedCPMMarginPercent = 31.76,
                GoalCPMMarginPercent = 20,
                InventoryTypeMakeupDetails = new List<PlanBuying.PlanBuyingInventoryTypeDetail>
                {
                    new PlanBuying.PlanBuyingInventoryTypeDetail
                    {
                        InventoryTypeId = 1,
                        Budget = 18300,
                        CPM = 3.75,
                        Impressions = 3660000,
                        SharePercent = 20
                    },
                    new PlanBuying.PlanBuyingInventoryTypeDetail
                    {
                        InventoryTypeId = 2,
                        Budget = 18300,
                        CPM = 3.75,
                        Impressions = 3660000,
                        SharePercent = 20
                    }
                },
                DaypartMakeupDetails = new List<PlanBuying.PlanBuyingDaypart>
                {
                    new PlanBuying.PlanBuyingDaypart
                    {
                        DaypartId = 1,
                        GoalPercent = 50,
                        SharePercent= 55,
                        Budget= 18300,
                        CPM= 3.75,
                        Impressions= 3660000
                    },
                    new PlanBuying.PlanBuyingDaypart
                    {
                        DaypartId = 2,
                        GoalPercent = null,
                        SharePercent= 45,
                        Budget= 20222,
                        CPM= 4.25,
                        Impressions= 4758000
                    }
                },
                Status = 2,
                Notes = "General notes for the buy",
                ModifiedDate = new DateTime(2020, 1, 7, 8, 30, 52),
                ModifiedBy = "Johnny Rockets"
            };

            return result;
        } 

        public bool SavePlanBuying(int planId, PlanBuyingRequest plan)
        {
            return true;
        }
    }
}
