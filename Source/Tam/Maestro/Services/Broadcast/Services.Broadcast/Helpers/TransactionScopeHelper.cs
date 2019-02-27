using System;
using System.Reflection;
using System.Transactions;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.Helpers
{
    public static class TransactionScopeHelper
    {
        public static TransactionScopeWrapper CreateTransactionScopeWrapper(TimeSpan timeout)
        {
            return new TransactionScopeWrapper(CreateTransactionScope(timeout));
        }

        public static TransactionScope CreateTransactionScope(TimeSpan timeout)
        {
            SetTransactionManagerField("_cachedMaxTimeout", true);
            SetTransactionManagerField("_maximumTimeout", timeout);

            return new TransactionScope(TransactionScopeOption.Required,
                   new TransactionOptions
                   {
                       IsolationLevel = IsolationLevel.ReadUncommitted,
                       Timeout = timeout
                   });
        }

        public static void SetTransactionManagerField(string fieldName, object value)
        {
            typeof(TransactionManager)
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static)
                .SetValue(null, value);
        }
    }
}
