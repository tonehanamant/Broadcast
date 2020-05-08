namespace Services.Broadcast.Entities.Inventory
{
    public class InventoryExportDto
    {
        /// <summary>
        /// Id of the source inventory manifest record.
        /// </summary>
        public int? InventoryId { get; set; }

        /// <summary>
        /// The station_id.
        /// </summary>
        public int? StationId { get; set; }

        /// <summary>
        /// The media_week_id
        /// </summary>
        public int MediaWeekId { get; set; }

        /// <summary>
        /// The daypart_id
        /// </summary>
        public int DaypartId { get; set; }

        /// <summary>
        /// The Hh Impressions for the 30s spot length.
        /// </summary>
        public double? Impressions { get; set; }

        /// <summary>
        /// The cost for the 30s spot length.
        /// </summary>
        public decimal SpotCost { get; set; }

        /// <summary>
        /// The name of the program that ran during this inventory.
        /// </summary>
        public string ProgramName { get; set; }
    }
}