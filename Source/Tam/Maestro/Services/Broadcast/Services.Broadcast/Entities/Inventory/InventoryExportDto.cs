using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Inventory
{
    public class InventoryExportDto
    {
        /// <summary>
        /// Id of the source inventory manifest record.
        /// </summary>
        public int InventoryId { get; set; }

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
        /// The cost for the 30s spot length.
        /// </summary>
        public decimal SpotCost { get; set; }

        /// <summary>
        /// The name of the program that ran during this inventory.
        /// </summary>
        public string ProgramName { get; set; }

        /// <summary>
        /// The name of the program that the inventory file provided.
        /// </summary>
        public string InventoryProgramName { get; set; }

        /// <summary>
        /// The source of any program mata data.
        /// </summary>
        public ProgramSourceEnum? ProgramSource { get; set; }

        /// <summary>
        /// The genre of the program.
        /// </summary>
        public int? MaestroGenreId { get; set; }

        /// <summary>
        /// The Projected Hh Impressions for the 30s spot length.
        /// </summary>
        public double? HhImpressionsProjected { get; set; }

        /// <summary>
        /// The audiences provided with the inventory
        /// </summary>
        public List<InventoryExportAudienceDto> ProvidedAudiences { get; set; }
    }
}