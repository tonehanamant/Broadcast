namespace Services.Broadcast.Entities
{
    public class RatingsResult
    {
        public short station_code { get; set; }

        public bool mon { get; set; }

        public bool tue { get; set; }

        public bool wed { get; set; }

        public bool thu { get; set; }

        public bool fri { get; set; }

        public bool sat { get; set; }

        public bool sun { get; set; }

        public int start_time { get; set; }

        public int end_time { get; set; }

        public double? rating { get; set; }

        public override string ToString()
        {
            return station_code + " " + rating;
        }
    }
}