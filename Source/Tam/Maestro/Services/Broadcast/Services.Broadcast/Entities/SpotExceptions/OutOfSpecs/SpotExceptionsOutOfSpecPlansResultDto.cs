using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    public class SpotExceptionsOutOfSpecPlansResultDto
    {
        public SpotExceptionsOutOfSpecPlansResultDto()
        {
            Active = new List<SpotExceptionsOutOfSpecToDoPlansDto>();
            Completed = new List<SpotExceptionsOutOfSpecDonePlansDto>();
        }
        public List<SpotExceptionsOutOfSpecToDoPlansDto> Active { get; set; }
        public List<SpotExceptionsOutOfSpecDonePlansDto> Completed { get; set; }
    }

    public class SpotExceptionsOutOfSpecToDoPlansDto
    {
        public int PlanId { get; set; }
        public string AdvertiserName { get; set; }
        public string PlanName { get; set; }
        public int AffectedSpotsCount { get; set; }
        public double Impressions { get; set; }
        public string SyncedTimestamp { get; set; }
        public string SpotLengthString { get; set; }
        public string AudienceName { get; set; }
        public string FlightString { get; set; }
    }

    public class SpotExceptionsOutOfSpecDonePlansDto
    {
        public int PlanId { get; set; }
        public string AdvertiserName { get; set; }
        public string PlanName { get; set; }
        public int AffectedSpotsCount { get; set; }
        public double Impressions { get; set; }
        public string SyncedTimestamp { get; set; }
        public string SpotLengthString { get; set; }
        public string AudienceName { get; set; }
        public string FlightString { get; set; }
    }
}