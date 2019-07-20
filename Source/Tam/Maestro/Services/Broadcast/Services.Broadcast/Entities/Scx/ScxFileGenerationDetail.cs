using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Scx
{
    /// <summary>
    /// Details of a generated Scx File.
    /// </summary>
    public class ScxFileGenerationDetail
    {
        /// <summary>
        /// Gets or sets the generation request date time.
        /// </summary>
        /// <value>
        /// The generation request date time.
        /// </value>
        public DateTime GenerationRequestDateTime { get; set; }

        /// <summary>
        /// Gets or sets the generation requested by username.
        /// </summary>
        /// <value>
        /// The generation requested by username.
        /// </value>
        public string GenerationRequestedByUsername { get; set; }

        /// <summary>
        /// Gets or sets the name of the unit.
        /// </summary>
        /// <value>
        /// The name of the unit.
        /// </value>
        public string UnitName { get; set; }

        /// <summary>
        /// Gets or sets the daypart code identifier.
        /// </summary>
        /// <value>
        /// The daypart code identifier.
        /// </value>
        public int DaypartCodeId { get; set; }

        /// <summary>
        /// Gets or sets the name of the daypart code.
        /// </summary>
        /// <value>
        /// The name of the daypart code.
        /// </value>
        public string DaypartCodeName { get; set; }

        /// <summary>
        /// Gets or sets the quarter details.
        /// </summary>
        /// <value>
        /// The quarter details.
        /// </value>
        public List<QuarterDetailDto> QuarterDetails { get; set; }

        /// <summary>
        /// Gets or sets the processing status.
        /// </summary>
        /// <value>
        /// The processing status.
        /// </value>
        public BackgroundJobProcessingStatus ProcessingStatus { get; set; }

        /// <summary>
        /// Gets or sets the full file path.
        /// </summary>
        /// <value>
        /// The full file path.
        /// </value>
        public string FullFilePath { get; set; }
    }
}