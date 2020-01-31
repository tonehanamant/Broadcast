using System;

namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A request element for the internal client.
    /// </summary>
    public class GuideRequestElementDto
    {
        public string Id { get; set; }

        public GuideRequestDaypartDto Daypart { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string StationCallLetters { get; set; }

        public string NetworkAffiliate { get; set; }
    }
}