using LaunchDarkly.Client;
using Tam.Maestro.Common.Systems.DataTransferObjects;

namespace Services.Broadcast.Entities
{
    public class BroadcastEmployeeDto : EmployeeDto
    {
        public BroadcastEmployeeDto()
        {
        }

        public BroadcastEmployeeDto(EmployeeDto employee)
        {
            Accountdomainsid = employee.Accountdomainsid;
            Datecreated = employee.Datecreated;
            Datelastlogin = employee.Datelastlogin;
            Email = employee.Email;
            Firstname = employee.Firstname;
            FullName = employee.FullName;
            Hitcount = employee.Hitcount;
            Id = employee.Id;
            InternalExtension = employee.InternalExtension;
            Lastname = employee.Lastname;
            Mi = employee.Mi;
            Phone = employee.Phone;
            Status = employee.Status;
            UniqueIdentifier = employee.UniqueIdentifier;
            Username = employee.Username;
        }

        /// <summary>
        /// The authenticated client hash for the Launch Darkly application.
        /// </summary>
        public string LaunchDarklyClientHash { get; set; }
    }
}