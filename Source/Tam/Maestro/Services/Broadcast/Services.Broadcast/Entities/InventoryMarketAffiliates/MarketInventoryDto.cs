namespace Services.Broadcast.Entities.InventoryMarketAffiliates
{
    public class MarketInventoryDto
    {
        /// <summary>
        /// Gets or sets the market code.
        /// </summary>
        /// <value>The market code.</value>
        public short? marketCode { get; set; }

        /// <summary>
        /// Gets or sets the affiliation.
        /// </summary>
        /// <value>The affiliation.</value>
        public string affiliation { get; set; }

        /// <summary>
        /// Gets or sets the is inventory.
        /// </summary>
        /// <value>The is inventory.</value>
        public string isInventory { get; set; }
    }
}
