using System;

namespace Services.Broadcast.Exceptions
{
    public class BroadcastDuplicateRatesFileException : Exception
    {
        public BroadcastDuplicateRatesFileException()
        {
        }

        public BroadcastDuplicateRatesFileException(string message) : base(message)
        {
        }

        public BroadcastDuplicateRatesFileException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
