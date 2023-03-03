using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary></summary>
    public class OutOfSpecDonePlanSpotsDto
    {
       
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the estimate identifier.
        /// </summary>
        /// <value>The estimate identifier.</value>
        public int? EstimateId { get; set; }

        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        /// <value>The reason.</value>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the reason label.
        /// </summary>
        /// <value>The reason label.</value>
        public string ReasonLabel { get; set; }

        /// <summary>
        /// Gets or sets the market rank.
        /// </summary>
        /// <value>The market rank.</value>
        public int? MarketRank { get; set; }

        /// <summary>
        /// Gets or sets the dma.
        /// </summary>
        /// <value>The dma.</value>
        public int DMA { get; set; }

        /// <summary>
        /// Gets or sets the market.
        /// </summary>
        /// <value>The market.</value>
        public string Market { get; set; }

        /// <summary>
        /// Gets or sets the station.
        /// </summary>
        /// <value>The station.</value>
        public string Station { get; set; }

        /// <summary>
        /// Gets or sets the time zone.
        /// </summary>
        /// <value>The time zone.</value>
        public string TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the affiliate.
        /// </summary>
        /// <value>The affiliate.</value>
        public string Affiliate { get; set; }

        /// <summary>
        /// Gets or sets the day.
        /// </summary>
        /// <value>The day.</value>
        public string Day { get; set; }

        /// <summary>
        /// Gets or sets the name of the genre.
        /// </summary>
        /// <value>The name of the genre.</value>
        public string GenreName { get; set; }

        /// <summary>
        /// Gets or sets the house isci.
        /// </summary>
        /// <value>The house isci.</value>
        public string HouseIsci { get; set; }

        /// <summary>
        /// Gets or sets the client isci.
        /// </summary>
        /// <value>The client isci.</value>
        public string ClientIsci { get; set; }

        /// <summary>
        /// Gets or sets the program air date.
        /// </summary>
        /// <value>The program air date.</value>
        public string ProgramAirDate { get; set; }

        /// <summary>
        /// Gets or sets the program air time.
        /// </summary>
        /// <value>The program air time.</value>
        public string ProgramAirTime { get; set; }

        /// <summary>
        /// Gets or sets the flight end date.
        /// </summary>
        /// <value>The flight end date.</value>
        public string FlightEndDate { get; set; }

        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>The name of the program.</value>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the spot length string.
        /// </summary>
        /// <value>The spot length string.</value>
        public string SpotLengthString { get; set; }

        /// <summary>
        /// Gets or sets the daypart code.
        /// </summary>
        /// <value>The daypart code.</value>
        public string DaypartCode { get; set; }

        /// <summary>
        /// Gets or sets the decision string.
        /// </summary>
        /// <value>The decision string.</value>
        public string DecisionString { get; set; }

        /// <summary>
        /// Gets or sets the synced timestamp.
        /// </summary>
        /// <value>The synced timestamp.</value>
        public string SyncedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>The comments.</value>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the plan daypart codes.
        /// </summary>
        /// <value>The plan daypart codes.</value>
        public List<string> PlanDaypartCodes { get; set; }

        /// <summary>
        /// Gets or sets the name of the inventory source.
        /// </summary>
        /// <value>The name of the inventory source.</value>
        public string InventorySourceName { get; set; }
    }
}