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
    
    public partial class postlog_file_detail_demographics
    {
        public int id { get; set; }
        public Nullable<int> audience_id { get; set; }
        public Nullable<long> postlog_file_detail_id { get; set; }
        public Nullable<double> overnight_rating { get; set; }
        public Nullable<double> overnight_impressions { get; set; }
    
        public virtual audience audience { get; set; }
        public virtual postlog_file_details postlog_file_details { get; set; }
    }
}
