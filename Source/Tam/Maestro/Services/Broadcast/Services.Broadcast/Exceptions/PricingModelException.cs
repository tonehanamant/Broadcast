using System;

namespace Services.Broadcast.Exceptions
{
    public class PricingModelException : CadentException
    {
        public PricingModelException(string message) : base(message) { }
    }
}