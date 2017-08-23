using System;

namespace Services.Broadcast.Exceptions
{
    public class BroadcastProgramFlightException: Exception
    {
        public BroadcastProgramFlightException()
        {
        }

        public BroadcastProgramFlightException(string message) : base(message)
        {
        }

        public BroadcastProgramFlightException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
