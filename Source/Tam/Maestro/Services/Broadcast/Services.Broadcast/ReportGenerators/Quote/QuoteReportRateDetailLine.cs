using Services.Broadcast.Entities.QuoteReport;
using System.Collections.Generic;

namespace Services.Broadcast.ReportGenerators.Quote
{
    public class QuoteReportRateDetailLine
    {
        public string ProgramName { get; set; }
        public string DaypartName { get; set; }
        public string StationCallsign { get; set; }
        public string MarketName { get; set; }
        public string Affiliate { get; set; }
        public List<QuoteReportRateDetailLineAudience> PlanAudiences { get; set; } = new List<QuoteReportRateDetailLineAudience>();
        public int SpotsAllocated { get; set; }
        public decimal SpotCost { get; set; }
    }
}