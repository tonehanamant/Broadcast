using System;

namespace Services.Broadcast.Exceptions
{
    public class NoSelectedDaysException : Exception
    {
        public NoSelectedDaysException(string message) : base(message) { }
    }
}