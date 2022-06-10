using System;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsOutOfSpecsDto
    {
        public int Id { get; set; }
        public string SpotUniqueHashExternal { get; set; }
        public string ReasonCodeMessage { get; set; }
        public int EstimateId { get; set; }
        public string IsciName { get; set; }        
        public int? RecommendedPlanId { get; set; }
        public string RecommendedPlanName { get; set; }
        public string ProgramName { get; set; }
        public string StationLegacyCallLetters { get; set; }
        public string Affiliate { get; set; }
        public string Market { get; set; }
        public Guid? AdvertiserMasterId { get; set; }
        public string AdvertiserName { get; set; }        
        public int? SpotLengthId { get; set; }
        public SpotLengthDto SpotLength { get; set; } = new SpotLengthDto();
        public int? AudienceId { get; set; }
        public AudienceDto Audience { get; set; } = new AudienceDto();
        public string Product { get; set; }
        public DateTime? FlightStartDate { get; set; }
        public DateTime? FlightEndDate { get; set; }
        public string DaypartCode { get; set; }
        public string GenreName { get; set; }
        public DaypartDetailDto DaypartDetail { get; set; } = new DaypartDetailDto();
        public string ProgramNetwork { get; set; }
        public DateTime ProgramAirTime { get; set; }
        public string IngestedBy { get; set; }
        public DateTime IngestedAt { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Impressions { get; set; }
        public int PlanId { get; set; }
        public SpotExceptionsOutOfSpecDecisionsDto SpotExceptionsOutOfSpecDecision { get; set; } = new SpotExceptionsOutOfSpecDecisionsDto();
        public SpotExceptionsOutOfSpecReasonCodeDto SpotExceptionsOutOfSpecReasonCode { get; set; } = new SpotExceptionsOutOfSpecReasonCodeDto();
        public int? MarketCode { get; set; }
        public int? MarketRank { get; set; }

        public string HouseIsci { get; set; }
        public Genre ProgramGenre { get; set; } = new Genre();
        public string TimeZone { get; set; } = "EST"; // TODO: Populate this for real
        public int DMA { get; set; } = 58; // TODO: Populate this for real
        public string Comments { get; set; }
        public int InventorySourceId { get; set; }
        public string InventorySourceName { get; set; }
    }
}

