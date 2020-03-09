using System;

namespace Services.Broadcast.Entities.ProgramGuide
{
    /// <summary>
    /// A line in a ProgramGuideInterfaceFile.
    /// </summary>
    public class GuideInterfaceExportElement
    {
        public int inventory_id { get; set; }
        public int inventory_week_id { get; set; }
        public int inventory_daypart_id { get; set; }

        public string station_call_letters { get; set; }
        public string affiliation { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }

        public string daypart_text { get; set; }
        public bool mon { get; set; }
        public bool tue { get; set; }
        public bool wed { get; set; }
        public bool thu { get; set; }
        public bool fri { get; set; }
        public bool sat { get; set; }
        public bool sun { get; set; }
        public int daypart_start_time { get; set; }
        public int daypart_end_time { get; set; }

        public string program_name { get; set; }
        public string show_type { get; set; }
        public string genre { get; set; }
        public int? program_start_time { get; set; }
        public int? program_end_time { get; set; }
        public DateTime? program_start_date { get; set; }
        public DateTime? program_end_date { get; set; }
    }
}