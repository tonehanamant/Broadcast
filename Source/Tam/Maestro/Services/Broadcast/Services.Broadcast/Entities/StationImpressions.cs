namespace Services.Broadcast.Entities
{
    public class StationImpressions
    {
        public int Id { get; set; }
        public string Legacy_call_letters { get; set; }
        public double Impressions { get; set; }
        public double Rating  { get; set; }
    }
}