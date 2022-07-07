using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.Locking
{
    /// <summary>
    /// Represents a result from a unlock attempt
    /// </summary>
    public class ReleaseLockResponse
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
        /// Defines if any error occured in the api response
        /// </summary>
        public string Error { get; set; }
    }
}
