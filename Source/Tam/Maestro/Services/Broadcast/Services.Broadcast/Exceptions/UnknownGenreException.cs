using System;

namespace Services.Broadcast.Exceptions
{
    public class UnknownGenreException : CadentException
    {
        public UnknownGenreException(string message) : base(message)
        {
        }
    }
}
