using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class OutboundPostLogFileValidationResult
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public PostLogProcessingStatusEnum Status { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public string FileHash { get; set; }
        public PostLogFileSourceEnum Source { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
