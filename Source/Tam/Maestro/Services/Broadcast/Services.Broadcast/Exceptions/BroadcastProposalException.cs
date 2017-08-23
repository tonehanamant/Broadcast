using System;

namespace Services.Broadcast.Exceptions
{
    public class BroadcastProposalException : Exception
    {
        public BroadcastProposalException()
        {
        }

        public BroadcastProposalException(string message)
            : base(message)
        {
        }

        public BroadcastProposalException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}
