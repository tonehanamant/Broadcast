using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    public class ProgramSearchRequest
    {
        public string Name { get; set; }
        public string Genre { get; set; }
        public int Start { get; set; }
        public int Limit { get; set; }
    }
}
