using Services.Broadcast.Entities;
using System.IO;
using System.Linq;
using System.Text;

namespace Services.Broadcast.ReportGenerators
{
    public class MyEventsReportGenerator : IReportGenerator<MyEventsReportData>
    {
        private const string Delimiter = "\t";
        private const string DateFormat = "MM/dd/yyyy";
        private const string TimeFormat = "hh:mm:tt";
        private const string FileExtension = ".txt";
        private const string DoubleDelimiter = Delimiter + Delimiter;

        public ReportOutput Generate(MyEventsReportData myEventsReportData)
        {
            var memoryStream = new MemoryStream();
            var tempPath = Path.GetTempPath();
            var streamWriter = new StreamWriter(memoryStream);
            var firstMyEventsReportData = myEventsReportData.Lines.First();
            var fileName = firstMyEventsReportData.ReportableName + FileExtension;

            foreach (var data in myEventsReportData.Lines)
            {
                streamWriter.Write(_BuildReportLine(data));
            }

            streamWriter.Flush();

            return new ReportOutput(fileName) { Stream = memoryStream };
        }

        private string _BuildReportLine(MyEventsReportDataLine data)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(_BuildReportLineWithDelimiter(data.ReportableName));
            stringBuilder.Append(_BuildReportLineWithDelimiter(data.ScheduleStartDate.ToString(DateFormat)));
            stringBuilder.Append(_BuildReportLineWithDelimiter(data.ScheduleEndDate.ToString(DateFormat)));
            stringBuilder.Append(_BuildReportLineWithDelimiter(data.StationCallLetters));
            stringBuilder.Append(_BuildReportLineWithDoubleDelimiter(data.LineupPOT));
            stringBuilder.Append(_BuildReportLineWithDoubleDelimiter(data.LineupDOW));
            stringBuilder.Append(_BuildReportLineWithDoubleDelimiter(data.OffAir));
            stringBuilder.Append(_BuildReportLineWithDoubleDelimiter(data.LineupStartDate.ToString(DateFormat)));
            stringBuilder.Append(_BuildReportLineWithDoubleDelimiter(data.LineupNumberOfWeeks));
            stringBuilder.Append(_BuildReportLineWithDoubleDelimiter(data.LineupStartTime.ToString(TimeFormat)));
            stringBuilder.Append(_BuildReportLineWithDoubleDelimiter(data.Duration));
            stringBuilder.Append(_BuildReportLineWithDoubleDelimiter(data.MOP));
            stringBuilder.Append(_BuildReportLineWithDoubleDelimiter(data.CVT));
            stringBuilder.Append(_BuildReportLineWithDoubleDelimiter(data.LineupMultiBarter));
            stringBuilder.Append(_BuildReportLineWithDoubleDelimiter(data.LineupCashOnly));
            stringBuilder.AppendLine(_BuildReportLineWithDelimiter(data.LineupAiringNumber));

            return stringBuilder.ToString();
        }

        private string _BuildReportLineWithDelimiter(object data)
        {
            return data.ToString() + Delimiter;
        }

        private string _BuildReportLineWithDoubleDelimiter(object data)
        {
            return data.ToString() + DoubleDelimiter;
        }
    }
}
