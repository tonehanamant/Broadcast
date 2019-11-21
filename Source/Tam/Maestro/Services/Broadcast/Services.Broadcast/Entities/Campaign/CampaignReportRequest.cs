using System.Collections.Generic;

namespace Services.Broadcast.Entities.Campaign
{
    public class CampaignReportRequest
    {
        public int CampaignId { get; set; }
        public List<int> SelectedPlans { get; set; } = new List<int>();
    }
}
