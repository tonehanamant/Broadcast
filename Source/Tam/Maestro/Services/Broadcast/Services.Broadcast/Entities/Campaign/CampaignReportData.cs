using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Entities.Campaign
{
    public class CampaignReportData
    {
        private const string DATE_FORMAT_SHORT_YEAR = "MM/dd/yy";

        public CampaignReportData(CampaignExportTypeEnum exportType, CampaignDto campaign
            , List<PlanDto> plans, AgencyDto agency, AdvertiserDto advertiser
            , List<PlanAudienceDisplay> guaranteedDemos
            , List<LookupDto> spotLenghts
            , List<PlanAudienceDisplay> orderedAudiences
            , IQuarterCalculationEngine _QuarterCalculationEngine)
        {
            CampaignName = campaign.Name;
            CreatedDate = DateTime.Now.ToString(DATE_FORMAT_SHORT_YEAR);
            AgencyName = agency.Name;
            ClientName = advertiser.Name;
            CampaignFlightStartDate = campaign.FlightStartDate != null ? campaign.FlightStartDate.Value.ToString(DATE_FORMAT_SHORT_YEAR) : string.Empty;
            CampaignFlightEndDate = campaign.FlightEndDate != null ? campaign.FlightEndDate.Value.ToString(DATE_FORMAT_SHORT_YEAR) : string.Empty;

            var orderedAudiencesId = orderedAudiences.Select(x => x.Id).ToList();
            GuaranteedDemo = guaranteedDemos
                .OrderBy(x=> orderedAudiencesId.IndexOf(x.Id))
                .Select(x => x.Code)
                .ToList();

            SpotLengths = plans
                .Select(x => new { spotLenghts.Single(y => y.Id == x.SpotLengthId).Display, x.Equivalized })
                .Distinct()
                .OrderBy(x => int.Parse(x.Display))
                .Select(x => $":{x.Display}{(x.Equivalized ? " eq." : string.Empty)}")
                .ToList();

            PostingType = plans.Select(x => x.PostingType).Distinct().Single().ToString();
            Status = exportType.Equals(CampaignExportTypeEnum.Contract) ? "Order" : "Proposal";
        }

        public string CampaignName { get; set; }
        public string CreatedDate { get; set; }
        public string AgencyName { get; set; }
        public string ClientName { get; set; }
        public string CampaignFlightStartDate { get; set; }
        public string CampaignFlightEndDate { get; set; }
        public List<string> GuaranteedDemo { get; set; }
        public List<string> SpotLengths { get; set; }
        public string PostingType { get; set; }
        public string Status { get; set; }
    }
}
