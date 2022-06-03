using System;

namespace Services.Broadcast.Entities.DTO
{
    public class SpotExceptionsIngestTriggerRequest
    {
        public Guid? RequestId { get; set; }
        public string Username { get; set; }
        public bool RunInBackground { get; set; } 
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
