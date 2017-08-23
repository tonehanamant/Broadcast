using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class GroupedDisplayProposalStationPrograms
    {
        public DisplayScheduleStation Station { get; set; }
        public ProposalTotalFields StationTotals { get; set; }
        public List<ProposalProgramDto> ProposalPrograms { get; set; }        
    }
}
