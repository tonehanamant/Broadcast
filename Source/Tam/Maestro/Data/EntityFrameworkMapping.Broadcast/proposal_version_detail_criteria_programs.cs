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
    
    public partial class proposal_version_detail_criteria_programs
    {
        public int id { get; set; }
        public int proposal_version_detail_id { get; set; }
        public byte contain_type { get; set; }
        public string program_name { get; set; }
        public int program_name_id { get; set; }
    
        public virtual proposal_version_details proposal_version_details { get; set; }
    }
}
