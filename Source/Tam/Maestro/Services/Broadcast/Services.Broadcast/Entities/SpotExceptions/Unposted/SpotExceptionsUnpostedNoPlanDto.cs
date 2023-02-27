namespace Services.Broadcast.Entities.SpotExceptions.Unposted
{
    public class SpotExceptionsUnpostedNoPlanDto
    {
        public int Id { get; set; }
        public string HouseIsci { get; set; }
        public string ClientIsci { get; set; }
        public int? ClientSpotLengthId { get; set; }
        public int Count { get; set; }
        public System.DateTime ProgramAirTime { get; set; }
        public int? EstimateID { get; set; }
        public string IngestedBy { get; set; }
        public System.DateTime IngestedAt { get; set; }
        public int IngestMediaWeekId { get; set; }
    }
}
