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
    
    public partial class plan_version_available_markets
    {
        public int id { get; set; }
        public short market_code { get; set; }
        public int market_coverage_File_id { get; set; }
        public int rank { get; set; }
        public double percentage_of_us { get; set; }
        public Nullable<double> share_of_voice_percent { get; set; }
        public int plan_version_id { get; set; }
        public bool is_user_share_of_voice_percent { get; set; }
    
        public virtual market_coverage_files market_coverage_files { get; set; }
        public virtual market market { get; set; }
        public virtual plan_versions plan_versions { get; set; }
    }
}
