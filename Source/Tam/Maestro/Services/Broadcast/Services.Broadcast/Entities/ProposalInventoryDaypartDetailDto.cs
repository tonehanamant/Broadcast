namespace Services.Broadcast.Entities
{
    public class ProposalInventoryDaypartDetailDto
    {
        public string InventorySpot { get; set; }
        public int InventoryDetailId { get; set; }
        public int DetailLevel { get; set; }

        public override string ToString()
        {
            return InventoryDetailId + "-" + DetailLevel;
        }
    }
}
