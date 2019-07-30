using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IPlanRepository : IDataRepository
    {
        /// <summary>
        /// Saves the new plan.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns>Id of the new plan</returns>
        int SaveNewPlan(CreatePlanDto plan, string createdBy, DateTime createdDate);
    }

    public class PlanRepository : BroadcastRepositoryBase, IPlanRepository
    {
        public PlanRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient)
        {
        }

        /// <inheritdoc/>
        public int SaveNewPlan(CreatePlanDto plan, string createdBy, DateTime createdDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var newPlan = new plan
                    {
                        campaign_id = plan.CampaignId,
                        created_by = createdBy,
                        created_date = createdDate,
                        equivalized = plan.Equivalized,
                        name = plan.Name,
                        product_id = plan.ProductId,
                        spot_length_id = plan.SpotLengthId,
                        status = (int)plan.Status
                    };
                    context.plans.Add(newPlan);
                    context.SaveChanges();
                    return newPlan.id;
                });
        }
    }
}
