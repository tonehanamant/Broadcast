using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.Campaign
{
    public class CampaignReportData
    {
        private const string DATE_FORMAT = "MM/dd/yyyy";
        private const string DATE_FORMAT_SHORT_YEAR = "MM/dd/yy";

        public CampaignReportData(CampaignExportTypeEnum exportType, CampaignDto campaign
            , List<PlanDto> plans, AgencyDto agency, AdvertiserDto advertiser
            , List<PlanAudienceDisplay> guaranteedDemos
            , List<LookupDto> spotLenghts
            , IQuarterCalculationEngine _QuarterCalculationEngine)
        {            
            CampaignStartQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(campaign.FlightStartDate).ShortFormat();
            CampaignEndQuarter = _QuarterCalculationEngine.GetQuarterRangeByDate(campaign.FlightEndDate).ShortFormat();
            CampaignName = campaign.Name;
            CreatedDate = DateTime.Now.ToString(DATE_FORMAT);
            AgencyName = agency.Name;
            ClientName = advertiser.Name;
            CampaignFlightStartDate = campaign.FlightStartDate != null ? campaign.FlightStartDate.Value.ToString(DATE_FORMAT_SHORT_YEAR) : string.Empty;
            CampaignFlightEndDate = campaign.FlightEndDate != null ? campaign.FlightEndDate.Value.ToString(DATE_FORMAT_SHORT_YEAR) : string.Empty;
            GuaranteedDemo = string.Join(",", guaranteedDemos.Select(x => x.Code).ToList());
            SpotLengths = plans
                .Select(x => new { x.SpotLengthId, x.Equivalized })
                .Distinct()
                .Select (x => new { x.Equivalized, spotLenghts.Single(y => y.Id == x.SpotLengthId).Display })
                .OrderBy(x=>x.Display)
                .Select(x => $":{x.Display}{(x.Equivalized ? " eq." : string.Empty)}")
                .ToList();
            PostingType = plans.Select(x => x.PostingType).Distinct().Single().ToString();
            Status = exportType.Equals(CampaignExportTypeEnum.Contract) ? "Order" : "Proposal";   
        }

        public string CampaignName { get; set; }
        public string CreatedDate { get; set; }
        public string CampaignStartQuarter { get; set; }
        public string CampaignEndQuarter { get; set; }
        public string AgencyName { get; set; }
        public string ClientName { get; set; }
        public string CampaignFlightStartDate { get; set; }
        public string CampaignFlightEndDate { get; set; }
        public string GuaranteedDemo { get; set; }
        public List<string> SpotLengths { get; set; }
        public string PostingType { get; set; }
        public string Status { get; set; }
    }
}
