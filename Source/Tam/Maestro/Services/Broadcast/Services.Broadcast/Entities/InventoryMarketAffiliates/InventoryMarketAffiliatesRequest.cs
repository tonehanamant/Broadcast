namespace Services.Broadcast.Entities.InventoryMarketsAffiliates
{
    public class InventoryMarketAffiliatesReportRequest
    {
        /// <summary>
        /// Gets or sets the inventory source identifier.
        /// </summary>
        /// <value>The inventory source identifier.</value>
        public int InventorySourceId { get; set; }
        /// <summary>
        /// Gets or sets the quarter.
        /// </summary>
        /// <value>The quarter.</value>
        public QuarterDetailDto Quarter { get; set; }
    }
}
