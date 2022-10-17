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
    
    public partial class spot_exceptions_recommended_plans_done
    {
        public spot_exceptions_recommended_plans_done()
        {
            this.spot_exceptions_recommended_plan_details_done = new HashSet<spot_exceptions_recommended_plan_details_done>();
        }
    
        public int id { get; set; }
        public string spot_unique_hash_external { get; set; }
        public int ambiguity_code { get; set; }
        public string execution_id_external { get; set; }
        public int estimate_id { get; set; }
        public string inventory_source { get; set; }
        public string house_isci { get; set; }
        public string client_isci { get; set; }
        public Nullable<int> spot_length_id { get; set; }
        public System.DateTime program_air_time { get; set; }
        public string station_legacy_call_letters { get; set; }
        public string affiliate { get; set; }
        public Nullable<int> market_code { get; set; }
        public Nullable<int> market_rank { get; set; }
        public string program_name { get; set; }
        public string program_genre { get; set; }
        public string ingested_by { get; set; }
        public System.DateTime ingested_at { get; set; }
        public int ingested_media_week_id { get; set; }
    
        public virtual ICollection<spot_exceptions_recommended_plan_details_done> spot_exceptions_recommended_plan_details_done { get; set; }
        public virtual spot_lengths spot_lengths { get; set; }
    }
}