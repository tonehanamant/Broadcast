namespace Services.Broadcast.Entities
{
    public class BvsPostDetailAudience
    {
        public int BvsDetailId { get; }

        public int AudienceRank { get; }

        public int AudienceId { get; }

        public double Delivery { get; }

        public BvsPostDetailAudience(int bvsDetailId, int audienceRank, int audienceId, double delivery)
        {
            BvsDetailId = bvsDetailId;
            AudienceRank = audienceRank;
            AudienceId = audienceId;
            Delivery = delivery;
        }
    }
}