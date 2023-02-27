using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.Unposted
{
    /// <summary></summary>
    public class OutOfSpecUnpostedResultsDto
    {
        /// <summary>Initializes a new instance of the <see cref="OutOfSpecUnpostedResultsDto" /> class.</summary>
        public OutOfSpecUnpostedResultsDto()
        {
            NoPlan = new List<OutOfSpecNoPlanDto>();
            NoReelRoster = new List<OutOfSpecNoReelRosterDto>();
        }

        /// <summary>
        /// Gets or sets the no plan.
        /// </summary>
        /// <value>The no plan.</value>
        public List<OutOfSpecNoPlanDto> NoPlan { get; set; }

        /// <summary>
        /// Gets or sets the no reel roster.
        /// </summary>
        /// <value>The no reel roster.</value>
        public List<OutOfSpecNoReelRosterDto> NoReelRoster { get; set; }

    }

    /// <summary></summary>
    public class OutOfSpecNoPlanDto
    {
        /// <summary>
        /// Gets or sets the house isci.
        /// </summary>
        /// <value>The house isci.</value>
        public string HouseIsci { get; set; }

        /// <summary>
        /// Gets or sets the client isci.
        /// </summary>
        /// <value>The client isci.</value>
        public string ClientIsci { get; set; }

        /// <summary>
        /// Gets or sets the length of the client spot.
        /// </summary>
        /// <value>The length of the client spot.</value>
        public string ClientSpotLength { get; set; }

        /// <summary>
        /// Gets or sets the affected spots count.
        /// </summary>
        /// <value>The affected spots count.</value>
        public int AffectedSpotsCount { get; set; }

        /// <summary>
        /// Gets or sets the program air date.
        /// </summary>
        /// <value>The program air date.</value>
        public string ProgramAirDate { get; set; }

        /// <summary>
        /// Gets or sets the estimate identifier.
        /// </summary>
        /// <value>The estimate identifier.</value>
        public int? EstimateId { get; set; }
    }

    /// <summary></summary>
    public class OutOfSpecNoReelRosterDto
    {
        /// <summary>
        /// Gets or sets the house isci.
        /// </summary>
        /// <value>The house isci.</value>
        public string HouseIsci { get; set; }

        /// <summary>
        /// Gets or sets the affected spots count.
        /// </summary>
        /// <value>The affected spots count.</value>
        public int AffectedSpotsCount { get; set; }

        /// <summary>
        /// Gets or sets the program air date.
        /// </summary>
        /// <value>The program air date.</value>
        public string ProgramAirDate { get; set; }

        /// <summary>
        /// Gets or sets the estimate identifier.
        /// </summary>
        /// <value>The estimate identifier.</value>
        public int? EstimateId { get; set; }
    }
}
