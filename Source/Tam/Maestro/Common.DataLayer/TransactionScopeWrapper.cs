using System;
using System.Transactions;

namespace Tam.Maestro.Common.DataLayer
{
    public interface ITransactionScopeWrapper : IDisposable
    {
        void Complete();
    }

    public class TransactionScopeWrapper : ITransactionScopeWrapper
    {
        private readonly TransactionScope _TransactionScope;

        public TransactionScopeWrapper()
        {
            _TransactionScope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted,
                    Timeout = new System.TimeSpan(1, 0, 0)
                });
        }

        public TransactionScopeWrapper(System.Transactions.IsolationLevel pIsolationLevel)
        {
            _TransactionScope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = pIsolationLevel,
                    Timeout = new System.TimeSpan(1, 0, 0)
                });
        }

        public TransactionScopeWrapper(System.Transactions.TransactionScopeOption pOption, System.Transactions.IsolationLevel pIsolationLevel)
        {
            _TransactionScope = new TransactionScope(pOption,
                new TransactionOptions()
                {
                    IsolationLevel = pIsolationLevel,
                    Timeout = new System.TimeSpan(1, 0, 0)
                });
        }

        public TransactionScopeWrapper(TransactionScope transactionScope)
        {
            _TransactionScope = transactionScope;
        }

        public void Dispose()
        {
            _TransactionScope.Dispose();
        }

        public void Complete()
        {
            _TransactionScope.Complete();
        }
    }
}
