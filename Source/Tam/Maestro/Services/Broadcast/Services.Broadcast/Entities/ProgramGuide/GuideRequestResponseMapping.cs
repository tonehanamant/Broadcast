using System;
using System.Text;

namespace Services.Broadcast.Entities.ProgramGuide
{
    public class GuideRequestResponseMapping
    {
        public int RequestElementNumber { get; set; }
        public int WeekNumber { get; set; }
        public int ManifestId { get; set; }
        public int ManifestDaypartId { get; set; }
        public DateTime WeekStartDte { get; set; }
        public DateTime WeekEndDate { get; set; }

        public string DaypartText { get; set; }
        public string StationCallLetters { get; set; }
        public string NetworkAffiliate { get; set; }

        public string RequestEntryId => $"R{RequestElementNumber.ToString().PadLeft(6, '0')}.M{ManifestId.ToString().PadLeft(3, '0')}.W{WeekNumber}.D{ManifestDaypartId}";

        public override string ToString()
        {
            const string dateFormat = "yyyy-MM-dd";
            var sb = new StringBuilder();

            sb.AppendLine($"RequestEntryId : {RequestEntryId}");
            sb.AppendLine($"RequestElementNumber : {RequestElementNumber}");
            sb.AppendLine($"WeekNumber : {WeekNumber}");
            sb.AppendLine($"ManifestId : {ManifestId}");
            sb.AppendLine($"ManifestDaypartId : {ManifestDaypartId}");
            sb.AppendLine();
            sb.AppendLine($"WeekStartDte : {WeekStartDte.ToString(dateFormat)}");
            sb.AppendLine($"WeekEndDate : {WeekEndDate.ToString(dateFormat)}");
            sb.AppendLine();
            sb.AppendLine($"DaypartText : {DaypartText}");
            sb.AppendLine($"StationCallLetters : {StationCallLetters}");
            sb.AppendLine($"NetworkAffiliate : {NetworkAffiliate}");

            return sb.ToString();
        }
    }
}
