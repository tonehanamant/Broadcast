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
    
    public partial class plan_summary_quarters
    {
        public int id { get; set; }
        public int plan_summary_id { get; set; }
        public int quarter { get; set; }
        public int year { get; set; }
    
        public virtual plan_summary plan_summary { get; set; }
    }
}
