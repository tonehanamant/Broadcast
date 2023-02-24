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
    
    public partial class staged_out_of_specs
    {
        public int id { get; set; }
        public string spot_unique_hash_external { get; set; }
        public string execution_id_external { get; set; }
        public Nullable<int> estimate_id { get; set; }
        public string inventory_source { get; set; }
        public string house_isci { get; set; }
        public string client_isci { get; set; }
        public int client_spot_length { get; set; }
        public System.DateTime broadcast_aired_date { get; set; }
        public int aired_time { get; set; }
        public string station_legacy_call_letters { get; set; }
        public string affiliate { get; set; }
        public Nullable<int> market_code { get; set; }
        public Nullable<int> market_rank { get; set; }
        public decimal rate { get; set; }
        public string audience_name { get; set; }
        public double impressions { get; set; }
        public string program_name { get; set; }
        public string program_genre { get; set; }
        public int reason_code { get; set; }
        public string reason_code_message { get; set; }
        public string lead_in_program_name { get; set; }
        public string lead_out_program_name { get; set; }
        public int plan_id { get; set; }
        public string daypart_code { get; set; }
        public Nullable<int> start_time { get; set; }
        public Nullable<int> end_time { get; set; }
        public Nullable<int> monday { get; set; }
        public Nullable<int> tuesday { get; set; }
        public Nullable<int> wednesday { get; set; }
        public Nullable<int> thursday { get; set; }
        public Nullable<int> friday { get; set; }
        public Nullable<int> saturday { get; set; }
        public Nullable<int> sunday { get; set; }
        public string ingested_by { get; set; }
        public System.DateTime ingested_at { get; set; }
    }
}
