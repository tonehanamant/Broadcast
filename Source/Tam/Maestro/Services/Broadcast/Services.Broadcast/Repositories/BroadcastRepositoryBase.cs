using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

namespace Common.Services.Repositories
{
    public class BroadcastRepositoryBase : CoreRepositoryBase<QueryHintBroadcastContext>
    {
        public BroadcastRepositoryBase(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper, TAMResource.BroadcastConnectionString.ToString())
        {
        }
        public void BulkInsert<T>(DbContext context, List<T> list)
        {
            string name1 = typeof(T).Name;
            SqlConnection connection = context.Database.Connection as SqlConnection;
            if (connection == null)
                throw new Exception("BulkInsert must only be used with a SqlConnection");
            SqlTransaction underlyingTransaction = context.Database.CurrentTransaction.UnderlyingTransaction as SqlTransaction;
            if (underlyingTransaction == null)
                throw new Exception("BulkInsert must only be used with a SqlTransaction");
            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.CheckConstraints | SqlBulkCopyOptions.KeepIdentity, underlyingTransaction))
            {
                sqlBulkCopy.BatchSize = list.Count;
                sqlBulkCopy.DestinationTableName = name1;
                sqlBulkCopy.BulkCopyTimeout = 0; // Infinite - the context should have its own timeout
                DataTable table = new DataTable();
                PropertyDescriptor[] array = TypeDescriptor.GetProperties(typeof(T)).Cast<PropertyDescriptor>().Where<PropertyDescriptor>((Func<PropertyDescriptor, bool>)(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))).ToArray<PropertyDescriptor>();
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
                        objArray[index] = array[index].GetValue((object)obj);
                    table.Rows.Add(objArray);
                }
                sqlBulkCopy.WriteToServer(table);
            }
        }
    }
}
