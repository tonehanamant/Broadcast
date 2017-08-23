using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class TrafficDisplayDto
    {
        public TrafficDisplayDto()
        {
            Weeks = new List<TrafficWeek>();
        }
        public List<TrafficWeek> Weeks { get; set; }
    }

    public class TrafficWeek
    {
        public TrafficWeek()
        {
            TrafficProposalInventories = new List<TrafficProposalInventory>();
        }
        public int WeekId { get; set; }
        public string Week { get; set; }
        public List<TrafficProposalInventory> TrafficProposalInventories { get; set; } 
    }

    public class TrafficProposalInventory
    {
        public System.DateTime? StartDate;
        public System.DateTime? EndDate;
        public int Id { get; set; }
        public string Name { get; set; }
        internal int AdvertiserId { get; set; }
        public string Advertiser { get; set; }
        public int OpenMarketUnassignedISCI { get; set; }
        public int ProprietaryUnassignedISCI { get; set; }
    }
}
