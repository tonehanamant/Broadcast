using System;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary></summary>
    public class OutOfSpecSpotDoneDecisionsDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the spot exceptions out of spec done identifier.
        /// </summary>
        /// <value>The spot exceptions out of spec done identifier.</value>
        public int SpotExceptionsOutOfSpecDoneId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [accepted as in spec].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [accepted as in spec]; otherwise, <c>false</c>.</value>
        public bool AcceptedAsInSpec { get; set; }

        /// <summary>
        /// Gets or sets the decision notes.
        /// </summary>
        /// <value>The decision notes.</value>
        public string DecisionNotes { get; set; }

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

        /// <summary>
        /// Gets or sets the decided by.
        /// </summary>
        /// <value>The decided by.</value>
        public string DecidedBy { get; set; }

        /// <summary>
        /// Gets or sets the decided at.
        /// </summary>
        /// <value>The decided at.</value>
        public DateTime DecidedAt { get; set; }

        /// <summary>
        /// Gets or sets the synced by.
        /// </summary>
        /// <value>The synced by.</value>
        public string SyncedBy { get; set; }

        /// <summary>
        /// Gets or sets the synced at.
        /// </summary>
        /// <value>The synced at.</value>
        public DateTime? SyncedAt { get; set; }
    }
}
