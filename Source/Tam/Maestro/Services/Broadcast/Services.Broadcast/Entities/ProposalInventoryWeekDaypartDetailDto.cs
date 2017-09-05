namespace Services.Broadcast.Entities
{
    public class ProposalInventoryWeekDaypartDetailDto
    {
        public int Id { get; set; }
        public string InventorySpot { get; set; }
        public string Contract { get; set; }
        public float Eff { get; set; }
        public float EffVariation { get; set; }
        public double Impressions { get; set; }
        public decimal Cpm { get; set; }
        public decimal Cost { get; set; }
        public bool IsHiatus { get; set; }
    }
}
