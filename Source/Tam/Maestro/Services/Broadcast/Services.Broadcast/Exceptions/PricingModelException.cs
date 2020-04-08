using System;

namespace Services.Broadcast.Exceptions
{
    public class PricingModelException : Exception
    {
        public PricingModelException(string message) : base(message) { }
    }
}