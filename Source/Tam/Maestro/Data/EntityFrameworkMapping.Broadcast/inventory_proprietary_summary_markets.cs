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
    
    public partial class inventory_proprietary_summary_markets
    {
        public int id { get; set; }
        public int inventory_proprietary_summary_id { get; set; }
        public short market_code { get; set; }
        public Nullable<double> market_coverage { get; set; }
        public string created_by { get; set; }
        public System.DateTime created_at { get; set; }
        public string modified_by { get; set; }
        public Nullable<System.DateTime> modified_at { get; set; }
    
        public virtual inventory_proprietary_summary inventory_proprietary_summary { get; set; }
        public virtual market market { get; set; }
    }
}
