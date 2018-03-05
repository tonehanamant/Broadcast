using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class OutboundAffidavitFileValidationResultDto
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int Status { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public string FileHash { get; set; }
        public int SourceId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate{ get; set; }
    }
}
