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
    
    public partial class fluidity_categories
    {
        public fluidity_categories()
        {
            this.plan_versions = new HashSet<plan_versions>();
        }
    
        public int id { get; set; }
        public string code { get; set; }
        public string category { get; set; }
        public Nullable<int> parent_category_id { get; set; }
    
        public virtual ICollection<plan_versions> plan_versions { get; set; }
    }
}