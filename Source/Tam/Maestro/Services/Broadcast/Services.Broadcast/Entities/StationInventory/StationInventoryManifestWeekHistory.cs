using System;

namespace Services.Broadcast.Entities.StationInventory
{
    public class StationInventoryManifestWeekHistory
    {
        public int Id { get; set; }

        public int MediaWeekId { get; set; }

        public int Spots { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime SysStartDate { get; set; }

        public DateTime SysEndDate { get; set; }
    }
}
