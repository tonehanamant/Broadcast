using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Scx
{
    public class ScxOpenMarketFileGenerationDetail
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
        public List<string> Affiliates { get; set; }

        /// <summary>
        /// Gets or sets the daypart code.
        /// </summary>
        /// <value>
        /// The daypart code.
        /// </value>
        public List<string> DaypartCodes { get; set; }

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
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>
        /// The file identifier.
        /// </value>
        public int? FileId { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }
    }
}
