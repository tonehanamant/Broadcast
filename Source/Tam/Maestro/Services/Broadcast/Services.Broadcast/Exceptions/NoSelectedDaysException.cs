using System;

namespace Services.Broadcast.Exceptions
{
    public class NoSelectedDaysException : CadentException
    {
        public NoSelectedDaysException(string message) : base(message) { }
    }
}