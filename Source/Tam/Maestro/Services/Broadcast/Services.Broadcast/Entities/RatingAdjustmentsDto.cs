namespace Services.Broadcast.Entities
{
    public class RatingAdjustmentsDto
    {
        public int MediaMonthId { get; set; }
        public string MediaMonthDisplay { get; set; }
        public decimal AnnualAdjustment { get; set; }
        public decimal NtiAdjustment { get; set; }
    }
}
