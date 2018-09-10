using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class DeliveredSpotsValueForStationsForWeek
    {
        public int MediaWeekId { get; set; }

        public IEnumerable<DeliveredSpotsValueForStation> DeliveredSpotsValueForStations { get; set; }

        public class DeliveredSpotsValueForStation
        {
            public string Station { get; set; }

            public int Spots { get; set; }
        }
    }
}
