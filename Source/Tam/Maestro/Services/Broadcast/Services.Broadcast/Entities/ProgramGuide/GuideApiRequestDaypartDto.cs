namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A request daypart sent to the external api.
    /// </summary>
    public class GuideApiRequestDaypartDto
    {
        public string id { get; set; }

        public string dayparttext { get; set; }

        public bool mon { get; set; }

        public bool tue { get; set; }

        public bool wed { get; set; }

        public bool thu { get; set; }

        public bool fri { get; set; }

        public bool sat { get; set; }

        public bool sun { get; set; }

        /// <summary>
        /// Seconds since midnight.
        /// </summary>
        public int starttime { get; set; }

        /// <summary>
        /// Seconds since midnight.
        /// </summary>
        public int endtime { get; set; }
    }
}