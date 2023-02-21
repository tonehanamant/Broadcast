using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary>
    /// Request class for the out of spec bulk edit
    /// </summary>
    public class SaveOutOfSpecSpotBulkEditRequestDto
    {
        /// <summary>
        ///Defines constructor for the class OutOfSpecBulkEditRequestDto
        /// </summary>
        public SaveOutOfSpecSpotBulkEditRequestDto()
        {
            this.Decisions = new OutOfSpecDecisionsToSaveRequestDto();
        }
        /// <summary>
        /// Gets or sets the SpotIds identifiers.
        /// </summary>
        /// <value>The SpotIds identifiers.</value>
        public List<int> SpotIds { get; set; }
        /// <summary>
        /// Gets or sets the decisions.
        /// </summary>
        /// <value>The decisions.</value>
        public OutOfSpecDecisionsToSaveRequestDto Decisions { get; set; }

    }
    /// <summary>
    /// Request class for the decisions
    /// </summary>
    public class OutOfSpecDecisionsToSaveRequestDto
    {
        /// <summary>
        /// Gets or sets a value indicating whether [accept as in spec].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [accept as in spec]; otherwise, <c>false</c>.</value>
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
