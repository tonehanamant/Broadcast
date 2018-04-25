using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Services.Broadcast.ReportGenerators
{
    public class MyEventsReportGenerator : IReportGenerator<List<MyEventsReportData>>
    {
        private const string Delimiter = "\t";
        private const string DateFormat = "MM/dd/yyyy";
        private const string TimeFormat = "hh:mm:tt";
        private const string FileExtension = ".txt";
        private const string DoubleDelimiter = Delimiter + Delimiter;

        public ReportOutput Generate(List<MyEventsReportData> myEventsReportData)
        {
            var myEventsReportMemoryStream = _GenerateMyEventsTextReport(myEventsReportData, out string fileName);
            return new ReportOutput(fileName) { Stream = myEventsReportMemoryStream };
        }

        private MemoryStream _GenerateMyEventsTextReport(List<MyEventsReportData> myEventsReportData, out string fileName)
        {
            if (!myEventsReportData.Any())
                throw new Exception("No data found for MyEvents report");

            var memoryStream = new MemoryStream();
            var tempPath = Path.GetTempPath();
            var firstMyEventsReportData = myEventsReportData.First();
            fileName = firstMyEventsReportData.ReportableName + FileExtension;
            var filePath = Path.Combine(tempPath, fileName);

            using (var streamWriter = new StreamWriter(filePath))
            {
                foreach (var data in myEventsReportData)
                {
                    streamWriter.Write(_BuildReportLine(data));
                }
            }

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(memoryStream);
            }

            return memoryStream;
        }

        private string _BuildReportLine(MyEventsReportData data)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(_BuildReportLineWithDelimiter(data.ReportableName));
            stringBuilder.Append(_BuildReportLineWithDelimiter(data.ScheduleStartDate.ToString(DateFormat)));
            stringBuilder.Append(_BuildReportLineWithDelimiter(data.ScheduleEndDate.ToString(DateFormat)));
            stringBuilder.Append(_BuildReportLineWithDelimiter(data.CallLetter));
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
