using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Exceptions
{
    public class BroadcastAffidavitException : Exception
    {
        public List<string> ErrorList { get; set; } 

        public BroadcastAffidavitException()
        {
            ErrorList = new List<string>();
        }

        public BroadcastAffidavitException(List<string> errorList )
        {
            ErrorList = errorList;
        }

        public BroadcastAffidavitException(string message) : base(message)
        {
            ErrorList = new List<string>();
        }

        public BroadcastAffidavitException(string message, List<string> errorList )
            : base(message)
        {
            ErrorList = errorList;
        }

        public BroadcastAffidavitException(string message, Exception inner)
            : base(message, inner)
        {
            ErrorList = new List<string>();
        }

        public BroadcastAffidavitException(string message, Exception inner, List<string> errorList )
            : base(message, inner)
        {
            ErrorList = errorList;
        }
    }
}
