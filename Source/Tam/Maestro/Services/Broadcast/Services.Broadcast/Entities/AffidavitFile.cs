using System;
using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class AffidavitFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileHash { get; set; }
        public int SourceId { get; set; }
        public DateTime CreatedDate { get; set; }
        public FileProcessingStatusEnum Status { get; set; }
        public List<AffidavitFileDetail> AffidavitFileDetails { get; set; } = new List<AffidavitFileDetail>();
        public List<WWTVFileProblem> AffidavitFileProblems { get; set; } = new List<WWTVFileProblem>();
    }
}
