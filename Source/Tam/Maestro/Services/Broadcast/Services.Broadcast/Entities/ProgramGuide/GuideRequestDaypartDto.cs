namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A request daypart sent to the internal client.
    /// </summary>
    public class GuideRequestDaypartDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool Monday { get; set; }

        public bool Tuesday { get; set; }

        public bool Wednesday { get; set; }

        public bool Thursday { get; set; }

        public bool Friday { get; set; }

        public bool Saturday { get; set; }

        public bool Sunday { get; set; }

        /// <summary>
        /// Seconds since midnight.
        /// </summary>
        public int StartTime { get; set; }

        /// <summary>
        /// Seconds since midnight.
        /// </summary>
        public int EndTime { get; set; }
    }
}