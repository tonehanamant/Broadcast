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
    
    public partial class inventory_detail_slot_component_proposal
    {
        public int inventory_detail_slot_component_id { get; set; }
        public int proprosal_version_detail_quarter_week_id { get; set; }
        public int inventory_detail_slot_id { get; set; }
        public int daypart_id { get; set; }
        public short station_code { get; set; }
        public System.DateTime start_date { get; set; }
        public System.DateTime end_date { get; set; }
        public string daypart_code { get; set; }
        public Nullable<byte> rate_source { get; set; }
    }
}
