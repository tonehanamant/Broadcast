using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Campaign
{
    public class CampaignReportData
    {
        
        public CampaignReportData(CampaignDto campaign, List<PlanDto> plans, IQuarterCalculationEngine _QuarterCalculationEngine)
        {            
            CampaignStartQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(campaign.FlightStartDate).ShortFormat();
            CampaignEndQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(campaign.FlightEndDate).ShortFormat();
            CampaignName = campaign.Name;
            CreatedDate = DateTime.Now.ToString("MM/dd/yyyy");

        }


        public string CampaignName { get; set; }
        public string CreatedDate { get; set; }
        public string CampaignStartQuarter { get; set; }
        public string CampaignEndQuarter { get; set; }
    }
}
