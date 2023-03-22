namespace Services.Broadcast.Entities
{
    /// <summary>missing station DTO</summary>
    public class StationsGapDto
    {
        public string LegacyCallLetters { get; set; }
        public int? MarketCode { get; set; }
        public string MarketName { get; set; }
        public string Affiliation { get; set; }

    }
}
