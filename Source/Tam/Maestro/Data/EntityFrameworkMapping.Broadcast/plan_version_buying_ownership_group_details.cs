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
    
    public partial class plan_version_buying_ownership_group_details
    {
        public int id { get; set; }
        public int plan_version_buying_result_id { get; set; }
        public string ownership_group_name { get; set; }
        public int markets { get; set; }
        public int stations { get; set; }
        public int spots { get; set; }
        public double impressions { get; set; }
        public decimal cpm { get; set; }
        public decimal budget { get; set; }
        public double impressions_percentage { get; set; }
    
        public virtual plan_version_buying_results plan_version_buying_results { get; set; }
    }
}