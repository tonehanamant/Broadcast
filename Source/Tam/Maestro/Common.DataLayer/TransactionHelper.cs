using System;
using System.Data.Entity;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;
using IsolationLevel = System.Data.IsolationLevel;

namespace Tam.Maestro.Data.EntityFrameworkMapping
{
    public interface ITransactionHelper
    {
        ITransactionScopeWrapper BeginTransaction(DbContext pMaestroContext, IsolationLevel pIsolationLevel);
    }

    public class TransactionHelper : ITransactionHelper
    {
        public ITransactionScopeWrapper BeginTransaction(DbContext pMaestroContext, IsolationLevel pIsolationLevel)
        {
            var isolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            switch (pIsolationLevel)
            {
                case IsolationLevel.ReadCommitted:
                    isolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
                    break;
                case IsolationLevel.ReadUncommitted:
                    isolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                    break;
                default:
                    throw new Exception("Unsupported isolation level: " + pIsolationLevel);
            }

            var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = isolationLevel,
                    Timeout = new System.TimeSpan(1, 0, 0)
                });
            return new TransactionScopeWrapper(transactionScope);
        }
    }
}
