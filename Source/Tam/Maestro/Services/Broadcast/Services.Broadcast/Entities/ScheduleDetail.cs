using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Entities
{
    public class ScheduleSpotTarget
    {
        public ScheduleDetail ScheduleDetail { get; set; }
        public ScheduleDetailWeek ScheduleDetailWeek { get; set; }

        public bool IsLeadInMatch { get; set; }

        public bool AllSpotsFilled()
        {
            return ScheduleDetailWeek.FilledSpots >= ScheduleDetailWeek.Spots;
        }
    }

    public class ScheduleDetail
    {
        public string Market { get; set; }
        public string Station { get; set; }
        public string Program { get; set; }
        public int DaypartId { get; set; }
        public string SpotLength { get; set; }
        public int? SpotLengthId { get; set; }
        public List<ScheduleDetailWeek> DetailWeeks { get; set; }

        public ScheduleDetail()
        {
            DetailWeeks = new List<ScheduleDetailWeek>();
        }

        public List<ScheduleSpotTarget> ToScheduleSpotTargets()
        {
            return DetailWeeks.Select(
                x => new ScheduleSpotTarget
                {
                    ScheduleDetailWeek = x,
                    ScheduleDetail = this,
                    IsLeadInMatch = false,
                }).ToList();
        }
    }

    public class ScheduleDetailWeek
    {
        public int ScheduleDetailWeekId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Spots { get; set; }
        public bool Matched { get; set; }
        public short FilledSpots { get; set; }
    }
}
