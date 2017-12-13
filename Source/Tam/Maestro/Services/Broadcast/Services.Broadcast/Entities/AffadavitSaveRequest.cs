using System;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    [Serializable]
    public class AffadavitSaveRequest
    {
        public string FileName;
        public string FileHash;
        public int Source;
        public List<AffadavitSaveRequestDetail> Details;

        public AffadavitSaveRequest()
        {
            Details = new List<AffadavitSaveRequestDetail>();
        }
    }

    public class AffadavitSaveRequestDetail
    {
        public DateTime AirTime;
        public string ProgramName;
        public int SpotLength;
        public string Isci;
    }
}
