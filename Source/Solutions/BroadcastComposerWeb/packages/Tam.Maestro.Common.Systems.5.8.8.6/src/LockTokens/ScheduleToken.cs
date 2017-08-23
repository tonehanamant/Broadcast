using System;
using Tam.Maestro.Data.Entities;

namespace Common.Systems.LockTokens
{
    [Serializable]
    public class ScheduleToken : IBusinessEntity
    {
        private readonly int _Id;

        public ScheduleToken(int id)
        {
            _Id = id;
        }

        public string UniqueIdentifier
        {
            get { return _Id.ToString(); }
        }
    }
}