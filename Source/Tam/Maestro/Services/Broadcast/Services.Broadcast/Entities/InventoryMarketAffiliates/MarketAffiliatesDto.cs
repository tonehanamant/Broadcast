namespace Services.Broadcast.Entities.InventoryMarketAffiliates
{
    public class MarketAffiliatesDto
    {
        /// <summary>
        /// Gets or sets the name of the market.
        /// </summary>
        /// <value>The name of the market.</value>
        public string marketName { get; set; }

        /// <summary>
        /// Gets or sets the market code.
        /// </summary>
        /// <value>The market code.</value>
        public short? marketCode { get; set; }

        /// <summary>
        /// Gets or sets the rank.
        /// </summary>
        /// <value>The rank.</value>
        public int rank { get; set; }

        /// <summary>
        /// Gets or sets the affiliation.
        /// </summary>
        /// <value>The affiliation.</value>
        public string affiliation { get; set; }
    }
}
