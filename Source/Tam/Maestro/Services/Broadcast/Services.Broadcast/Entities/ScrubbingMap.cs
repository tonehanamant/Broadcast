using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class ScrubbingMap
    {
        public int EstimateId { get; set; }
        public string BvsProgram { get; set; }
        public string ScheduleProgram { get; set; }
        public string BvsStation { get; set; }
        public string ScheduleStation { get; set; }
        public List<int> DetailIds { get; set; }

        public ScrubbingMap()
        {
            DetailIds = new List<int>();
        }
    }
}
