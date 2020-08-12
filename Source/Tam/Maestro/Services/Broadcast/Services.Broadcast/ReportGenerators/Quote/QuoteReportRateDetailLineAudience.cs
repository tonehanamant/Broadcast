namespace Services.Broadcast.ReportGenerators.Quote
{
    public class QuoteReportRateDetailLineAudience
    {
        /// <summary>
        /// True if this is the primary demo.
        /// </summary>
        public bool IsPrimaryDemo { get; set; }

        /// <summary>
        /// True to indicate that this is the House Holds demo.
        /// </summary>
        public bool IsHouseHolds { get; set; }

        /// <summary>
        /// The Id for the audience.
        /// </summary>
        public int AudienceId { get; set; }

        /// <summary>
        /// The CPM for the demo.
        /// </summary>
        public decimal CPM { get; set; }

        /// <summary>
        /// The impressions for the demo.
        /// </summary>
        public double Impressions { get; set; }
    }
}