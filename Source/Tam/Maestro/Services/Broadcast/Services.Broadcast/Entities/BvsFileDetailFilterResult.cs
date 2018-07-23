using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class BvsFileDetailFilterResult
    {
        public List<BvsFileDetail> Ignored = new List<BvsFileDetail>();
        public List<BvsFileDetail> Updated = new List<BvsFileDetail>();
        public List<BvsFileDetail> New = new List<BvsFileDetail>();
    }
}