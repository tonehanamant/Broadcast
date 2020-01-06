namespace Services.Broadcast.Entities
{
    public class DetectionPostDetailAudience
    {
        public int DetectionDetailId { get; set; }

        public int AudienceRank { get; set; }

        public int AudienceId { get; set; }

        public double Delivery { get; set; }

        public DetectionPostDetailAudience(int detectionDetailId, int audienceRank, int audienceId, double delivery)
        {
            DetectionDetailId = detectionDetailId;
            AudienceRank = audienceRank;
            AudienceId = audienceId;
            Delivery = delivery;
        }
    }
}