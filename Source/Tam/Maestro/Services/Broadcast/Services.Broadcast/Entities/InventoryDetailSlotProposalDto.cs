namespace Services.Broadcast.Entities
{
    public class InventoryDetailSlotProposalDto
    {
        public int InventoryDetailSlotId { get; set; }
        public int ProposalVersionDetailQuarterWeekId { get; set; }
        public int Order { get; set; }
        public string CreatedBy { get; set; }
    }
}