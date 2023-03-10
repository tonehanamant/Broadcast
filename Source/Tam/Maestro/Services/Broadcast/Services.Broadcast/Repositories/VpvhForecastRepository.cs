using Common.Services.Repositories;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Data.EntityFrameworkMapping.BroadcastForecast;

namespace Services.Broadcast.Repositories
{
    public interface IVpvhForecastRepository: IDataRepository
    {
        /// <summary>
        /// Get VPVH value from BroadcastForecast database.
        /// </summary>
        /// <param name="standardDaypartId">The standard daypart identifier.</param>
        /// <param name="audienceId">The audience identifier.</param>
        /// <param name="quarterId">The quarter identifier.</param>
        /// <param name="year">The year</param>
        /// <returns>VPVH value</returns>
        double GetVpvhValueFromForecastDb(int standardDaypartId, int audienceId, int quarterId, int year);
    }
    public class VpvhForecastRepository : BroadcastForecastRepositoryBase, IVpvhForecastRepository
    {
        public VpvhForecastRepository(IContextFactory<QueryHintBroadcastForecastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper)
        {

        }
        /// <inheritdoc />
        public double GetVpvhValueFromForecastDb(int standardDaypartId, int audienceId, int quarterId, int year)
        {
            return _InReadUncommitedTransaction(
               context =>
               {
                   return context.vpvh_quarters.Single(x => x.standard_daypart_id == standardDaypartId 
                                                           && x.audience_id == audienceId 
                                                           && x.quarter == quarterId 
                                                           && x.year==year).vpvh;
               });
        }
    }
}
