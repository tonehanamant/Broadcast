using System;

namespace Services.Broadcast.Exceptions
{
    public class DuplicateProgramsException : Exception
    {
        public DuplicateProgramsException(string message) : base(message) { }
    }
}