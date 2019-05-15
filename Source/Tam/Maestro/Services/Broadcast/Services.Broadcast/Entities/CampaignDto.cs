using System;

namespace Services.Broadcast.Entities
{
    public class CampaignDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AdvertiserId { get; set; }
        public int AgencyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? Budget { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
    }
}