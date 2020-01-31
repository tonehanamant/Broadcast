namespace Services.Broadcast.Entities.Nti
{
    public class NtiUniverseAudienceMapping
    {
        public int Id { get; set; }

        public BroadcastAudience Audience { get; set; }

        public string NtiAudienceCode { get; set; }
    }
}
