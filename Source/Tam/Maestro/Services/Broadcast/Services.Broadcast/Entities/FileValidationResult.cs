using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class FileValidationResult
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public ProcessingStatusEnum Status { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public string FileHash { get; set; }
        public FileSourceEnum Source { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
