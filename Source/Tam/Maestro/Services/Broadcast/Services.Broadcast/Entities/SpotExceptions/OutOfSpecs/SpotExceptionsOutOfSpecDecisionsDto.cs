using System;
namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
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
        public string ProgramName { get; set; }
        public string DaypartCode { get; set; }
        public string GenreName { get; set; }
    }
}
		
