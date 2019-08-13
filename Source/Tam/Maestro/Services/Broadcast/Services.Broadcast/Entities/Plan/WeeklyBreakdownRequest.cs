﻿using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.Plan
{
    public class WeeklyBreakdownRequest
    {
        public DateTime FlightStartDate { get; set; }
        public DateTime FlightEndDate { get; set; }
        public List<DateTime> FlightHiatusDays { get; set; }
        public PlanGloalBreakdownTypeEnum DeliveryType { get; set; }
        public double TotalImpressions { get; set; }        
        public List<WeeklyBreakdownWeek> Weeks { get; set; } = new List<WeeklyBreakdownWeek>();
    }
}