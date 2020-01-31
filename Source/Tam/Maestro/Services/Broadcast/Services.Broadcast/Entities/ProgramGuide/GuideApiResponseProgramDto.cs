using System;

namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A response program returned from the external api.
    /// </summary>
    public class GuideApiResponseProgramDto
    {
        public string program_id { get; set; }

        public string program { get; set; }

        public string genreid { get; set; }

        public string genre { get; set; }

        public string showtype { get; set; }

        public string syndicationtype { get; set; }

        public int occurances { get; set; }

        public DateTime startdate { get; set; }

        public DateTime enddate { get; set; }

        public string starttime { get; set; }

        public string endtime { get; set; }

        public bool mon { get; set; }

        public bool tue { get; set; }

        public bool wed { get; set; }

        public bool thu { get; set; }

        public bool fri { get; set; }

        public bool sat { get; set; }

        public bool sun { get; set; }
    }
}