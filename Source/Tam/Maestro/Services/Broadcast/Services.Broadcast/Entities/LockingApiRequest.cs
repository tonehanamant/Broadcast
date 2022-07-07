using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    /// <summary>
    /// This class defines the request for the locking service
    /// </summary>
    public class LockingApiRequest
    {
        /// <summary>
        /// The type of object to base the lock on
        /// </summary>
        public string ObjectType { get; set; }

        /// <summary>
        /// The unique object ID associated with the object type
        /// </summary>
        public string ObjectId { get; set; }

        /// <summary>
        /// When the lock should automatically expire
        /// </summary>
        public TimeSpan ExpirationTimeSpan { get; set; }

        /// <summary>
        /// Determines whether the lock is shared across applications or not
        /// </summary>
        public bool IsShared { get; set; }

        /// <summary>
        /// Represents a list of additional applications the lock is shared with
        /// </summary>
        public IEnumerable<LockApplication> SharedApplications { get; set; }
        /// <summary>
        /// Defines the object type and objectid on which lock applies
        /// </summary>
        /// <returns>Returns key that contains on=bject type and objectid</returns>
        public string ToShortString()
            => $"Object Type: {ObjectType}; Object ID: {ObjectId}";
    }
    /// <summary>
    /// Defines the shared application properties 
    /// </summary>
    public class LockApplication
    {
        /// <summary>
        /// Defines the id of shared application
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Defines the name of shared application
        /// </summary>
        public string Name { get; set; }
    }
}
