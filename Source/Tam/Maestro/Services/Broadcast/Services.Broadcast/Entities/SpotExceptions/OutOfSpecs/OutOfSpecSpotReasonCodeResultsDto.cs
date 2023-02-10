namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary></summary>
    public class OutOfSpecSpotReasonCodeResultsDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the reason code.
        /// </summary>
        /// <value>The reason code.</value>
        public int ReasonCode { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>The label.</value>
        public string Label { get; set; }
        /// <summary>
        /// Gets or sets the count of spot exceptions oos
        /// </summary>
        public int Count { get; set; }
    }
}
