using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ProposalInventoryTotalsDto
    {
        public decimal TotalCost { get; set; }
        public double BudgetPercent { get; set; }
        public double TotalImpressions { get; set; }
        public double ImpressionsPercent { get; set; }
        public decimal TotalCpm { get; set; }
        public double CpmPercent { get; set; }
        public bool BudgetMarginAchieved { get; set; }
        public bool ImpressionsMarginAchieved { get; set; }
        public bool CpmMarginAchieved { get; set; }

        public List<InventoryWeek> Weeks { get; set; }

        public class InventoryWeek
        {
            public int MediaWeekId { get; set; }
            public double Impressions { get; set; }
            public double ImpressionsPercent { get; set; }
            public decimal Budget { get; set; }
            public double BudgetPercent { get; set; }
            public bool BudgetMarginAchieved { get; set; }
            public bool ImpressionsMarginAchieved { get; set; }
        }

        public ProposalInventoryTotalsDto()
        {
            Weeks = new List<InventoryWeek>();
        }
    }

    public class ProposalInventoryTotalsRequestDto
    {
        public int ProposalDetailId { get; set; }
        public decimal DetailTargetBudget { get; set; }
        public double DetailTargetImpressions { get; set; }
        public decimal DetailCpm { get; set; }
        public List<InventoryWeek> Weeks { get; set; }

        public class InventoryWeek
        {
            public int MediaWeekId { get; set; }
            public double ImpressionGoal { get; set; }
            public decimal Budget { get; set; }
            public List<AllocatedSlot> Slots { get; set; } 

            public class AllocatedSlot
            {
                public double Impressions { get; set; }
                public decimal Cpm { get; set; }
                public decimal Cost { get; set; }
            }

            public InventoryWeek()
            {
                Slots = new List<AllocatedSlot>();
            }

        }

        public ProposalInventoryTotalsRequestDto()
        {
            Weeks = new List<InventoryWeek>();
        }
    }


}
