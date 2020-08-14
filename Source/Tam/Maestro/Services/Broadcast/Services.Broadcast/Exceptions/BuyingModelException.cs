using System;

namespace Services.Broadcast.Exceptions
{
    public class BuyingModelException : Exception
    {
        public BuyingModelException(string message) : base(message) { }
    }
}