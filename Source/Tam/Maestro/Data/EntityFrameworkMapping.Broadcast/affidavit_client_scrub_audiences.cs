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
    
    public partial class affidavit_client_scrub_audiences
    {
        public int affidavit_client_scrub_id { get; set; }
        public int audience_id { get; set; }
        public double impressions { get; set; }
    
        public virtual affidavit_client_scrubs affidavit_client_scrubs { get; set; }
        public virtual audience audience { get; set; }
    }
}
