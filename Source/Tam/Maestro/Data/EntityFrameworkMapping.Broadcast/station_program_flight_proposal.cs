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
    
    public partial class station_program_flight_proposal
    {
        public int station_program_flight_id { get; set; }
        public int proprosal_version_detail_quarter_week_id { get; set; }
        public int spots { get; set; }
        public string created_by { get; set; }
        public string isci { get; set; }
        public double impressions { get; set; }
        public decimal spot_cost { get; set; }
        public int spot_length_id { get; set; }
        public short station_code { get; set; }
        public System.DateTime start_date { get; set; }
        public System.DateTime end_date { get; set; }
        public string daypart_code { get; set; }
        public Nullable<byte> rate_source { get; set; }
    
        public virtual proposal_version_detail_quarter_weeks proposal_version_detail_quarter_weeks { get; set; }
    }
}
