using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class TrackingMapValue
    {
        public string BvsValue { get; set; }
        public string ScheduleValue { get; set; }
    }

    public class BvsMap
    {
        public List<TrackingMapValue> TrackingMapValues { get; set; }
        public int Id { get; set; }
        public int Version { get; set; }
        public TrackingMapType TrackingMapType { get; set; }

    }

    public enum TrackingMapType
    {
        Station = 1,
        Program = 2
    }
}
