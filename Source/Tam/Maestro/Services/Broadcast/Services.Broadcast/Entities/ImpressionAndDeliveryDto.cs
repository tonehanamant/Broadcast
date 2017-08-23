namespace Services.Broadcast.Entities
{
    public class ImpressionAndDeliveryDto
    {
        public int AudienceId { get; set; }
        public string AudienceName { get; set; }

        public int? OrderedImpressions { get; set; }
        public double DeliveredImpressions { get; set; }

        public double OutOfSpecDeliveredImpressions { get; set; }

        public double TotalDeliveredImpressions
        {
            get { return DeliveredImpressions + OutOfSpecDeliveredImpressions; }
        }
    }
}
