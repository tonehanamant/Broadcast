using System;

namespace Services.Broadcast.Entities
{
    public class BvsFileSummary
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime UploadDate { get; set; }
        public int RecordCount { get; set; }
    }
}