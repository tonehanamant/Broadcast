using System;
using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities.Plan
{
    /// <summary>
    /// A dto for a Plan.
    /// </summary>
    public class PlanDto
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the campaign identifier.
        /// </summary>
        /// <value>
        /// The campaign identifier.
        /// </value>
        public int CampaignId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the spot length identifier.
        /// </summary>
        /// <value>
        /// The spot length identifier.
        /// </value>
        public int SpotLengthId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PlanDto"/> is equivalized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if equivalized; otherwise, <c>false</c>.
        /// </value>
        public bool Equivalized { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public PlanStatusEnum Status { get; set; }

        /// <summary>
        /// Gets or sets the product identifier.
        /// </summary>
        /// <value>
        /// The product identifier.
        /// </value>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the flight start date.
        /// </summary>
        /// <value>
        /// The flight start date.
        /// </value>
        public DateTime? FlightStartDate { get; set; }

        /// <summary>
        /// Gets or sets the flight end date.
        /// </summary>
        /// <value>
        /// The flight end date.
        /// </value>
        public DateTime? FlightEndDate { get; set; }

        /// <summary>
        /// Gets or sets the flight notes.
        /// </summary>
        /// <value>
        /// The flight notes.
        /// </value>
        public string FlightNotes { get; set; }

        /// <summary>
        /// Gets or sets the flight hiatus days.
        /// </summary>
        /// <value>
        /// The flight hiatus days.
        /// </value>
        public List<DateTime> FlightHiatusDays { get; set; } = new List<DateTime>();

        /// <summary>
        /// Gets or sets the audience identifier.
        /// </summary>
        /// <value>
        /// The audience identifier.
        /// </value>
        public int AudienceId { get; set; }

        /// <summary>
        /// Gets or sets the type of the audience.
        /// </summary>
        /// <value>
        /// The type of the audience.
        /// </value>
        public AudienceTypeEnum AudienceType { get; set; }

        /// <summary>
        /// Gets or sets the type of the posting.
        /// </summary>
        /// <value>
        /// The type of the posting.
        /// </value>
        public PostingTypeEnum PostingType { get; set; }

        /// <summary>
        /// Gets or sets the share book identifier.
        /// </summary>
        /// <value>
        /// The share book identifier.
        /// </value>
        public int ShareBookId { get; set; }

        /// <summary>
        /// Gets or sets the hut book identifier.
        /// </summary>
        /// <value>
        /// The hut book identifier.
        /// </value>
        public int? HUTBookId { get; set; }
    }
}
