using System;

namespace Services.Broadcast.Exceptions
{
    public class BroadcastInventoryDataException : CadentException
    {
        private int _inventoryFileId = 0;

        public int InventoryFileId
        {
            get
            {
                return _inventoryFileId;
            }
        }

        public BroadcastInventoryDataException()
        {
            
        }

        public BroadcastInventoryDataException(int pInventoryFileId)
        {
            _inventoryFileId = pInventoryFileId;
        }

        public BroadcastInventoryDataException(string message)
            : base(message)
        {
        }

        public BroadcastInventoryDataException(string message, int pInventoryFileId) : base(message)
        {
            _inventoryFileId = pInventoryFileId;
        }

        public BroadcastInventoryDataException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public BroadcastInventoryDataException(string message, int pInventoryFileId, Exception inner)
            : base(message, inner)
        {
            _inventoryFileId = pInventoryFileId;
        }

    }
}
