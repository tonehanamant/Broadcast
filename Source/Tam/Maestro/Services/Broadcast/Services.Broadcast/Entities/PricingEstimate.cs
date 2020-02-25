namespace Services.Broadcast.Entities
{
    public class PricingEstimate
    {
        public int InventorySourceId { get; set; }
        
        public int MediaWeekId { get; set; }

        public double Impressions { get; set; }

        public decimal Cost { get; set; }
    }
}
