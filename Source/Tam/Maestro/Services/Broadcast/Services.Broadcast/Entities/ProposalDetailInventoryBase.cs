using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProposalDetailInventoryBase : IHaveSingleSharedPostingBooks
    {
        public ProposalDetailInventoryBase()
        {
            ProposalFlightWeeks = new List<ProposalFlightWeek>();
            DetailFlightWeeks = new List<ProposalFlightWeek>();
        }

        // proposal fields
        public string ProposalName { get; set; }
        public int ProposalId { get; set; }
        public int ProposalVersion { get; set; }
        public int ProposalVersionId { get; set; }

        public DateTime? ProposalFlightStartDate { get; set; }
        public DateTime? ProposalFlightEndDate { get; set; }

        public List<ProposalFlightWeek> ProposalFlightWeeks { get; set; }
        public double? Margin { get; set; }

        // spot lenght id is saved
        public int DetailSpotLengthId { get; set; }
        public int DetailSpotLength { get; set; }
        public DaypartDto DetailDaypart { get; set; }
        // daypart is saved
        public int? DetailDaypartId { get; set; }

        public DateTime DetailFlightStartDate { get; set; }
        public DateTime DetailFlightEndDate { get; set; }
        public List<ProposalFlightWeek> DetailFlightWeeks { get; set; }

        public decimal? DetailTargetBudget { get; set; }
        public double DetailTargetImpressions { get; set; }
        public decimal DetailCpm { get; set; }
        public int DetailId { get; set; }

        public double DetailBudgetPercent { get; set; }
        public double DetailImpressionsPercent { get; set; }
        public double DetailCpmPercent { get; set; }

        public decimal DetailTotalBudget { get; set; }
        public double DetailTotalImpressions { get; set; }
        public decimal DetailTotalCpm { get; set; }

        public bool DetailBudgetMarginAchieved { get; set; }
        public bool DetailImpressionsMarginAchieved { get; set; }
        public bool DetailCpmMarginAchieved { get; set; }

        public ProposalEnums.ProposalPlaybackType? PlaybackType { get; set; }
        public int? ShareProjectionBookId { get; set; }
        public int? HutProjectionBookId { get; set; }
        public int? SingleProjectionBookId { get; set; }
        public int? GuaranteedAudience { get; set; }
        public bool? Equivalized { get; set; }
        public SchedulePostType? PostType { get; set; }
    }
}
