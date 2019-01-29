using System.Collections.Generic;

namespace Services.Broadcast.Entities.Nti
{
    public class NtiProposalVersionDetailWeek
    {
        public int WeekId { get; set; }
        public Dictionary<int, double> NsiImpressions { get; set; }
        public List<NtiComponentAudiencesImpressions> Audiences { get; set; } = new List<NtiComponentAudiencesImpressions>();
    }
}
