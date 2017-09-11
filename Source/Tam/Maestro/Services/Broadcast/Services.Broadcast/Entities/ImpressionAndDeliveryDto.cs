namespace Services.Broadcast.Entities
{
    public class ImpressionAndDeliveryDto
    {
        public int AudienceId { get; set; }
        public string AudienceName { get; set; }

        public double OrderedImpressions { get; set; }
        public double DeliveredImpressions { get; set; }

        public double OutOfSpecDeliveredImpressions { get; set; }

        public double TotalDeliveredImpressions
        {
            get { return DeliveredImpressions + OutOfSpecDeliveredImpressions; }
        }
    }
}
