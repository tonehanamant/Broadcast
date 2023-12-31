﻿using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;

namespace Services.Broadcast.Entities.Vpvh
{

    public class VpvhFileItem
    {
        public DisplayAudience  Audience { get; set; }

        public int Quarter { get; set; }

        public int Year { get; set; }

        public double AMNews { get; set; }

        public double PMNews { get; set; }

        public double SynAll { get; set; }
    }
}
