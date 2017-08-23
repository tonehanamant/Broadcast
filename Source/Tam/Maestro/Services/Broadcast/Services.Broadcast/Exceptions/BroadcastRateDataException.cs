using System;

namespace Services.Broadcast.Exceptions
{
    public class BroadcastRateDataException : Exception
    {
        private int _rateFileId = 0;

        public int RateFileId
        {
            get
            {
                return _rateFileId;
            }
        }

        public BroadcastRateDataException()
        {
            
        }

        public BroadcastRateDataException(int pRateFileId)
        {
            _rateFileId = pRateFileId;
        }

        public BroadcastRateDataException(string message)
            : base(message)
        {
        }

        public BroadcastRateDataException(string message, int pRateFileId) : base(message)
        {
            _rateFileId = pRateFileId;
        }

        public BroadcastRateDataException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public BroadcastRateDataException(string message, int pRateFileId, Exception inner)
            : base(message, inner)
        {
            _rateFileId = pRateFileId;
        }

    }
}
