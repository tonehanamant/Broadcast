using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class PostedContract
    {
        public int ContractId { get; set; }
        public string ContractName { get; set; }
        public DateTime? UploadDate { get; set; }
        public int SpotsInSpec { get; set; }
        public int SpotsOutOfSpec { get; set; }
        public double PrimaryAudienceDeliveredImpressions { get; set; }
        public int GuaranteedAudienceId { get; set; }
        public double PrimaryAudienceBookedImpressions { get; set; }
        public double PrimaryAudienceDelivery { get; set; }
        public double HouseholdDeliveredImpressions { get; set; }
        public int AdvertiserId { get; set; }
        public string Advertiser { get; set; }
        public SchedulePostType PostType { get; set; }
        public bool Equivalized { get; set; }
        public bool IsActiveThisWeek { get; set; }
        public DateTime? LastBuyDate { get; set; }
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public List<ProposalFlightWeek> FlightWeeks { get; set; }
    }
}
