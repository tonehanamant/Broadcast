﻿using System;

namespace Services.Broadcast.Entities
{
    public class InventoryCardManifestDto
    {
        public int StationId { get; set; }
        public short? MarketCode { get; set; }
        public string DaypartCode { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? FileId { get; set; }
        public int ManifestId { get; set; }
    }
}