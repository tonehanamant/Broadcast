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
    
    public partial class plan_version_pricing_parameters
    {
        public plan_version_pricing_parameters()
        {
            this.plan_version_pricing_parameters_inventory_source_percentages = new HashSet<plan_version_pricing_parameters_inventory_source_percentages>();
        }
    
        public int id { get; set; }
        public int plan_version_id { get; set; }
        public Nullable<decimal> min_cpm { get; set; }
        public Nullable<decimal> max_cpm { get; set; }
        public double coverage_goal { get; set; }
        public double impressions_goal { get; set; }
        public decimal budget_goal { get; set; }
        public decimal cpm_goal { get; set; }
        public double proprietary_blend { get; set; }
        public Nullable<double> competition_factor { get; set; }
        public Nullable<double> inflation_factor { get; set; }
        public int unit_caps_type { get; set; }
        public int unit_caps { get; set; }
        public decimal cpp { get; set; }
        public int currency { get; set; }
        public double rating_points { get; set; }
    
        public virtual plan_versions plan_versions { get; set; }
        public virtual ICollection<plan_version_pricing_parameters_inventory_source_percentages> plan_version_pricing_parameters_inventory_source_percentages { get; set; }
    }
}
