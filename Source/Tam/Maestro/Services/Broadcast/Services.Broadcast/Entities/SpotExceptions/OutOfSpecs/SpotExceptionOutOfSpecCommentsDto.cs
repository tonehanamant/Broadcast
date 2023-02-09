using System;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    public class SpotExceptionOutOfSpecCommentsDto
    {/// <summary>
     /// Gets or sets the spot unique hash external.
     /// </summary>
     /// <value>The spot unique hash external.</value>
        public string SpotUniqueHashExternal { get; set; }

        /// <summary>
        /// Gets or sets the execution identifier external.
        /// </summary>
        /// <value>The execution identifier external.</value>
        public string ExecutionIdExternal { get; set; }

        /// <summary>
        /// Gets or sets the name of the isci.
        /// </summary>
        /// <value>The name of the isci.</value>
        public string IsciName { get; set; }

        /// <summary>
        /// Gets or sets the recommended plan identifier.
        /// </summary>
        /// <value>The recommended plan identifier.</value>
        public int? RecommendedPlanId { get; set; }

        /// <summary>
        /// Gets or sets the program air time.
        /// </summary>
        /// <value>The program air time.</value>
        public DateTime ProgramAirTime
        {
            get; set;

        }
        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>The comments.</value>
        public string Comments { get; set; }
        /// <summary>
        /// Gets or sets the Reason Code identifier.
        /// </summary>
        /// <value>The Reason Code identifier.</value>
        public int ReasonCode { get; set; }
        /// <summary>
        /// Gets or sets the station legacy call letters.
        /// </summary>
        /// <value>The station legacy call letters.</value>
        public string StationLegacyCallLetters { get; set; }
    }
}
