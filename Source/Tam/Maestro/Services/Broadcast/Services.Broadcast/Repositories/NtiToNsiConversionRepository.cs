using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface INtiToNsiConversionRepository : IDataRepository
    {
        List<NtiToNsiConversionRate> GetLatestNtiToNsiConversionRates();
    }

    public class NtiToNsiConversionRepository : BroadcastRepositoryBase, INtiToNsiConversionRepository
    {
        public NtiToNsiConversionRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, 
            IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public List<NtiToNsiConversionRate> GetLatestNtiToNsiConversionRates()
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var latestMediaMonth = context.nti_to_nsi_conversion_rates
                        .Select(x => x.media_month_id)
                        .ToList()
                        .Max();

                    return context.nti_to_nsi_conversion_rates
                        .Where(x => x.media_month_id == latestMediaMonth)
                        .Select(x => new NtiToNsiConversionRate
                        {
                            Id = x.id,
                            ConversionRate = x.conversion_rate,
                            DaypartDefaultId = x.daypart_default_id,
                            MediaMonthId = x.media_month_id
                        })
                        .ToList();
                });
        }
    }
}
