using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class MyEventsReportData
    {
        public List<MyEventsReportDataLine> Lines { get; set; } = new List<MyEventsReportDataLine>();
    }
}
