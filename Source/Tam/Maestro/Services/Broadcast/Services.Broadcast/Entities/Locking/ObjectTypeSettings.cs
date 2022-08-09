using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Locking
{    
    public class ObjectTypeSettings
    {
        public LockExpirationTime broadcast_campaign { get; set; }
        public LockExpirationTime broadcast_proposal { get; set; }
        public LockExpirationTime broadcast_station { get; set; }
        public LockExpirationTime broadcast_plan { get; set; }
    }   
    public class LockExpirationTime
    {
        public string DefaultExpiration { get; set; }
        public string MaxExpiration { get; set; }
    }
}
