namespace Services.Broadcast.Entities
{
    public class StationImpressions
    {
        public int id { get; set; }

        public short station_code { get; set; }

        public double impressions { get; set; }
    }
}