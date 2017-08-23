
using System;

namespace Services.Broadcast.Exceptions
{
    public class HutGreaterThanShareException : Exception
    {
        public HutGreaterThanShareException(string message) : base(message) { }
    }
}