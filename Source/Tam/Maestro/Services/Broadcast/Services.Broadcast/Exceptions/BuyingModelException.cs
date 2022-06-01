using System;

namespace Services.Broadcast.Exceptions
{
    public class BuyingModelException : CadentException
    {
        public BuyingModelException(string message) : base(message) { }
    }
}