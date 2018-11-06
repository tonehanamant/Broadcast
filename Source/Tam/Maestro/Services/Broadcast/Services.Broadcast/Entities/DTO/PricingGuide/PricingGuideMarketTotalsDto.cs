namespace Services.Broadcast.Entities.DTO.PricingGuide
{
    public class PricingGuideMarketTotalsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Coverage { get; set; }
        public int Stations { get; set; }
        public int Programs { get; set; }
        public decimal MinCpm { get; set; }
        public decimal MaxCpm { get; set; }
        public decimal AvgCpm { get; set; }
        public bool Selected { get; set; }
    }
}
