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
    
    public partial class plan_version_buying_market_details
    {
        public int id { get; set; }
        public int plan_version_buying_market_id { get; set; }
        public int rank { get; set; }
        public double market_coverage_percent { get; set; }
        public int stations { get; set; }
        public int spots { get; set; }
        public double impressions { get; set; }
        public double cpm { get; set; }
        public double budget { get; set; }
        public double impressions_percentage { get; set; }
        public Nullable<double> share_of_voice_goal_percentage { get; set; }
    
        public virtual plan_version_buying_markets plan_version_buying_markets { get; set; }
    }
}
