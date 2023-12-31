﻿
using System;

namespace Services.Broadcast.Exceptions
{
    public class BroadcastInventoryFileValidationException : CadentException
    {
        public BroadcastInventoryFileValidationException()
        {
        }

        public BroadcastInventoryFileValidationException(string message) : base(message)
        {
        }

        public BroadcastInventoryFileValidationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
