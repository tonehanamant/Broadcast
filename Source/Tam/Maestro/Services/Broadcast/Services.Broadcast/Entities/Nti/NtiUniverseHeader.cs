using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Nti
{
    public class NtiUniverseHeader
    {
        public int Id { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public int WeekNumber { get; set; }

        public List<NtiUniverseDetail> NtiUniverseDetails { get; set; }

        public List<NtiUniverse> NtiUniverses { get; set; }
    }
}
