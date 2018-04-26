using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class AffidavitFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileHash { get; set; }
        public int SourceId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int MediaMonthId { get; set; }
        public List<AffidavitFileDetail> AffidavitFileDetails { get; set; }

        public AffidavitFile()
        {
            AffidavitFileDetails = new List<AffidavitFileDetail>();
        }
    }
}
