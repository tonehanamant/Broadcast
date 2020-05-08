using System.Collections.Generic;

namespace Services.Broadcast.Entities.Inventory
{
    /// <summary>
    /// Detailed info for an export line.
    /// </summary>
    public class InventoryExportLineDetail
    {
        /// <summary>
        /// The station_id.
        /// </summary>
        public int StationId { get; set; }

        /// <summary>
        /// The daypart_id
        /// </summary>
        public int DaypartId { get; set; }

        /// <summary>
        /// The list of program names that ran.
        /// </summary>
        public List<string> ProgramNames { get; set; }

        /// <summary>
        /// The average spot cost.
        /// </summary>
        public decimal AvgSpotCost { get; set; }

        /// <summary>
        /// The average Hh impressions.
        /// </summary>
        public double AvgHhImpressions { get; set; }

        /// <summary>
        /// The average CPM.
        /// </summary>
        public decimal AvgCpm { get; set; }

        /// <summary>
        /// The week details.
        /// </summary>
        public List<InventoryExportLineWeekDetail> Weeks { get; set; }
    }
}