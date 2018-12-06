using System;
using System.Collections.Generic;
using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class ScrubbingFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileHash { get; set; }
        public int SourceId { get; set; }
        public DateTime CreatedDate { get; set; }
        public FileProcessingStatusEnum Status { get; set; }
        public List<ScrubbingFileDetail> FileDetails { get; set; } = new List<ScrubbingFileDetail>();
        public List<FileProblem> FileProblems { get; set; } = new List<FileProblem>();
    }
}
