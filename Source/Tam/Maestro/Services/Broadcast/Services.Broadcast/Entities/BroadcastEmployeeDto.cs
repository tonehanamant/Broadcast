using Tam.Maestro.Common.Systems.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class BroadcastEmployeeDto : EmployeeDto
    {
        /// <summary>
        /// The authenticated client hash for the Launch Darkly application.
        /// </summary>
        public string LaunchDarklyClientHash { get; set; }
    }
}