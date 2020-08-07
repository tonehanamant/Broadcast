using System;

namespace Services.Broadcast.Entities.QuoteReport
{
    public class QuoteReportData
    {
        public string ExportFileName { get; set; }

        private const string FILENAME_FORMAT = "PlanQuote_{0}_{1}.xlsx";
        private const string DATE_FORMAT_FILENAME = "MMddyyyy";
        private const string TIME_FORMAT_FILENAME = "hhmmss";

        public QuoteReportData(DateTime currentMoment)
        {
            ExportFileName = string.Format(
                FILENAME_FORMAT, 
                currentMoment.ToString(DATE_FORMAT_FILENAME), 
                currentMoment.ToString(TIME_FORMAT_FILENAME));
        }
    }
}
