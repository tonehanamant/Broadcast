using System;
namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsOutOfSpecDecisionsDto
    {
        public int Id { get; set; }
        public int SpotExceptionsOutOfSpecId { get; set; }
        public bool AcceptedAsInSpec { get; set; }
        public string DecisionNotes { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SyncedBy { get; set; }
        public DateTime? SyncedAt { get; set; }
    }
}
		
