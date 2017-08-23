using EntityFrameworkMapping.Broadcast;
using System.Collections.Generic;

namespace Services.Broadcast.Entities
{
    public class BvsFileDetailFilterResult
    {
        public List<bvs_file_details> Ignored = new List<bvs_file_details>();
        public List<bvs_file_details> Updated = new List<bvs_file_details>();
        public List<bvs_file_details> New = new List<bvs_file_details>();
    }
}