using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface IDaypartCodeRepository : IDataRepository
    {
        bool ActiveDaypartCodeExists(string daypartCode);
        DaypartCode GetDaypartCodeByCode(string daypartCode);
    }

    public class DaypartCodeRepository : BroadcastRepositoryBase, IDaypartCodeRepository
    {
        public DaypartCodeRepository(
            ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory, 
            ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public bool ActiveDaypartCodeExists(string daypartCode)
        {
            return _InReadUncommitedTransaction(context => context.daypart_codes.Any(x => x.is_active && x.name == daypartCode));
        }

        public DaypartCode GetDaypartCodeByCode(string daypartCode)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var queryResult = context.daypart_codes.FirstOrDefault(x => x.is_active && x.name == daypartCode);

                return queryResult == null ? 
                    null :
                    new DaypartCode
                    {
                        Id = queryResult.id,
                        Name = queryResult.name
                    };
            });
        }
    }
}
