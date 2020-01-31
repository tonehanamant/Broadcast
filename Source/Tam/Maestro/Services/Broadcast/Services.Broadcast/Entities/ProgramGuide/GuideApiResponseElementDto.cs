using System.Collections.Generic;

namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A response element returned from the external api.
    /// </summary>
    public class GuideApiResponseElementDto
    {
        public string id { get; set; }

        public List<GuideApiResponseProgramDto> programs { get; set; }

        public string daypart { get; set; }

        public string station { get; set; }

        public string affiliate { get; set; }

        public string start_date { get; set; }

        public string end_date { get; set; }
    }
}