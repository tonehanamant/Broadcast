using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A response element returned from the internal client.
    /// </summary>
    public class GuideResponseElementDto
    {
        public string RequestElementId { get; set; }

        public List<GuideResponseProgramDto> Programs { get; set; }

        public string RequestDaypartId { get; set; }

        public string Station { get; set; }

        public string Affiliate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}