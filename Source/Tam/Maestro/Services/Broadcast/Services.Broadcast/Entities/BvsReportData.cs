using Services.Broadcast.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Entities
{
    public class BvsReportData
    {
        public BvsReportData()
        {
            AudienceImpressions = new List<AudienceImpressionsAndDelivery>();
        }

        public int Rank { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string Affiliate { get; set; }
        public int SpotLength { get; set; }
        public string ProgramName { get; set; }
        public DisplayDaypart DisplayDaypart { get; set; }
        public string Isci { get; set; }
        public double? Cost { get; set; }
        public int OrderedSpots { get; set; }
        public int? DeliveredSpots { get; set; }
        public double? SpotClearance { get; set; }
        public string SpecStatus { get; set; }
        public List<AudienceImpressionsAndDelivery> AudienceImpressions { get; set; }
        public int? MediaWeekId { get; set; }
        public int Status { get; set; }
        public int? OutOfSpecSpots { get; set; }
        public double GetDeliveredImpressions(int audienceId)
        {
            return AudienceImpressions.Where(ai => ai.AudienceId == audienceId).Sum(ai => ai.Delivery);
        }
        public double GetOrderedImpressions(int audienceId)
        {
            var impressionData = AudienceImpressions.Where(ai => ai.AudienceId == audienceId).ToList();
            return impressionData.Sum(x => x.Impressions);
        }
    }

    public class BvsReportOutOfSpecData : BvsReportData
    {
        public DateTime DateAired { get; set; }
        public bool MatchStation { get; set; }
        public bool MatchProgram { get; set; }
        public bool MatchAirTime { get; set; }
        public bool MatchIsci { get; set; }
        public bool MatchSpotLength { get; set; }
    }


    public class BvsReportDataContainer
    {
        public BvsReportDataContainer()
        {
            ReportData = new List<BvsReportData>();
            ImpressionsAndDelivery = new List<ImpressionAndDeliveryDto>();
        }
        /// <summary>
        /// Contains in and out of spec data
        /// </summary>
        public List<BvsReportData> ReportData { get; set; }

        public List<ImpressionAndDeliveryDto> ImpressionsAndDelivery { get; private set; }

        public List<BvsReportData> GetInSpec()
        {
            return ReportData.Where(rd => !(rd is BvsReportOutOfSpecData)).ToList();
        }
        public List<BvsReportData> GetOutOfSpec()
        {
            return ReportData.Where(rd => (rd is BvsReportOutOfSpecData)).ToList();
        }

        public List<BvsReportData> GetReportData(bool inSpec)
        {
            return inSpec ? GetInSpec() : GetOutOfSpec();
        }

        // only returns inspec
        public int? OrderedSpots()
        {
            return GetInSpec().Sum(x => x.OrderedSpots);
        }
        public int? DeliveredSpots(bool inSpec)
        {
            var reportData = inSpec ? GetInSpec() : GetOutOfSpec();
            return reportData.Sum(x => x.DeliveredSpots);
        }
    }

}
