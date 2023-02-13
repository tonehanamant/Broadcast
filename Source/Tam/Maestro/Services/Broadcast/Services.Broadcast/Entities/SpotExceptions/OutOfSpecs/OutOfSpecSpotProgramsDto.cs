using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary></summary>
    public class OutOfSpecSpotProgramsDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutOfSpecSpotProgramsDto" /> class.
        /// </summary>
        public OutOfSpecSpotProgramsDto()
        {
            this.Genres = new List<string>();
        }

        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>The name of the program.</value>
        public string ProgramName { get; set; }

        /// <summary>
        /// Gets or sets the genres.
        /// </summary>
        /// <value>The genres.</value>
        public List<string> Genres { get; set; }
    }
}
