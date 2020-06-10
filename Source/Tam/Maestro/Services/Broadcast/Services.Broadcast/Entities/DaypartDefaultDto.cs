﻿using Services.Broadcast.Entities.Enums;

namespace Services.Broadcast.Entities
{
    public class DaypartDefaultDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public VpvhCalculationSourceTypeEnum VpvhCalculationSourceType { get; set; }
    }
}
