using System;
using Tam.Maestro.Data.Entities;

namespace Common.Systems.LockTokens
{
    [Serializable]
    public class StationToken : IBusinessEntity
    {
        private readonly int _Id;

        public StationToken(int id)
        {
            _Id = id;
        }

        public string UniqueIdentifier
        {
            get { return _Id.ToString(); }
        }
    }
}