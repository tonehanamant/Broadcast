namespace Services.Broadcast.Entities
{
    public class BvsPostDetailAudience
    {
        private readonly int _BvsDetailId;
        private readonly int _AudienceRank;
        private readonly int _AudienceId;
        private readonly double _Delivery;

        public int BvsDetailId
        {
            get { return _BvsDetailId; }
        }

        public int AudienceRank
        {
            get { return _AudienceRank; }
        }

        public int AudienceId
        {
            get { return _AudienceId; }
        }

        public double Delivery
        {
            get { return _Delivery; }
        }

        public BvsPostDetailAudience(int bvsDetailId, int audienceRank, int audienceId, double delivery)
        {
            _BvsDetailId = bvsDetailId;
            _AudienceRank = audienceRank;
            _AudienceId = audienceId;
            _Delivery = delivery;
        }
    }
}