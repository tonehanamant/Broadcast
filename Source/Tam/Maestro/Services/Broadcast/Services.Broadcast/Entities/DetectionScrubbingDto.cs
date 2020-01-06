using System.Collections.Generic;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class DetectionScrubbingDto
    {
        public string ScheduleName { get; set; }
        public int? EstimateId { get; set; }
        public int CurrentPostingBookId { get; set; }
        public string ISCIs { get; set; }
        
        public List<LookupDto> PostingBooks { get; set; }
        public List<DetectionTrackingDetail> DetectionDetails { get; set; }

        public List<LookupDto> SchedulePrograms { get; set; }
        public List<LookupDto> ScheduleNetworks { get; set; }
    }

    
}
