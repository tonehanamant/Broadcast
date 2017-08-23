namespace Services.Broadcast.Entities.OpenMarketInventory
{
    public class BaseProposalInventoryWeekDto
    {
        public string QuarterText { get; set; }
        public string Week { get; set; }
        
        public double ImpressionsGoal { get; set; }
        public double ImpressionsPercent { get; set; }
        public double ImpressionsTotal { get; set; }
        public bool ImpressionsMarginAchieved { get; set; }

        public decimal Budget { get; set; }
        public double BudgetPercent { get; set; }
        public decimal BudgetTotal { get; set; }
        public bool BudgetMarginAchieved { get; set; }

        public bool IsHiatus { get; set; }
        public int MediaWeekId { get; set; }

        

    }
}