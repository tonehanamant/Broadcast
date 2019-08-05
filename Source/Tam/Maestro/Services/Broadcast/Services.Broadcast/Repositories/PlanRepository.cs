using Common.Services.Extensions;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using Services.Broadcast.Helpers;
using System;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IPlanRepository : IDataRepository
    {
        /// <summary>
        /// Saves the new plan.
        /// </summary>
        /// <param name="planDto">The plan.</param>
        /// <param name="createdBy">The created by.</param>
        /// <param name="createdDate">The created date.</param>
        /// <returns>Id of the new plan</returns>
        int SaveNewPlan(PlanDto planDto, string createdBy, DateTime createdDate);

        /// <summary>
        /// Saves the plan.
        /// </summary>
        /// <param name="planDto">The plan.</param>
        /// <param name="modifiedBy">The modified by.</param>
        /// <param name="modifiedDate">The modified date.</param>
        void SavePlan(PlanDto planDto, string modifiedBy, DateTime modifiedDate);

        /// <summary>
        /// Gets the plan.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        PlanDto GetPlan(int planId);
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
        public int SaveNewPlan(PlanDto planDto, string createdBy, DateTime createdDate)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var newPlan = new plan
                    {
                        created_by = createdBy,
                        created_date = createdDate,
                    };
                    _HydrateFromDto(newPlan, planDto, context);

                    context.plans.Add(newPlan);
                    context.SaveChanges();
                    return newPlan.id;
                });
        }

        /// <inheritdoc />
        public void SavePlan(PlanDto planDto, string modifiedBy, DateTime modifiedDate)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var plan = context.plans
                        .Include(p => p.plan_flight_hiatus)
                        .Single(p => p.id == planDto.Id, "Invalid plan id.");
                    _HydrateFromDto(plan, planDto, context);

                    context.SaveChanges();
                });
        }

        /// <inheritdoc />
        public PlanDto GetPlan(int planId)
        {
            return _InReadUncommitedTransaction(context =>
                {
                    var entity = context.plans
                        .Include(p => p.plan_flight_hiatus)
                        .Include(p => p.plan_secondary_audiences)
                        .Single(s => s.id == planId, "Invalid plan id.");
                    return _MapToDto(entity);
                });
        }

        private PlanDto _MapToDto(plan entity)
        {
            var dto = new PlanDto
            {
                Id = entity.id,
                CampaignId = entity.campaign_id,
                Name = entity.name,
                SpotLengthId = entity.spot_length_id,
                Equivalized = entity.equivalized,
                Status = EnumHelper.GetEnum<PlanStatusEnum>(entity.status),
                ProductId = entity.product_id,
                FlightStartDate = entity.flight_start_date,
                FlightEndDate = entity.flight_end_date,
                FlightNotes = entity.flight_notes,
                AudienceId = entity.audience_id,
                AudienceType = EnumHelper.GetEnum<AudienceTypeEnum>(entity.audience_type),
                HUTBookId = entity.hut_book_id,
                ShareBookId = entity.share_book_id,
                PostingType = EnumHelper.GetEnum<PostingTypeEnum>(entity.posting_type),
                FlightHiatusDays = entity.plan_flight_hiatus.Select(h => h.hiatus_day).ToList(),
                SecondaryAudiences = entity.plan_secondary_audiences
                    .Select(x => new PlanAudienceDto
                    {
                        AudienceId = x.audience_id,
                        Type = (AudienceTypeEnum)x.audience_type
                    }).ToList()
            };
            return dto;
        }

        /// <summary>
        /// Populates the editable fields from the dto to the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="planDto">The plan dto.</param>
        /// <param name="context">The context.</param>
        private void _HydrateFromDto(plan entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            entity.name = planDto.Name;
            entity.product_id = planDto.ProductId;
            entity.spot_length_id = planDto.SpotLengthId;
            entity.equivalized = planDto.Equivalized;
            entity.status = (int)planDto.Status;
            entity.campaign_id = planDto.CampaignId;

            entity.flight_start_date = planDto.FlightStartDate;
            entity.flight_end_date = planDto.FlightEndDate;
            entity.flight_notes = planDto.FlightNotes;

            entity.audience_id = planDto.AudienceId;
            entity.audience_type = (int)planDto.AudienceType;
            entity.hut_book_id = planDto.HUTBookId;
            entity.share_book_id = planDto.ShareBookId;
            entity.posting_type = (int)planDto.PostingType;
            
            _HydratePlanFlightHiatus(entity, planDto, context);
            _HydratePlanSecondaryAudiences(entity, planDto, context);
        }

        #region Private methods

        private void _HydratePlanFlightHiatus(plan entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_flight_hiatus.RemoveRange(entity.plan_flight_hiatus);
            planDto.FlightHiatusDays.ForEach(d =>
            {
                entity.plan_flight_hiatus.Add(new plan_flight_hiatus { hiatus_day = d });
            });
        }

        private void _HydratePlanSecondaryAudiences(plan entity, PlanDto planDto, QueryHintBroadcastContext context)
        {
            context.plan_secondary_audiences.RemoveRange(entity.plan_secondary_audiences);
            planDto.SecondaryAudiences.ForEach(d =>
            {
                entity.plan_secondary_audiences.Add(new plan_secondary_audiences
                {
                    audience_id = d.AudienceId,
                    audience_type = (int)d.Type
                });
            });
        }
        #endregion
    }
}
