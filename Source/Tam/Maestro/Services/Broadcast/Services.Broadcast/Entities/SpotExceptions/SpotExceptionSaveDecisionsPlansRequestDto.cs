using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionSaveDecisionsPlansRequestDto
    {
        public SpotExceptionSaveDecisionsPlansRequestDto()
        {
            Decisions = new List<SpotExceptionsOutOfSpecDecisionsPlansDto>();
        }
        public List<SpotExceptionsOutOfSpecDecisionsPlansDto> Decisions { get; set; }
    }

    public class SpotExceptionsOutOfSpecDecisionsPlansDto
    {
        public int Id { get; set; }
        public bool AcceptAsInSpec { get; set; }
        public string Comments { get; set; }
    }
}
