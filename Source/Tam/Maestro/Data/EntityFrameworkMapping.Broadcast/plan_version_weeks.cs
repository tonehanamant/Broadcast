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
    
    public partial class plan_version_weeks
    {
        public int id { get; set; }
        public int media_week_id { get; set; }
        public int number_active_days { get; set; }
        public string active_days_label { get; set; }
        public double weekly_impressions { get; set; }
        public double weekly_impressions_percentage { get; set; }
        public int plan_version_id { get; set; }
        public double weekly_rating_points { get; set; }
        public decimal weekly_budget { get; set; }
    
        public virtual media_weeks media_weeks { get; set; }
        public virtual plan_versions plan_versions { get; set; }
    }
}
