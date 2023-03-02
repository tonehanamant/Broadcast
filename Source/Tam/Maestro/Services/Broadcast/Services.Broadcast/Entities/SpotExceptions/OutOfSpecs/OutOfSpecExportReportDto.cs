using System;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary>The Out Of Spec Export Report</summary>
    public class OutOfSpecExportReportDto
    {
        /// <summary>
        /// Gets or sets the MarketRank 
        /// </summary>
        /// <value>The MarketRank.</value>
        public int? MarketRank { get; set; }
        /// <summary>
        /// Gets or sets the Market
        /// </summary>
        /// <value>The market</value>
        public string Market { get; set; }
        /// <summary>
        /// Gets or sets the Station
        /// </summary>
        /// <value>The Station </value>
        public string Station { get; set; }
        /// <summary>
        /// Gets or sets the Affiliate
        /// </summary>
        /// <value>The Affiliate</value>
        public string Affiliate { get; set; }
        /// <summary>
        /// Gets or sets the WeekStartDate
        /// </summary>
        /// <value>The WeekStartDate</value>
        public DateTime WeekStartDate { get; set; }
        /// <summary>
        /// Gets or sets the Day
        /// </summary>
        /// <value>The Day</value>
        public string Day { get; set; }
        /// <summary>
        /// Gets or sets the Date
        /// </summary>
        /// <value>The Date</value>
        public DateTime Date { get; set; }
        /// <summary>
        /// Gets or sets the TimeAired
        /// </summary>
        /// <value>The TimeAired</value>
        public string TimeAired { get; set; }
        /// <summary>
        /// Gets or sets the ProgramName
        /// </summary>
        /// <value>The ProgramName</value>
        public string ProgramName { get; set; }
        /// <summary>
        /// Gets or sets the Length
        /// </summary>
        /// <value>The Length</value>
        public int Length { get; set; }
        /// <summary>
        /// Gets or sets the HouseIsci
        /// </summary>
        /// <value>The HouseIsci</value>
        public string HouseIsci { get; set; }
        /// <summary>
        /// Gets or sets the ClientIsci
        /// </summary>
        /// <value>The ClientIsci</value>
        public string ClientIsci { get; set; }
        /// <summary>
        /// Gets or sets the AdvertiserName
        /// </summary>
        /// <value>The AdvertiserName</value>
        public string AdvertiserName { get; set; }
        /// <summary>
        /// Gets or sets the AdvertiserMasterId
        /// </summary>
        /// <value>The AdvertiserMasterId</value>
        public Guid? AdvertiserMasterId { get; set; }
        /// <summary>
        /// Gets or sets the InventorySource
        /// </summary>
        /// <value>The InventorySource</value>
        public string InventorySource { get; set; }
        /// <summary>
        /// Gets or sets the InventorySourceDaypart
        /// </summary>
        /// <value>The InventorySourceDaypart</value>
        public string InventorySourceDaypart { get; set; }
        /// <summary>
        /// Gets or sets the InventoryOutOfSpecReason
        /// </summary>
        /// <value>The InventoryOutOfSpecReason</value>
        public string InventoryOutOfSpecReason { get; set; }
        /// <summary>
        /// Gets or sets the MarketCode
        /// </summary>
        /// <value>The market code</value>
        public int? Estimates { get; set; }
        /// <summary>
        /// Gets or sets the Spot
        /// </summary>
        /// <value>The Spot</value>
        public int Spot { get; set; }
        /// <summary>
        /// Gets or sets the RecommendedPlanId
        /// </summary>
        /// <value>The RecommendedPlanId </value>
        public int? RecommendedPlanId { get; set; }
        /// <summary>
        /// Gets or sets the Comment
        /// </summary>
        /// <value>The Comment</value>
        public string Comment { get; set; }
    }
}
