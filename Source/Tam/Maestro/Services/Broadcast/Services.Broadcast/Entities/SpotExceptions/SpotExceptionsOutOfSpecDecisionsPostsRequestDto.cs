using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions
{
    public class SpotExceptionsOutOfSpecDecisionsPostsRequestDto
    {
        public int Id { get; set; }
        public bool AcceptAsInSpec { get; set; }
        public string DecisionNotes { get; set; }
    }
}
