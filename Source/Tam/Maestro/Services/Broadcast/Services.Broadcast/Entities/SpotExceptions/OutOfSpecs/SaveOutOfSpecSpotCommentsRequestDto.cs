using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary></summary>
    public class SaveOutOfSpecSpotCommentsRequestDto
    {

        /// <summary>
        /// Gets or sets the spot ids.
        /// </summary>
        /// <value>The spot ids.</value>
        public List<int> SpotIds { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>The comments.</value>
        public string Comment { get; set; }
    }
}
