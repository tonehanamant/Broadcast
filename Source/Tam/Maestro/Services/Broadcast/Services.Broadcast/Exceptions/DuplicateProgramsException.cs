using System;

namespace Services.Broadcast.Exceptions
{
    public class DuplicateProgramsException : CadentException
    {
        public DuplicateProgramsException(string message) : base(message) { }
    }
}