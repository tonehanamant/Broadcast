using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Common.Services.Repositories
{
    public class RepositoryOptions
    {
        public static bool CodeExecutingForIntegegrationTest = false;
    }

    public class CoreRepositoryBase<CT> : IRepositoryBase where CT : DbContext
    {
        private static readonly int _Timeout;

        static CoreRepositoryBase()
        {
            var lCommandTimeoutFromConfig = ConfigurationManager.AppSettings["EntityFrameworkCommandTimeoutInSeconds"];
            if (lCommandTimeoutFromConfig != null)
            {
                _Timeout = Int32.Parse(lCommandTimeoutFromConfig);
            }
            else
            {
                _Timeout = 300;
            }
        }

        private readonly ISMSClient _SmsClient;
        private readonly IContextFactory<CT> _ContextFactory;
        private readonly ITransactionHelper _TransactionHelper;
        private readonly string _ConnectionStringType;

        public CoreRepositoryBase(ISMSClient pSmsClient, IContextFactory<CT> pContextFactory, ITransactionHelper pTransactionHelper, string connectionStringType)
        {
            _SmsClient = pSmsClient;
            _ContextFactory = pContextFactory;
            _TransactionHelper = pTransactionHelper;
            _ConnectionStringType = connectionStringType;
        }

        public void WarmupEntityFramework()
        {
            if (RepositoryOptions.CodeExecutingForIntegegrationTest)
            {
                return;
            }

            _InReadUncommitedTransaction(
                (database, transaction) =>
                {
                });
        }

        protected T _InReadUncommitedTransaction<T>(Func<CT, T> pFunc)
        {
            using (var context = CreateDBContext(false))
            using (var trx = _TransactionHelper.BeginTransaction(context, System.Data.IsolationLevel.ReadUncommitted))
            {
                var trx2 = context.Database.BeginTransaction(IsolationLevel.ReadUncommitted);

                context.Database.CommandTimeout = _Timeout;
                var ret = pFunc(context);

                trx2.Commit();
                trx.Complete();
                return ret;
            }
        }

        protected void _InReadUncommitedTransaction(Action<CT> pFunc)
        {
            using (var context = CreateDBContext(false))
            using (var trx = _TransactionHelper.BeginTransaction(context, System.Data.IsolationLevel.ReadUncommitted))
            {
                var trx2 = context.Database.BeginTransaction(IsolationLevel.ReadUncommitted);

                context.Database.CommandTimeout = _Timeout;
                pFunc(context);

                trx2.Commit();
                trx.Complete();
            }
        }

        protected void _InReadUncommitedTransaction(Action<CT, ITransactionScopeWrapper> pFunc)
        {
            using (var context = CreateDBContext(false))
            using (var lTransaction = _TransactionHelper.BeginTransaction(context, System.Data.IsolationLevel.ReadUncommitted))
            {
                var trx2 = context.Database.BeginTransaction(IsolationLevel.ReadUncommitted);

                context.Database.CommandTimeout = _Timeout;
                pFunc(context, lTransaction);

                trx2.Commit();
                lTransaction.Complete();
            }
        }

        protected void _InReadCommitedTransaction(Action<CT, ITransactionScopeWrapper> pFunc)
        {
            using (var context = CreateDBContext(false))
            using (var lTransaction = _TransactionHelper.BeginTransaction(context, System.Data.IsolationLevel.ReadCommitted))
            {
                context.Database.CommandTimeout = _Timeout;
                pFunc(context, lTransaction);
                lTransaction.Complete();
            }
        }

        protected virtual CT CreateDBContext(bool pOptimizeForBulkIsert)
        {
            var lConnectionString = _SmsClient.GetResource(_ConnectionStringType);
            //@todo _MaestroContextFactory must be tired to generic
            var context = _ContextFactory.FromTamConnectionString(lConnectionString);
            if (context != null)
            {
                context.Database.CommandTimeout = _Timeout;
                if (pOptimizeForBulkIsert)
                {
                    context.Configuration.ValidateOnSaveEnabled = false;
                    context.Configuration.AutoDetectChangesEnabled = false;
                }
            }

            return context;
        }

        public void BulkInsert<T>(System.Data.Entity.DbContext context, List<T> list)
        {
            var tableName = typeof (T).Name;

            var sqlConnection = context.Database.Connection as SqlConnection;
            if (sqlConnection == null)
            {
                throw new Exception("BulkInsert must only be used with a SqlConnection");
            }

            var sqlTransaction = context.Database.CurrentTransaction.UnderlyingTransaction as SqlTransaction;
            if (sqlTransaction == null)
            {
                throw new Exception("BulkInsert must only be used with a SqlTransaction");
            }

            using (var bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.CheckConstraints, sqlTransaction))
            {
                bulkCopy.BatchSize = list.Count;
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.BulkCopyTimeout = _Timeout;

                var table = new DataTable();
                var props = TypeDescriptor.GetProperties(typeof (T))
                    .Cast<PropertyDescriptor>()
                    .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                    .ToArray();

                foreach (var propertyInfo in props)
                {
                    bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                    table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                }

                var values = new object[props.Length];
                foreach (var item in list)
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = props[i].GetValue(item);
                    }

                    table.Rows.Add(values);
                }

                bulkCopy.WriteToServer(table);
            }
        }

        public T DoRetry<T>(Func<T> pFunc, int retryCount = 5, int sleepTimeInMilliSeconds = 50)
        {
            while (true)
            {
                try
                {
                    return pFunc();
                }
                catch
                {
                    if (--retryCount == 0)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(sleepTimeInMilliSeconds);
                    }
                }
            }
        }

        private static readonly object _sync = new object();

        public T DoLockAndRetry<T>(Func<T> pFunc, int retryCount = 5, int sleepTimeInMilliSeconds = 50)
        {
            while (true)
            {
                try
                {
                    lock (_sync)
                    {
                        return pFunc();
                    }
                }
                catch
                {
                    if (--retryCount == 0)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(sleepTimeInMilliSeconds);
                    }
                }
            }
        }
    }
}
