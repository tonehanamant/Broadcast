using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary>
    /// Out of spec edit request class
    /// </summary>
    public class OutOfSpecEditRequestDto
    {
        /// <summary>
        /// Gets or sets the spotid identifier.
        /// </summary>
        /// <value>The SpotId identifier.</value>
        public int SpotId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [accept as in spec].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [accept as in spec]; otherwise, <c>false</c>.</value>
        public bool AcceptAsInSpec { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>The comments.</value>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>The name of the program.</value>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the name of the genre.
        /// </summary>
        /// <value>The name of the genre.</value>
        public string GenreName { get; set; }

        /// <summary>
        /// Gets or sets the daypart code.
        /// </summary>
        /// <value>The daypart code.</value>
        public string DaypartCode { get; set; }
    }
}
