using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Inventory
{
    /// <summary>
    /// Detailed info for an export line.
    /// </summary>
    public class InventoryExportLineDetail
    {
        /// <summary>
        /// The id of the inventory record.
        /// </summary>
        public int InventoryId { get; set; }

        /// <summary>
        /// The station_id.
        /// </summary>
        public int? StationId { get; set; }

        /// <summary>
        /// The daypart_id
        /// </summary>
        public int DaypartId { get; set; }

        /// <summary>
        /// The program name provided with the inventory.
        /// </summary>
        public string InventoryProgramName { get; set; }

        /// <summary>
        /// The list of program names that ran.
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// The source of the program name.
        /// </summary>
        public ProgramSourceEnum? ProgramSource { get; set; }

        /// <summary>
        /// The genre id.
        /// </summary>
        public int? MaestroGenreId { get; set; }

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
        /// The audience impressions provided in the inventory file.
        /// </summary>
        public List<InventoryExportAudienceDto> ProvidedAudienceImpressions { get; set; }

        /// <summary>
        /// The week details.
        /// </summary>
        public List<InventoryExportLineWeekDetail> Weeks { get; set; }
    }
}