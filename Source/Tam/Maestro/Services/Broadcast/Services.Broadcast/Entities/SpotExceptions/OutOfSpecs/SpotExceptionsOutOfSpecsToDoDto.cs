using System;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary></summary>
    public class SpotExceptionsOutOfSpecsToDoDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the spot unique hash external.
        /// </summary>
        /// <value>The spot unique hash external.</value>
        public string SpotUniqueHashExternal { get; set; }

        /// <summary>
        /// Gets or sets the execution identifier external.
        /// </summary>
        /// <value>The execution identifier external.</value>
        public string ExecutionIdExternal { get; set; }

        /// <summary>
        /// Gets or sets the reason code message.
        /// </summary>
        /// <value>The reason code message.</value>
        public string ReasonCodeMessage { get; set; }

        /// <summary>
        /// Gets or sets the estimate identifier.
        /// </summary>
        /// <value>The estimate identifier.</value>
        public int? EstimateId { get; set; }

        /// <summary>
        /// Gets or sets the name of the isci.
        /// </summary>
        /// <value>The name of the isci.</value>
        public string IsciName { get; set; }

        /// <summary>
        /// Gets or sets the recommended plan identifier.
        /// </summary>
        /// <value>The recommended plan identifier.</value>
        public int? RecommendedPlanId { get; set; }

        /// <summary>
        /// Gets or sets the name of the recommended plan.
        /// </summary>
        /// <value>The name of the recommended plan.</value>
        public string RecommendedPlanName { get; set; }

        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>The name of the program.</value>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the station legacy call letters.
        /// </summary>
        /// <value>The station legacy call letters.</value>
        public string StationLegacyCallLetters { get; set; }

        /// <summary>
        /// Gets or sets the affiliate.
        /// </summary>
        /// <value>The affiliate.</value>
        public string Affiliate { get; set; }

        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>The market.</value>
        public string Market { get; set; }

        /// <summary>
        /// Gets or sets the advertiser master identifier.
        /// </summary>
        /// <value>The advertiser master identifier.</value>
        public Guid? AdvertiserMasterId { get; set; }

        /// <summary>
        /// Gets or sets the name of the advertiser.
        /// </summary>
        /// <value>The name of the advertiser.</value>
        public string AdvertiserName { get; set; }

        /// <summary>
        /// Gets or sets the spot length identifier.
        /// </summary>
        /// <value>The spot length identifier.</value>
        public int? SpotLengthId { get; set; }

        /// <summary>
        /// Gets or sets the length of the spot.
        /// </summary>
        /// <value>The length of the spot.</value>
        public SpotLengthDto SpotLength { get; set; } = new SpotLengthDto();

        /// <summary>
        /// Gets or sets the audience identifier.
        /// </summary>
        /// <value>The audience identifier.</value>
        public int? AudienceId { get; set; }

        /// <summary>
        /// Gets or sets the audience.
        /// </summary>
        /// <value>The audience.</value>
        public AudienceDto Audience { get; set; } = new AudienceDto();

        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        /// <value>The product.</value>
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the flight start date.
        /// </summary>
        /// <value>The flight start date.</value>
        public DateTime? FlightStartDate { get; set; }

        /// <summary>
        /// Gets or sets the flight end date.
        /// </summary>
        /// <value>The flight end date.</value>
        public DateTime? FlightEndDate { get; set; }

        /// <summary>
        /// Gets or sets the daypart code.
        /// </summary>
        /// <value>The daypart code.</value>
        public string DaypartCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the genre.
        /// </summary>
        /// <value>The name of the genre.</value>
        public string GenreName { get; set; }

        /// <summary>
        /// Gets or sets the daypart detail.
        /// </summary>
        /// <value>The daypart detail.</value>
        public DaypartDetailDto DaypartDetail { get; set; } = new DaypartDetailDto();

        /// <summary>
        /// Gets or sets the program network.
        /// </summary>
        /// <value>The program network.</value>
        public string ProgramNetwork { get; set; }

        /// <summary>
        /// Gets or sets the program air time.
        /// </summary>
        /// <value>The program air time.</value>
        public DateTime ProgramAirTime { get; set; }

        /// <summary>
        /// Gets or sets the ingested by.
        /// </summary>
        /// <value>The ingested by.</value>
        public string IngestedBy { get; set; }

        /// <summary>
        /// Gets or sets the ingested at.
        /// </summary>
        /// <value>The ingested at.</value>
        public DateTime IngestedAt { get; set; }

        /// <summary>
        /// Gets or sets the impressions.
        /// </summary>
        /// <value>The impressions.</value>
        public double Impressions { get; set; }

        /// <summary>
        /// Gets or sets the ingested media week identifier.
        /// </summary>
        /// <value>The ingested media week identifier.</value>
        public int IngestedMediaWeekId { get; set; }

        /// <summary>
        /// Gets or sets the plan identifier.
        /// </summary>
        /// <value>The plan identifier.</value>
        public int PlanId { get; set; }

        /// <summary>
        /// Gets or sets the spot exceptions out of spec reason code.
        /// </summary>
        /// <value>The spot exceptions out of spec reason code.</value>
        public SpotExceptionsOutOfSpecReasonCodeDto SpotExceptionsOutOfSpecReasonCode { get; set; } = new SpotExceptionsOutOfSpecReasonCodeDto();

        /// <summary>
        /// Gets or sets the market code.
        /// </summary>
        /// <value>The market code.</value>
        public int? MarketCode { get; set; }

        /// <summary>
        /// Gets or sets the market rank.
        /// </summary>
        /// <value>The market rank.</value>
        public int? MarketRank { get; set; }

        /// <summary>
        /// Gets or sets the house isci.
        /// </summary>
        /// <value>The house isci.</value>
        public string HouseIsci { get; set; }

        /// <summary>
        /// Gets or sets the time zone.
        /// </summary>
        /// <value>The time zone.</value>
        public string TimeZone { get; set; } 

        /// <summary>
        /// Gets or sets the dma.
        /// </summary>
        /// <value>The dma.</value>
        public int DMA { get; set; } = 58; // TODO: Populate this for real

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>The comments.</value>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the name of the inventory source.
        /// </summary>
        /// <value>The name of the inventory source.</value>
        public string InventorySourceName { get; set; }
    }
}

