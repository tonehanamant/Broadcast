namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// A market for a plan.
    /// </summary>
    public class PlanMarketDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the market code.
        /// </summary>
        /// <value>
        /// The market code.
        /// </value>
        public short MarketCode { get; set; }

        /// <summary>
        /// Gets or sets the market coverage file identifier from which this item was selected.
        /// </summary>
        /// <value>
        /// The market coverage file identifier from which this item was selected..
        /// </value>
        public int MarketCoverageFileId { get; set; }

        /// <summary>
        /// Gets or sets the rank of the market at time of selection.
        /// </summary>
        /// <value>
        /// The rank of the market at time of selection.
        /// </value>
        public int Rank { get; set; }

        /// <summary>
        /// Gets or sets the percentage coverage of the USA of the market at time of selection.
        /// </summary>
        /// <value>
        /// The percentage coverage of the USA of the market at time of selection.
        /// </value>
        public double PercentageOfUS { get; set; }

        /// <summary>
        /// The market name.
        /// </summary>
        public string Market { get; set; }
    }
}