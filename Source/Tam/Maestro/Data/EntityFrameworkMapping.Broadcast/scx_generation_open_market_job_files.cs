//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntityFrameworkMapping.Broadcast
{
    using System;
    using System.Collections.Generic;
    
    public partial class scx_generation_open_market_job_files
    {
        public int id { get; set; }
        public int scx_generation_open_market_job_id { get; set; }
        public string file_name { get; set; }
        public string standard_daypart_id { get; set; }
        public System.DateTime start_date { get; set; }
        public System.DateTime end_date { get; set; }
        public int export_genre_type_id { get; set; }
        public string affiliate { get; set; }
        public Nullable<System.Guid> shared_folder_files_id { get; set; }
        public string rank { get; set; }
    
        public virtual scx_generation_open_market_jobs scx_generation_open_market_jobs { get; set; }
    }
}
