using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    public class SpotExceptionsOutOfSpecDoneDecisionsDto
    {
        public int Id { get; set; }
        public int SpotExceptionsOutOfSpecDoneId { get; set; }
        public bool AcceptedAsInSpec { get; set; }
        public string DecisionNotes { get; set; }
        public string ProgramName { get; set; }
        public string GenreName { get; set; }
        public string DaypartCode { get; set; }
        public string DecidedBy { get; set; }
        public DateTime DecidedAt { get; set; }
        public string SyncedBy { get; set; }
        public DateTime? SyncedAt { get; set; }
    }

    public class SpotExceptionsOutOfSpecDoneDecisionsWithRequestDto
    {
        public SpotExceptionsOutOfSpecDoneDecisionsWithRequestDto()
        {
            DecisionRequest = new SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto();
        }
        public int Id { get; set; }
        public int SpotExceptionsOutOfSpecDoneId { get; set; }
        public bool AcceptedAsInSpec { get; set; }
        public string DecisionNotes { get; set; }
        public string ProgramName { get; set; }
        public string GenreName { get; set; }
        public string DaypartCode { get; set; }
        public string DecidedBy { get; set; }
        public DateTime DecidedAt { get; set; }
        public string SyncedBy { get; set; }
        public DateTime? SyncedAt { get; set; }

        public SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto DecisionRequest { get; set; }
    }
}
