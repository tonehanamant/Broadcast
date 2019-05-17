using System;
using Tam.Maestro.Data.Entities;

namespace Services.Broadcast.Entities.StationInventory
{
    public class StationInventoryManifestWeek
    {
        public int Id { get; set; }

        public MediaWeek MediaWeek { get; set; }
        
        public int Spots { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
