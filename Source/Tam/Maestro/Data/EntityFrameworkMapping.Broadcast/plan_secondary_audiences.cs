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
    
    public partial class plan_secondary_audiences
    {
        public int id { get; set; }
        public int plan_id { get; set; }
        public int audience_type { get; set; }
        public int audience_id { get; set; }
        public double vpvh { get; set; }
        public double delivery_rating_points { get; set; }
        public decimal cpm { get; set; }
        public double cpp { get; set; }
        public double universe { get; set; }
        public double delivery_impressions { get; set; }
    
        public virtual audience audience { get; set; }
        public virtual plan plan { get; set; }
    }
}
