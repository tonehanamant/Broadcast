namespace Services.Broadcast.Entities.SpotExceptions.Unposted
{
    public class SpotExceptionsUnpostedNoReelRosterDto
    {
        public int Id { get; set; }
        public string HouseIsci { get; set; }
        public int Count { get; set; }
        public System.DateTime ProgramAirTime { get; set; }
        public long? EstimateId { get; set; }
        public string IngestedBy { get; set; }
        public System.DateTime IngestedAt { get; set; }
        public int IngestedMediaWeekId { get; set; }
    }
}
