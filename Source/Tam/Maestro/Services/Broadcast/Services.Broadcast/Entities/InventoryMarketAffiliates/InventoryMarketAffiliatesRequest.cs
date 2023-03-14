namespace Services.Broadcast.Entities.InventoryMarketAffiliates
{
    public class InventoryMarketAffiliatesRequest:UserInformation
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
