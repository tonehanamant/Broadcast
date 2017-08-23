using System;
using Tam.Maestro.Data.Entities;

namespace Common.Systems.LockTokens
{
    [Serializable]
    public class ProposalToken : IBusinessEntity
    {
        private readonly int _Id;

        public ProposalToken(int id)
        {
            _Id = id;
        }

        public string UniqueIdentifier
        {
            get { return _Id.ToString(); }
        }
    }
}