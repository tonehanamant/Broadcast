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
    
    public partial class plan_version_buying_result_spot_stations
    {
        public int id { get; set; }
        public int plan_version_buying_result_id { get; set; }
        public string program_name { get; set; }
        public string genre { get; set; }
        public string station { get; set; }
        public double impressions { get; set; }
        public int spots { get; set; }
        public decimal budget { get; set; }
    
        public virtual plan_version_buying_results plan_version_buying_results { get; set; }
    }
}