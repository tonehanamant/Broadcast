using System.Collections.Generic;
using Tam.Maestro.Data.Entities;

namespace Tam.Maestro.Services.Cable.DatabaseDtos
{
    public class AuthenticationEmployee
    {
        public Employee Employee = new Employee();
        public List<int> Roles =  new List<int>();
    }
}
