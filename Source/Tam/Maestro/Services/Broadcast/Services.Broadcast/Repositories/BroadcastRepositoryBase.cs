using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Common.Services.Repositories
{
    public class BroadcastRepositoryBase : CoreRepositoryBase<QueryHintBroadcastContext>
    {
        public BroadcastRepositoryBase(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper, IConfigurationWebApiClient configurationWebApiClient)
            : base(configurationWebApiClient, pBroadcastContextFactory, pTransactionHelper, TAMResource.BroadcastConnectionString.ToString())
        {
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
                PropertyDescriptor[] array = TypeDescriptor.GetProperties(typeof(T)).Cast<PropertyDescriptor>().Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System")).ToArray<PropertyDescriptor>();
                foreach (PropertyDescriptor propertyDescriptor in array)
                {
                    sqlBulkCopy.ColumnMappings.Add(propertyDescriptor.Name, propertyDescriptor.Name);
                    DataColumnCollection columns = table.Columns;
                    string name2 = propertyDescriptor.Name;
                    Type type = Nullable.GetUnderlyingType(propertyDescriptor.PropertyType);
                    if ((object)type == null)
                        type = propertyDescriptor.PropertyType;
                    columns.Add(name2, type);
                }
                object[] objArray = new object[array.Length];
                foreach (T obj in list)
                {
                    for (int index = 0; index < objArray.Length; ++index)
                        objArray[index] = array[index].GetValue(obj);
                    table.Rows.Add(objArray);
                }
                sqlBulkCopy.WriteToServer(table);
            }
        }
    }
}
