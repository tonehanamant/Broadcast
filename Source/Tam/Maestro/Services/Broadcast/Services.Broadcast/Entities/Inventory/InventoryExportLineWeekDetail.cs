namespace Services.Broadcast.Entities.Inventory
{
    /// <summary>
    /// Detailed info for a week of an export line.
    /// </summary>
    public class InventoryExportLineWeekDetail
    {
        /// <summary>
        /// The media_week_id
        /// </summary>
        public int MediaWeekId { get; set; }

        /// <summary>
        /// The cost of the spot.
        /// </summary>
        public decimal SpotCost { get; set; }

        /// <summary>
        /// The Hh impressions.
        /// </summary>
        public double HhImpressions { get; set; }

        /// <summary>
        /// The CPM.
        /// </summary>
        public decimal Cpm { get; set; }
    }
}