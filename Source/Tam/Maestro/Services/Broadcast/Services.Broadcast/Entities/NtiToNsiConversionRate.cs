namespace Services.Broadcast.Entities
{
    public class NtiToNsiConversionRate
    {
        public int Id { get; set; }

        public double ConversionRate { get; set; }

        public int DaypartDefaultId { get; set; }

        public int MediaMonthId { get; set; }
    }
}
