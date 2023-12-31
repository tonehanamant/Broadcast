﻿using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.InventorySummary
{
    public class InventoryUploadHistoryDto
    {
        public int FileId { get; set; }
        public DateTime UploadDateTime { get; set; }
        public string Username { get; set; }
        public string Filename { get; set; }
        public List<string> DaypartCodes { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? EndDate { get; set; }
        public MediaMonthDto HutBook { get; set; }
        public MediaMonthDto ShareBook { get; set; }
        public int Rows { get; set; }
        public string Status { get; set; }
        public List<QuarterDetailDto> Quarters { get; set; }
    }
}
