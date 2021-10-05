using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Isci
{
    public class IsciPlanDetailDto
    {
        public int Id { get; set; }
        public Guid? AdvertiserMasterId { get; set; }
        public string AdvertiserName { get; set; }
        public List<int> SpotLengthValues { get; set; }
        public string AudienceCode { get; set; }
        public string Title { get; set; }
        public List<string> Dayparts { get; set; }
        public string ProductName { get; set; }
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public List<string> Iscis { get; set; }
    }
}
