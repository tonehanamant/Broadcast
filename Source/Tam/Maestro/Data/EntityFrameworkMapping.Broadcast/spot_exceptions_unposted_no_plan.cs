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
    
    public partial class spot_exceptions_unposted_no_plan
    {
        public int id { get; set; }
        public string house_isci { get; set; }
        public string client_isci { get; set; }
        public Nullable<int> client_spot_length_id { get; set; }
        public int count { get; set; }
        public System.DateTime program_air_time { get; set; }
        public long estimate_id { get; set; }
        public string ingested_by { get; set; }
        public System.DateTime ingested_at { get; set; }
        public int ingested_media_week_id { get; set; }
    }
}
