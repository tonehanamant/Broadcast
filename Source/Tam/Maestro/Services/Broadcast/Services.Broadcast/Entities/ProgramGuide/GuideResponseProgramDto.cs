using System;

namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A response program returned from the internal client.
    /// </summary>
    public class GuideResponseProgramDto
    {
        public string ProgramName { get; set; }

        public string SourceGenre { get; set; }

        public string ShowType { get; set; }

        public string SyndicationType { get; set; }

        public int Occurrences { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        
        /// <summary>
        /// Seconds since midnight.
        /// </summary>
        public int StartTime { get; set; }

        /// <summary>
        /// Seconds since midnight.
        /// </summary>
        public int EndTime { get; set; }

        public bool Monday { get; set; }

        public bool Tuesday { get; set; }

        public bool Wednesday { get; set; }

        public bool Thursday { get; set; }

        public bool Friday { get; set; }

        public bool Saturday { get; set; }

        public bool Sunday { get; set; }
    }
}