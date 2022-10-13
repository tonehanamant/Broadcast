using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    public class SpotExceptionsOutOfSpecSaveDecisionsRequestDto
    {
        public SpotExceptionsOutOfSpecSaveDecisionsRequestDto()
        {
            Decisions = new List<SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto>();
        }
        public List<SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto> Decisions { get; set; }
    }

    public class SpotExceptionsOutOfSpecDoneDecisionsToSaveRequestDto
    {
        public int Id { get; set; }
        public bool AcceptAsInSpec { get; set; }
        public string Comments { get; set; }
        public string ProgramName { get; set; }
        public string GenreName { get; set; }
        public string DaypartCode { get; set; }
    }
}
