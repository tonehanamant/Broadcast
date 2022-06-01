using System;

namespace Services.Broadcast.Exceptions
{
    public class IdenticalHutAndShareException : CadentException
    {
        public IdenticalHutAndShareException(string message) : base(message) { }
    }
}