using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Locking
{
    /// <summary>
    /// Represents a result from a lock attempt
    /// </summary>
    public class LockingResult
    {
        /// <summary>
        /// The type of object the lock is associated with
        /// </summary>
        public string objectType { get; set; }
        /// <summary>
        /// The unique object ID associated with the object type
        /// </summary>
        public string objectId { get; set; }
        /// <summary>
        /// Determines whether the lock is shared across applications
        /// </summary>
        public bool isShared { get; set; }
        /// <summary>
        /// Date the lock was created on (in UTC)
        /// </summary>
        public DateTime createdOn { get; set; }
        /// <summary>
        /// When the lock automatically expires (in UTC)
        /// </summary>
        public DateTime expiresAt { get; set; }
        /// <summary>
        /// Seconds remaining for the lock to be expired
        /// </summary>
        public int expiresIn { get; set; }
        /// <summary>
        /// Username who initiated the lock
        /// </summary>
        public string owner { get; set; }
        /// <summary>
        /// The first name of the owning user of the lock
        /// </summary>
        public string ownerFirstName { get; set; }
        /// <summary>
        /// The last name of the owning user of the lock
        /// </summary>
        public string ownerLastName { get; set; }
        /// <summary>
        /// Source application ID that initiated the lock
        /// </summary>
        public string applicationId { get; set; }
        /// <summary>
        /// Source application name that initiated the lock
        /// </summary>
        public string applicationName { get; set; }
        /// <summary>
        /// Represents a list of additional applications the lock is shared with
        /// </summary>
        public List<object> sharedApplications { get; set; }
    }
}
