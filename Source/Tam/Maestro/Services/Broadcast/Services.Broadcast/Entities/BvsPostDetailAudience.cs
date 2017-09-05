namespace Services.Broadcast.Entities
{
    public class BvsPostDetailAudience
    {
        public int BvsDetailId { get; set; }

        public int AudienceRank { get; set; }

        public int AudienceId { get; set; }

        public double Delivery { get; set; }

        public BvsPostDetailAudience(int bvsDetailId, int audienceRank, int audienceId, double delivery)
        {
            BvsDetailId = bvsDetailId;
            AudienceRank = audienceRank;
            AudienceId = audienceId;
            Delivery = delivery;
        }
    }
}