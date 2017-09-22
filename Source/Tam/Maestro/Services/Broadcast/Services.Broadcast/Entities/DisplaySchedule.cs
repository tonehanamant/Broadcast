﻿using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class DisplaySchedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AdvertiserId { get; set; }
        public string Advertiser { get; set; }
        public int? Estimate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? SpotsBooked { get; set; }
        public int SpotsDelivered { get; set; }
        public int OutOfSpec { get; set; }
        public int PostingBookId { get; set; }
        public string PostingBook { get; set; }
        public DateTime? PostingBookDate { get; set; }
        public double? PrimaryDemoBooked { get; set; }
        public double PrimaryDemoDelivered { get; set; }
        public SchedulePostType PostType { get; set; }
        public RatesFile.RateSourceType InventorySource { get; set; }
        public bool IsEquivalized { get; set; }
        public List<short> MarketRestrictions { get; set; }
        public int DaypartRestrictionId { get; set; }
        public DaypartDto DaypartRestriction { get; set; }
        public List<int> Audiences { get; set; }
        public List<IsciDto> Iscis { get; set; }

        public bool IsBlank
        {
            get { return Estimate == null; }
        }
    }
}
