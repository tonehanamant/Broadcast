using System;

namespace Services.Broadcast.Entities
{
    public class ArchivedIscisDto
    {
        public long FileDetailId { get; set; }
        public string ISCI { get; set; }
        public DateTime DateAired { get; set; }
        public int TimeAired { get; set; }
        public int SpotLength { get; set; }
        public string ProgramName { get; set; }
        public string Genre { get; set; }
        public string Station { get; set; }
        public string Market { get; set; }
        public string Affiliate { get; set; }
    }
}
