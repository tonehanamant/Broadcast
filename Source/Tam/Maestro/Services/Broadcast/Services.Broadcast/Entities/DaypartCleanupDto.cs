using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class DaypartCleanupDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public List<int> Days { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
    }
}
