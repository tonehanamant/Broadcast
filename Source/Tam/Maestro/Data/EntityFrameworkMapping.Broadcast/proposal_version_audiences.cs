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
    
    public partial class proposal_version_audiences
    {
        public int id { get; set; }
        public int proposal_version_id { get; set; }
        public int audience_id { get; set; }
        public byte rank { get; set; }
    
        public virtual audience audience { get; set; }
        public virtual proposal_versions proposal_versions { get; set; }
    }
}
