using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Scx
{
    public class ScxOpenMarketFileGenerationDetailDto
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
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>
        /// The file identifier.
        /// </value>
        public int? FileId { get; set; }

        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        /// <value>
        /// The filename.
        /// </value>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the name of the unit.
        /// </summary>
        /// <value>
        /// The name of the unit.
        /// </value>
        public string Affilates { get; set; }

        /// <summary>
        /// Gets or sets the daypart code.
        /// </summary>
        /// <value>
        /// The daypart code.
        /// </value>
        public string DaypartCode { get; set; }

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>
        /// The end date time.
        /// </value>
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the processing status identifier.
        /// </summary>
        /// <value>
        /// The processing status identifier.
        /// </value>
        public int ProcessingStatusId { get; set; }
    }
}
