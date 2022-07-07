using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Locking
{
    public class LockingResultDto
    {
        /// <summary>
        /// Defines the locking key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// Returns True if api gives response otherwise false
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// Defines the locking time out 
        /// </summary>
        public int LockTimeoutInSeconds { get; set; }

        /// <summary>
        /// Defines the userid who locked the object
        /// </summary>
        public string LockedUserId { get; set; }

        /// <summary>
        /// Defines the username who locks the object
        /// </summary>
        public string LockedUserName { get; set; }

        /// <summary>
        /// Defines if any error occured in the api response
        /// </summary>
        public string Error { get; set; }
    }
}
