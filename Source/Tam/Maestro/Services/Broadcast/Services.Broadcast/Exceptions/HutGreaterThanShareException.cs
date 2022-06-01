
using System;

namespace Services.Broadcast.Exceptions
{
    public class HutGreaterThanShareException : CadentException
    {
        public HutGreaterThanShareException(string message) : base(message) { }
    }
}