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
    
    public partial class spot_exceptions_out_of_specs
    {
        public spot_exceptions_out_of_specs()
        {
            this.spot_exceptions_out_of_spec_decisions = new HashSet<spot_exceptions_out_of_spec_decisions>();
        }
    
        public int id { get; set; }
        public string spot_unique_hash_external { get; set; }
        public string execution_id_external { get; set; }
        public string reason_code_message { get; set; }
        public int estimate_id { get; set; }
        public string isci_name { get; set; }
        public Nullable<int> recommended_plan_id { get; set; }
        public string program_name { get; set; }
        public string station_legacy_call_letters { get; set; }
        public Nullable<int> spot_length_id { get; set; }
        public Nullable<int> audience_id { get; set; }
        public string program_network { get; set; }
        public System.DateTime program_air_time { get; set; }
        public int reason_code_id { get; set; }
        public double impressions { get; set; }
        public Nullable<int> market_code { get; set; }
        public Nullable<int> market_rank { get; set; }
        public Nullable<int> program_genre_id { get; set; }
        public string house_isci { get; set; }
        public string ingested_by { get; set; }
        public System.DateTime ingested_at { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_at { get; set; }
        public string modified_by { get; set; }
        public System.DateTime modified_at { get; set; }
        public string comment { get; set; }
        public string daypart_code { get; set; }
        public string genre_name { get; set; }
    
        public virtual audience audience { get; set; }
        public virtual genre genre { get; set; }
        public virtual plan plan { get; set; }
        public virtual ICollection<spot_exceptions_out_of_spec_decisions> spot_exceptions_out_of_spec_decisions { get; set; }
        public virtual spot_exceptions_out_of_spec_reason_codes spot_exceptions_out_of_spec_reason_codes { get; set; }
        public virtual spot_lengths spot_lengths { get; set; }
    }
}
