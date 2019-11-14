using System;

namespace Services.Broadcast.Exceptions
{
    public class UnknownGenreException : Exception
    {
        public UnknownGenreException(string message) : base(message)
        {
        }
    }
}
