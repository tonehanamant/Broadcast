namespace Services.Broadcast.Entities
{
    public class DisplayScheduleStation
    {
        public short StationCode { get; set; }
        public string CallLetters {get; set; }
        public string LegacyCallLetters {get; set;}
        public string Affiliation { get; set; }
    }
}
