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
    
    public partial class affiliate
    {
        public affiliate()
        {
            this.plan_version_daypart_affiliate_restrictions = new HashSet<plan_version_daypart_affiliate_restrictions>();
            this.plan_version_custom_daypart_affiliate_restrictions = new HashSet<plan_version_custom_daypart_affiliate_restrictions>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_date { get; set; }
        public string modified_by { get; set; }
        public System.DateTime modified_date { get; set; }
    
        public virtual ICollection<plan_version_daypart_affiliate_restrictions> plan_version_daypart_affiliate_restrictions { get; set; }
        public virtual ICollection<plan_version_custom_daypart_affiliate_restrictions> plan_version_custom_daypart_affiliate_restrictions { get; set; }
    }
}
