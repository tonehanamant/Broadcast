using System;

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

        public string RequestEntryId => $"R{RequestElementNumber.ToString().PadLeft(6, '0')}.M{ManifestId.ToString().PadLeft(3, '0')}.W{WeekNumber}.D{ManifestDaypartId}";
    }
}