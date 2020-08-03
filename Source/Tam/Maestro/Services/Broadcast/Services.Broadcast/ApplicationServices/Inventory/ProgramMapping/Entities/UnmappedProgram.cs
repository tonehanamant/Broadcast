using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.ApplicationServices.Inventory.ProgramMapping.Entities
{
    public class UnmappedProgram
    {
        public string ProgramName { get; set; }
        public string OriginalName { get; set; }
        public string MatchedName {get; set;}
        public string Genre { get; set; }
        public string ShowType { get; set; }

        public float MatchConfidence { get; set; }
    }
}
