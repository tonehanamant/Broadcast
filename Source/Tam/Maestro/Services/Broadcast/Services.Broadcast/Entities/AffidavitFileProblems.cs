using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class AffidavitFileProblem
    {
        public long Id { get; set; }
        public int AffidavitFileId { get; set; }
        public string ProblemDescription { get; set; }

    }
}
