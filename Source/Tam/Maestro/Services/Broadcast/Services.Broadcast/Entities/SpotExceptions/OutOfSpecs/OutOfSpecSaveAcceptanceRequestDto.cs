using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    /// <summary>
    /// out of spec save request
    /// </summary>
    public class OutOfSpecSaveAcceptanceRequestDto
    {
        /// <summary>
        /// get and set the spotId's
        /// </summary>
        public List<int> SpotIds { get; set; }
        /// <summary>
        /// get and set the acceptance of spot
        /// </summary>
        public bool AcceptAsInSpec { get; set; }
        /// <summary>
        /// get and set the comment
        /// </summary>
        public string Comment { get; set; }
    }
}
