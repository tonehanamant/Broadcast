namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A request element sent to the external api.
    /// </summary>
    public class GuideApiRequestElementDto
    {
        public string id { get; set; }

        public GuideApiRequestDaypartDto daypart { get; set; }

        public string startdate { get; set; }

        public string enddate { get; set; }

        public string station { get; set; }

        public string affiliate { get; set; }
    }
}