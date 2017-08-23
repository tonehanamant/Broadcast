using System;

namespace Services.Broadcast.Exceptions
{
    public class IdenticalHutAndShareException : Exception
    {
        public IdenticalHutAndShareException(string message) : base(message) { }
    }
}