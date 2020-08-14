using Common.Services.ApplicationServices;
using Newtonsoft.Json;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Buying;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.ApplicationServices.Buying
{

    public interface IBuyingService : IApplicationService
    {
        /// <summary>
        /// Gets plans buying list.
        /// </summary>
        /// <returns></returns>
        List<PlanBuyingListingItem> GetPlansBuying(PlanBuyingListRequest request);

        /// <summary>
        /// Gets the plan buying.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        PlanBuying GetPlanBuyingById(int planId);

        /// <summary>
        /// Save the plan buying data.
        /// </summary>
        /// <param name="planId">The id of the plan.</param>
        /// <param name="plan">Save The plan data.</param>
        /// <returns></returns>
        bool SavePlanBuying(int planId, PlanBuyingRequest plan);

        /// <summary>
        /// Get times frames
        /// </summary>
        /// <returns></returns>
        List<LookupDto> GetTimeFrames();

        /// <summary>
        /// Get statuses
        /// </summary>
        /// <returns></returns>
        List<LookupDto> GetStatuses();
        
    }

    public class BuyingService : IBuyingService
    {
        public List<PlanBuyingListingItem> GetPlansBuying(PlanBuyingListRequest request)
        {
            var result = new List<PlanBuyingListingItem>();

            result.Add(new PlanBuyingListingItem
            {
                Id = 1,
                Campaign = new PlanBuyingListingItem.PlanBuyingListingItemCampaign
                {
                    Id = 1,
                    Name = "Some Campaign",
                    Advertiser = new PlanBuyingListingItem.PlanBuyingListingItemCampaign.PlanBuyingListingItemCampaignAdvertiser
                    {
                        Id = 1,
                        Name = "Some Advertiser",
                    },
                    Agency = new PlanBuyingListingItem.PlanBuyingListingItemCampaign.PlanBuyingListingItemCampaignAgency
                    {
                        Id = 1,
                        Name = "Some Agency",
                    }
                },
                Plan = new PlanBuyingListingItem.PlanBuyingListingItemDetails
                {
                    Id = 1,
                    Name = "Some Plan",
                    FlightStartDate = new DateTime(2020, 3, 4, 8, 30, 52),
                    FlightEndDate = new DateTime(2020, 2, 4, 9, 30, 52),
                    FlightActiveDays = 14,
                    FlightHiatusDays = 3,
                    Budget = 250000,
                    Impressions = 27450000,
                    CPM = 9.10
                },
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
                Id = 2,
                Campaign = new PlanBuyingListingItem.PlanBuyingListingItemCampaign
                {
                    Id = 2,
                    Name = "Some New Campaign",
                    Advertiser = new PlanBuyingListingItem.PlanBuyingListingItemCampaign.PlanBuyingListingItemCampaignAdvertiser
                    {
                        Id = 2,
                        Name = "Some New Advertiser",
                    },
                    Agency = new PlanBuyingListingItem.PlanBuyingListingItemCampaign.PlanBuyingListingItemCampaignAgency
                    {
                        Id = 2,
                        Name = "Some New Agency",
                    }
                },
                Plan = new PlanBuyingListingItem.PlanBuyingListingItemDetails
                {
                    Id = 2,
                    Name = "Some New Plan",
                    FlightStartDate = new DateTime(2020, 3, 4, 8, 30, 52),
                    FlightEndDate = new DateTime(2020, 2, 4, 9, 30, 52),
                    FlightActiveDays = 7,
                    FlightHiatusDays = 2,
                    Budget = 450000,
                    Impressions = 27650000,
                    CPM = 8.9
                },
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


            if (request.FlightFilter.Equals(PlanBuyingTimeFramesEnum.All))
            {
                result.Add(new PlanBuyingListingItem
                {
                    Id = 3,
                    Campaign = new PlanBuyingListingItem.PlanBuyingListingItemCampaign
                    {
                        Id = 3,
                        Name = "Updated Campaign",
                        Advertiser = new PlanBuyingListingItem.PlanBuyingListingItemCampaign.PlanBuyingListingItemCampaignAdvertiser
                        {
                            Id = 2,
                            Name = "Some New Advertiser",
                        },
                        Agency = new PlanBuyingListingItem.PlanBuyingListingItemCampaign.PlanBuyingListingItemCampaignAgency
                        {
                            Id = 2,
                            Name = "Some New Agency",
                        }
                    },
                    Plan = new PlanBuyingListingItem.PlanBuyingListingItemDetails
                    {
                        Id = 3,
                        Name = "Updated Plan",
                        FlightStartDate = new DateTime(2020, 3, 4, 8, 30, 52),
                        FlightEndDate = new DateTime(2020, 2, 4, 9, 30, 52),
                        FlightActiveDays = 6,
                        FlightHiatusDays = 1,
                        Budget = 390000,
                        Impressions = 23150000,
                        CPM = 7.5
                    },
                    BookedBudget = 5127000,
                    BookedImpressions = 92100000,
                    BookedCPM = 2.11,
                    GoalBudget = 123723,
                    GoalImpressions = 42238000,
                    GoalCPM = 5.21,
                    BookedMarginPercent = 20.11,
                    GoalMarginPercent = 31,
                    BookedImpressionsPercent = 166,
                    GoalImpressionsPercent = 143,
                    BookedCPMMarginPercent = 44.02,
                    GoalCPMMarginPercent = 24,
                    Status = 2,
                    ModifiedDate = new DateTime(2020, 2, 5, 6, 22, 33),
                    ModifiedBy = "Michael Jordan"
                });
            }

            if (request.StatusFilter.Equals(PlanBuyingStatusEnum.Contracted))
            {
                result.Add(new PlanBuyingListingItem
                {
                    Id = 4,
                    Campaign = new PlanBuyingListingItem.PlanBuyingListingItemCampaign
                    {
                        Id = 4,
                        Name = "Updated Contracted Campaign",
                        Advertiser = new PlanBuyingListingItem.PlanBuyingListingItemCampaign.PlanBuyingListingItemCampaignAdvertiser
                        {
                            Id = 1,
                            Name = "Some Advertiser",
                        },
                        Agency = new PlanBuyingListingItem.PlanBuyingListingItemCampaign.PlanBuyingListingItemCampaignAgency
                        {
                            Id = 2,
                            Name = "Some New Agency",
                        }
                    },
                    Plan = new PlanBuyingListingItem.PlanBuyingListingItemDetails
                    {
                        Id = 4,
                        Name = "Updated Contracted Plan",
                        FlightStartDate = new DateTime(2020, 3, 4, 8, 30, 52),
                        FlightEndDate = new DateTime(2020, 2, 4, 9, 30, 52),
                        FlightActiveDays = 6,
                        FlightHiatusDays = 1,
                        Budget = 390000,
                        Impressions = 23150000,
                        CPM = 7.5
                    },
                    BookedBudget = 3337000,
                    BookedImpressions = 98800000,
                    BookedCPM = 2.22,
                    GoalBudget = 321723,
                    GoalImpressions = 44438000,
                    GoalCPM = 5.11,
                    BookedMarginPercent = 20.11,
                    GoalMarginPercent = 31,
                    BookedImpressionsPercent = 166,
                    GoalImpressionsPercent = 143,
                    BookedCPMMarginPercent = 44.02,
                    GoalCPMMarginPercent = 24,
                    Status = 1,
                    ModifiedDate = new DateTime(2020, 2, 5, 6, 22, 33),
                    ModifiedBy = "Michael Jordan"
                });
            }

            return result;
        }

        public PlanBuying GetPlanBuyingById(int planId)
        {
            var result = new PlanBuying
            {
                Id = 1,
                Campaign = new PlanBuying.PlanBuyingCampaign
                {
                    Id = 1,   
                    Name = "Some Campaign"
                },
                Plan = new PlanBuying.PlanBuyingDetails
                {
                    Id = 1,
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
                    Id = 1,
                    Details = new List<PlanBuying.PlanBuyingInventoryPlanner.PlanBuyingInventoryPlannerDetail>
                    {
                        new PlanBuying.PlanBuyingInventoryPlanner.PlanBuyingInventoryPlannerDetail
                        {
                            InventorySourceId =  1,
                            Budget = 18300,
                            CPM = 5.00,
                            Impressions = 3660000,
                            SharePercent = 10,
                            EstimateId = "1",
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
                            EstimateId = "2",
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

        public List<LookupDto> GetTimeFrames()
        {
            return Enum.GetValues(typeof(PlanBuyingTimeFramesEnum))
               .Cast<PlanBuyingTimeFramesEnum>()
               .Select(x => new LookupDto
               {
                   Id = (int)x,
                   Display = x.GetDescriptionAttribute()
               })
               .OrderBy(x => x.Id).ToList();
        }

        public List<LookupDto> GetStatuses()
        {
            return Enum.GetValues(typeof(PlanBuyingStatusEnum))
               .Cast<PlanBuyingStatusEnum>()
               .Select(x => new LookupDto
               {
                   Id = (int)x,
                   Display = x.GetDescriptionAttribute()
               })
               .OrderBy(x => x.Id).ToList();
        }
    }
}
