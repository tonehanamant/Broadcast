﻿using Services.Broadcast.Entities.Enums;
using System;

namespace Services.Broadcast.Entities.BarterInventory
{
    public class BarterInventoryHeader
    {
        public string DaypartCode { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Cpm { get; set; }
        public int AudienceId { get; set; }
        public int ContractedDaypartId { get; set; }
        public int ShareBookId { get; set; }
        public int HutBookId { get; set; }
        public ProposalEnums.ProposalPlaybackType PlaybackType { get; set; }
    }
}