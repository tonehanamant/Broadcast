using System.Collections.Generic;

namespace Services.Broadcast.Entities.DTO
{
    public class CheckForAllocatedSpotsRequestDto
    {
        public List<int> ProposalDetailIds { get; set; } = new List<int>();
    }
}
