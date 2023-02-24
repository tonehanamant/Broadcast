namespace Services.Broadcast.Entities.SpotExceptions.Unposted
{
    /// <summary></summary>
    public class OutOfSpecUnpostedNoPlanDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

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
        /// Gets or sets the client spot length identifier.
        /// </summary>
        /// <value>The client spot length identifier.</value>
        public int? ClientSpotLength { get; set; }

        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>The count.</value>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the program air time.
        /// </summary>
        /// <value>The program air time.</value>
        public System.DateTime ProgramAirTime { get; set; }

        /// <summary>
        /// Gets or sets the estimate identifier.
        /// </summary>
        /// <value>The estimate identifier.</value>
        public long? EstimateID { get; set; }

        /// <summary>
        /// Gets or sets the ingested by.
        /// </summary>
        /// <value>The ingested by.</value>
        public string IngestedBy { get; set; }

        /// <summary>
        /// Gets or sets the ingested at.
        /// </summary>
        /// <value>The ingested at.</value>
        public System.DateTime IngestedAt { get; set; }

        /// <summary>
        /// Gets or sets the ingest media week identifier.
        /// </summary>
        /// <value>The ingest media week identifier.</value>
        public int IngestMediaWeekId { get; set; }
    }
}
