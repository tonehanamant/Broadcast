using System;

namespace Services.Broadcast.Exceptions
{
    public class BroadcastDuplicateInventoryFileException : Exception
    {
        public BroadcastDuplicateInventoryFileException()
        {
        }

        public BroadcastDuplicateInventoryFileException(string message) : base(message)
        {
        }

        public BroadcastDuplicateInventoryFileException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
