using BroadcastLogging;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using log4net;
using Services.Broadcast;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Common.Services.Repositories
{
    public class BroadcastRepositoryBase : CoreRepositoryBase<QueryHintBroadcastContext>
    {
        private readonly ILog _Log;

        public BroadcastRepositoryBase(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper, IConfigurationWebApiClient configurationWebApiClient)
            : base(configurationWebApiClient, pBroadcastContextFactory, pTransactionHelper, TAMResource.BroadcastConnectionString.ToString())
        {
            _Log = LogManager.GetLogger(GetType());
        }

        public string GetDbInfo()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return $"{context.Database.Connection.DataSource}|{context.Database.Connection.Database}";
                });
        }

        public new void BulkInsert<T>(DbContext context, List<T> list)
        {
            BulkInsert(context, list, new List<string>());
        }

        public void BulkInsert<T>(DbContext context, List<T> list, List<string> propertiesToIgnore)
        {
            string name1 = typeof(T).Name;
            if (!(context.Database.Connection is SqlConnection connection))
                throw new Exception("BulkInsert must only be used with a SqlConnection");
            if (!(context.Database.CurrentTransaction.UnderlyingTransaction is SqlTransaction underlyingTransaction))
                throw new Exception("BulkInsert must only be used with a SqlTransaction");
            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.KeepIdentity, underlyingTransaction))
            {
                sqlBulkCopy.BatchSize = list.Count;
                sqlBulkCopy.DestinationTableName = name1;
                sqlBulkCopy.BulkCopyTimeout = 0; // Infinite - the context should have its own timeout
                DataTable table = new DataTable();

                PropertyDescriptor[] properties = TypeDescriptor
                    .GetProperties(typeof(T))
                    .Cast<PropertyDescriptor>()
                    .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System") && !propertiesToIgnore.Contains(propertyInfo.Name))
                    .ToArray<PropertyDescriptor>();

                foreach (PropertyDescriptor propertyDescriptor in properties)
                {
                    sqlBulkCopy.ColumnMappings.Add(propertyDescriptor.Name, propertyDescriptor.Name);
                    DataColumnCollection columns = table.Columns;
                    string name2 = propertyDescriptor.Name;
                    Type type = Nullable.GetUnderlyingType(propertyDescriptor.PropertyType);
                    if ((object)type == null)
                        type = propertyDescriptor.PropertyType;
                    columns.Add(name2, type);
                }
                object[] objArray = new object[properties.Length];
                foreach (T obj in list)
                {
                    for (int index = 0; index < objArray.Length; ++index)
                        objArray[index] = properties[index].GetValue(obj);
                    table.Rows.Add(objArray);
                }
                sqlBulkCopy.WriteToServer(table);
            }
        }

        protected virtual void _LogInfo(string message, string username = null, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName, username);
            _Log.Info(logMessage.ToJson());
        }

        protected virtual void _LogWarning(string message, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Warn(logMessage.ToJson());
        }

        protected virtual void _LogError(string message, Exception ex = null, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Error(logMessage.ToJson(), ex);
        }

        protected virtual void _LogDebug(string message, [CallerMemberName]string memberName = "")
        {
            var logMessage = BroadcastLogMessageHelper.GetApplicationLogMessage(message, GetType(), memberName);
            _Log.Debug(logMessage.ToJson());
        }
    }
}
