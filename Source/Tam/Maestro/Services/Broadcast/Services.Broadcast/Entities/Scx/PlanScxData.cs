using System;

namespace Services.Broadcast.Entities.Scx
{
    public class PlanScxData : ScxData
    {
        public string PlanName { get; set; }
        public DateTime Generated { get; set; }
    }
}