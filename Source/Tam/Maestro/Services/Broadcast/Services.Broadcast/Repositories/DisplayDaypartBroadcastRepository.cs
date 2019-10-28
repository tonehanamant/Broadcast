using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.Common;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Services.Broadcast.Repositories
{
    public class ResultForTimespan
    {
        public int id { get; set; }
        public int start_time { get; set; }
        public int end_time { get; set; }
    }

    public class ResultForDaypart
    {
        public int id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public int tier { get; set; }
        public int start_time { get; set; }
        public int end_time { get; set; }
        public int mon { get; set; }
        public int tue { get; set; }
        public int wed { get; set; }
        public int thu { get; set; }
        public int fri { get; set; }
        public int sat { get; set; }
        public int sun { get; set; }
        public string daypart_text { get; set; }
        public double total_hours { get; set; }
    }

    public interface IDisplayDaypartRepository : IDataRepository
    {
        DisplayDaypart GetDisplayDaypart(int pDaypartId);
        Dictionary<int, DisplayDaypart> GetDisplayDayparts(IEnumerable<int> pDaypartIds);
        int SaveDaypart(DisplayDaypart pDaypart);
        int GetDisplayDaypartIdByText(string pDaypartText);
        int OnlyForTests_SaveDaypartInternal(DisplayDaypart pDaypart);
    }

    public class DisplayDaypartBroadcastRepository : BroadcastRepositoryBase, IDisplayDaypartRepository
    {
        public DisplayDaypartBroadcastRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public int SaveDaypart(DisplayDaypart pDaypart)
        {
            using (var trx = new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var daypartId = OnlyForTests_SaveDaypartInternal(pDaypart);
                trx.Complete();
                return daypartId;
            }
        }

        public int OnlyForTests_SaveDaypartInternal(DisplayDaypart pDaypart)
        {
            using (var trx = new TransactionScopeWrapper())
            {
                if (!pDaypart.IsValid)
                {
                    throw new Exception("Cannot save the invalid Daypart: " + pDaypart.ToString());
                }

                var daypartId = -1;
                var resultForDaypart = _GetExistingDaypart(pDaypart);

                var timespanId = -1;
                if (resultForDaypart == null)
                {
                    _ThrowIfMigrationNeeded();

                    var resultForTimespan = _GetExistingTimespan(pDaypart);
                    if (resultForTimespan == null)
                    {
                        timespanId = _InsertTimeSpan(pDaypart);
                    }
                    else
                    {
                        timespanId = resultForTimespan.id;
                    }

                    daypartId = _InsertDaypartAndDays(pDaypart, timespanId, 1);
                }
                else
                {
                    daypartId = resultForDaypart.id;
                }

                trx.Complete();
                return daypartId;
            }
        }

        private ResultForTimespan _GetExistingTimespan(DisplayDaypart pDaypart)
        {
            using (var scope = new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        return context.Database.SqlQuery<ResultForTimespan>(
                            "usp_PCS_LookupTimespan @start_time, @end_time",
                            new System.Data.SqlClient.SqlParameter("@start_time", pDaypart.StartTime),
                            new System.Data.SqlClient.SqlParameter("@end_time", pDaypart.EndTime)
                        ).SingleOrDefault();
                    });
            }
        }

        private ResultForDaypart _GetExistingDaypart(DisplayDaypart pDaypart)
        {
            using (var scope = new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        return context.Database.SqlQuery<ResultForDaypart>(
                            "usp_PCS_LookupDaypart @start_time, @end_time, @mon, @tue, @wed, @thu, @fri, @sat, @sun",
                            new System.Data.SqlClient.SqlParameter("@start_time", pDaypart.StartTime),
                            new System.Data.SqlClient.SqlParameter("@end_time", pDaypart.EndTime),
                            new System.Data.SqlClient.SqlParameter("@mon", pDaypart.Monday ? 1 : 0),
                            new System.Data.SqlClient.SqlParameter("@tue", pDaypart.Tuesday ? 1 : 0),
                            new System.Data.SqlClient.SqlParameter("@wed", pDaypart.Wednesday ? 1 : 0),
                            new System.Data.SqlClient.SqlParameter("@thu", pDaypart.Thursday ? 1 : 0),
                            new System.Data.SqlClient.SqlParameter("@fri", pDaypart.Friday ? 1 : 0),
                            new System.Data.SqlClient.SqlParameter("@sat", pDaypart.Saturday ? 1 : 0),
                            new System.Data.SqlClient.SqlParameter("@sun", pDaypart.Sunday ? 1 : 0)
                        ).SingleOrDefault();
                    });
            }
        }

        private void _ThrowIfMigrationNeeded()
        {
            if (_DaypartTableIsEmpty())
            {
                throw new Exception("The Broadcast system is missing daypart data. Please make sure that the Daypart migration Script has been run: MigrateDaypartsFromCableToBroadcast.sql");
            }
        }

        private bool _DaypartTableIsEmpty()
        {
            using (
                var scope = new TransactionScopeWrapper(
                    System.Transactions.TransactionScopeOption.Suppress,
                    System.Transactions.IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        return !context.dayparts.Any();
                    });
            }
        }

        private int _InsertTimeSpan(DisplayDaypart daypart)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var newTimeSpan = context.timespans.Create();
                    newTimeSpan.start_time = daypart.StartTime;
                    newTimeSpan.end_time = daypart.EndTime;

                    context.timespans.Add(newTimeSpan);
                    context.SaveChanges();

                    return newTimeSpan.id;
                });
        }

        private int _InsertDaypartAndDays(DisplayDaypart daypart, int timespanId, int tier)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var newDaypart = context.dayparts.Create();
                    newDaypart.timespan_id = timespanId;
                    newDaypart.code = daypart.Code;
                    newDaypart.name = daypart.Name;
                    newDaypart.tier = tier;
                    newDaypart.daypart_text = daypart.ToString();
                    newDaypart.total_hours = daypart.TotalHours;

                    context.dayparts.Add(newDaypart);
                    context.SaveChanges();

                    if (daypart.Monday)
                    {
                        context.Database.ExecuteSqlCommand("insert into daypart_days values ({0},{1})", newDaypart.id, 1);
                    }
                    if (daypart.Tuesday)
                    {
                        context.Database.ExecuteSqlCommand("insert into daypart_days values ({0},{1})", newDaypart.id, 2);
                    }
                    if (daypart.Wednesday)
                    {
                        context.Database.ExecuteSqlCommand("insert into daypart_days values ({0},{1})", newDaypart.id, 3);
                    }
                    if (daypart.Thursday)
                    {
                        context.Database.ExecuteSqlCommand("insert into daypart_days values ({0},{1})", newDaypart.id, 4);
                    }
                    if (daypart.Friday)
                    {
                        context.Database.ExecuteSqlCommand("insert into daypart_days values ({0},{1})", newDaypart.id, 5);
                    }
                    if (daypart.Saturday)
                    {
                        context.Database.ExecuteSqlCommand("insert into daypart_days values ({0},{1})", newDaypart.id, 6);
                    }
                    if (daypart.Sunday)
                    {
                        context.Database.ExecuteSqlCommand("insert into daypart_days values ({0},{1})", newDaypart.id, 7);
                    }

                    return newDaypart.id;
                });
        }

        public DisplayDaypart GetDisplayDaypart(int pDaypartId)
        {
            using (
                var scope = new TransactionScopeWrapper(
                    System.Transactions.TransactionScopeOption.Suppress,
                    System.Transactions.IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        var daypart = (from x in context.vw_ccc_daypart
                            where x.id == pDaypartId
                            select x).SingleOrDefault();

                        if (daypart == null)
                        {
                            _ThrowIfMigrationNeeded();
                            throw new ApplicationException("Could not find a daypart with id " + pDaypartId);
                        }

                        return new DisplayDaypart(
                            daypart.id,
                            daypart.code,
                            daypart.name,
                            daypart.start_time,
                            daypart.end_time,
                            daypart.mon == 1,
                            daypart.tue == 1,
                            daypart.wed == 1,
                            daypart.thu == 1,
                            daypart.fri == 1,
                            daypart.sat == 1,
                            daypart.sun == 1);
                    });
            }
        }

        public Dictionary<int, DisplayDaypart> GetDisplayDayparts(IEnumerable<int> pDaypartIds)
        {
            pDaypartIds = pDaypartIds.Distinct();
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
                return _InReadUncommitedTransaction(c =>
                            {
                                var dict = new Dictionary<int, DisplayDaypart>();
                                var dayparts = c.vw_ccc_daypart.Where(d => pDaypartIds.Contains(d.id));
                                foreach (var daypart in dayparts)
                                {
                                    dict.Add(daypart.id,
                                        new DisplayDaypart(daypart.id, daypart.code,daypart.name,daypart.start_time, daypart.end_time, daypart.mon == 1,
                                            daypart.tue == 1, daypart.wed == 1, daypart.thu == 1, daypart.fri == 1, daypart.sat == 1,
                                            daypart.sun == 1));
                                }
                                return dict;
                            });
        }

        public int GetDisplayDaypartIdByText(string pDaypartText)
        {
            return _InReadUncommitedTransaction(
              context =>
              {
                  var query = (from dp in context.dayparts
                               where dp.daypart_text == pDaypartText
                               select dp.id);

                  return query.FirstOrDefault();
              });
        }
    }
}
