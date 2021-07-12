using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IDayRepository : IDataRepository
    {
        List<Day> GetDays();
    }

    public class DayRepository : BroadcastRepositoryBase, IDayRepository
    {
        public DayRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper,
            IConfigurationWebApiClient pConfigurationWebApiClient, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient, featureToggleHelper, configurationSettingsHelper) { }

        public List<Day> GetDays()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    return context.days
                        .Select(x => new Day
                        {
                            Id = x.id,
                            Code1 = x.code_1,
                            Code2 = x.code_2,
                            Code3 = x.code_3,
                            Name = x.name,
                            Ordinal = x.ordinal
                        })
                        .ToList();
                });
        }
    }
}
