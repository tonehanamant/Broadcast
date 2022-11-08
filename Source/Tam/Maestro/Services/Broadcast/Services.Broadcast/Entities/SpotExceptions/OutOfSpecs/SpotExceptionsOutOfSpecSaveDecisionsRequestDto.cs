using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary></summary>
    public class SpotExceptionsOutOfSpecSaveDecisionsRequestDto
    {
        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="SpotExceptionsOutOfSpecSaveDecisionsRequestDto" /> class.
        /// </summary>
        public SpotExceptionsOutOfSpecSaveDecisionsRequestDto()
        {
            Decisions = new List<SpotExceptionsOutOfSpecDecisionsToSaveRequestDto>();
        }

        /// <summary>
        /// Gets or sets the decisions.
        /// </summary>
        /// <value>The decisions.</value>
        public List<SpotExceptionsOutOfSpecDecisionsToSaveRequestDto> Decisions { get; set; }
    }

    public class SpotExceptionsOutOfSpecDecisionsToSaveRequestDto
    {
        /// <summary>
        /// Gets or sets the todo identifier.
        /// </summary>
        /// <value>The todo identifier.</value>
        public int? TodoId { get; set; }

        /// <summary>
        /// Gets or sets the done identifier.
        /// </summary>
        /// <value>The done identifier.</value>
        public int? DoneId { get; set; }

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
