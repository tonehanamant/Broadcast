namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsOutOfSpecDetailsResultDto
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        public string DaypartCode { get; set; }
        public string Network { get; set; }
        public string AudienceName { get; set; }
        public string FlightStartDate { get; set; }
        public string FlightEndDate { get; set; }
        public string FlightDateString { get; set; }
        public bool? AcceptedAsInSpec { get; set; }
        public string DecisionNotes { get; set; }
        public string ProgramName { get; set; }
        public string ProgramAirDate { get; set; }
        public string ProgramAirTime { get; set; }
    }
}
