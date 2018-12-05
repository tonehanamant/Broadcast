namespace Services.Broadcast.Entities.Nti
{
    public class NtiRatingCategory
    {
        public string Category { get; set; }

        public string SubCategory { get; set; }

        public double Percent { get; set; }

        public double Impressions { get; set; }

        public int? VPVH { get; set; }
    }
}
