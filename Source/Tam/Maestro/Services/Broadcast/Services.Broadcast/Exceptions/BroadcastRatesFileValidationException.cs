
using System;

namespace Services.Broadcast.Exceptions
{
    public class BroadcastRatesFileValidationException : Exception
        {
        public BroadcastRatesFileValidationException()
        {
        }

        public BroadcastRatesFileValidationException(string message) : base(message)
        {
        }

        public BroadcastRatesFileValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
