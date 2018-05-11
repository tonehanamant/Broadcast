using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class MyEventsReportData
    {
        public MyEventsReportData()
        {
            Lines = new List<MyEventsReportDataLine>();
        }

        public List<MyEventsReportDataLine> Lines { get; set; }
    }
}
