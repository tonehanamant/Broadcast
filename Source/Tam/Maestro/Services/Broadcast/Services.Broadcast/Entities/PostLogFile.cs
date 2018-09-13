using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class PostLogFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileHash { get; set; }
        public int SourceId { get; set; }
        public DateTime CreatedDate { get; set; }
        public FileProcessingStatusEnum Status { get; set; }
        public List<PostLogFileDetail> FileDetails { get; set; } = new List<PostLogFileDetail>();
        public List<WWTVFileProblem> FileProblems { get; set; } = new List<WWTVFileProblem>();
    }
}
