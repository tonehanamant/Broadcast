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
    
    public partial class spot_exceptions_out_of_spec_comments
    {
        public int id { get; set; }
        public string spot_unique_hash_external { get; set; }
        public string execution_id_external { get; set; }
        public string isci_name { get; set; }
        public System.DateTime program_air_time { get; set; }
        public string station_legacy_call_letters { get; set; }
        public int reason_code_id { get; set; }
        public int recommended_plan_id { get; set; }
        public string comment { get; set; }
        public string added_by { get; set; }
        public System.DateTime added_at { get; set; }
    }
}
