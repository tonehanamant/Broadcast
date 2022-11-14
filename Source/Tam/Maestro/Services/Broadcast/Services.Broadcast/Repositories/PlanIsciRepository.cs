using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Isci;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IPlanIsciRepository : IDataRepository
    {
        /// <summary>
        /// list of Iscis based on search key.
        /// </summary>
        /// <param name="startDate">Isci search start date input</param> 
        /// <param name="endDate">Isci search end date input</param> 
        List<IsciAdvertiserDto> GetAvailableIscis(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the available plans for Isci mapping
        /// </summary>
        /// <param name="mediaMonthStartDate">The media month start date</param>
        /// <param name="mediaMonthEndDate">The media month end date</param>
        /// <returns>List of IsciPlanDetailDto object</returns>
        List<IsciPlanDetailDto> GetAvailableIsciPlans(DateTime mediaMonthStartDate, DateTime mediaMonthEndDate);

        /// <summary>
        /// Save Plan Isci mapping
        /// </summary>
        /// <param name="isciPlanMappings">The List which contains save parameters</param>
        /// /// <param name="createdBy">Created By</param>
        /// /// <param name="createdAt">Created At</param>
        /// <returns>Total number of inserted Plan Mappings</returns>
        int SaveIsciPlanMappings(List<PlanIsciDto> isciPlanMappings, string createdBy, DateTime createdAt);

        /// <summary>
        /// Get PlanIsci List
        /// </summary>
        /// <returns>List of IsciPlanMappingDto</returns>
        List<PlanIsciDto> GetPlanIscis();

        /// <summary>
        /// Gets the plan iscis.
        /// </summary>
        /// <param name="planIds">The plan ids.</param>
        /// <returns></returns>
        List<PlanIsciDto> GetPlanIscis(List<int> planIds);

        /// <summary>
        /// Gets the deleted plan iscis.
        /// </summary>
        /// <param name="planIds">The plan ids.</param>
        /// <returns></returns>
        List<PlanIsciDto> GetDeletedPlanIscis(List<int> planIds);

        /// <summary>
        /// Gets the plan iscis.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        List<PlanIsciDto> GetPlanIscis(int planId);

        /// <summary>
        /// Delete Plan Isci mappings
        /// </summary>
        /// <param name="isciPlanMappingsIdsToDelete">The List of ids to delete.</param>
        /// <param name="deletedBy">Deleted By</param>
        /// <param name="deletedAt">Deleted At</param>
        /// <returns>Total Number Of Deleted Plan ISCI Mappings</returns>
        int DeleteIsciPlanMappings(List<int> isciPlanMappingsIdsToDelete, string deletedBy, DateTime deletedAt);

        /// <summary>
        /// Delete Plan Isci mappings
        /// </summary>
        /// <param name="planId">The plan id to delete.</param>
        /// <param name="deletedBy">Deleted By</param>
        /// <param name="deletedAt">Deleted At</param>
        /// <returns>Total Number Of Deleted Plan ISCI Mappings</returns>
        int DeleteIsciPlanMappings(int planId, string deletedBy, DateTime deletedAt);

        /// <summary>
        /// Updates the isci plan mappings.
        /// </summary>
        /// <param name="isciPlanMappingsToUpdate">The isci plan mappings ids to update.</param>
        /// <param name="modifiedAt">The modified at.</param>
        /// <param name="modifiedBy">The modified by.</param>
        /// <returns><br /></returns>
        int UpdateIsciPlanMappings(List<PlanIsciDto> isciPlanMappingsToUpdate, DateTime modifiedAt, string modifiedBy);

        /// <summary>
        /// Deletes plan isci that do not exist in reel isci
        /// </summary>
        /// <param name="deletedAt">The time when plan isci deletes</param>
        /// <param name="deletedBy">The user who deletes plan isci</param>
        /// <returns>Total number of deleted plan isci</returns>
        int DeletePlanIscisNotExistInReelIsci(DateTime deletedAt, string deletedBy);

        /// <summary>
        /// Given a list of mapping ids, will return the MappedPlanCount for each Isci.
        /// </summary>
        /// <param name="isciPlanMappingIds">The isci plan mapping ids.</param>
        List<IsciMappedPlanCountDto> GetIsciPlanMappingCounts(List<int> isciPlanMappingIds);

        /// <summary>
        /// Gets the plan iscis by mapping identifier.
        /// </summary>
        /// <param name="isciPlanMappingId">The isci plan mapping identifier.</param>
        /// <returns><br /></returns>
        List<PlanIsciDto> GetPlanIscisByMappingId(int isciPlanMappingId);

        /// <summary>
        /// Gets the plan isci duplicates.
        /// </summary>
        /// <param name="modified">The modified.</param>
        /// <returns><br /></returns>
        List<PlanIsciDto> GetPlanIsciDuplicates(List<IsciPlanModifiedMappingDto> modified);

        /// <summary>
        /// Uns the delete isci plan mappings.
        /// </summary>
        /// <param name="idsToUnDelete">The ids to un delete.</param>
        /// <returns></returns>
        int UnDeleteIsciPlanMappings(List<int> idsToUnDelete);

        /// <summary>
        /// Search the Existing plan iscis based on the Plan
        /// </summary>        
        /// <param name="advertiserMasterId">advertiserMasterId</param>
        /// <returns>List of found iscis</returns>
        List<SearchPlan> GetMappedIscis(Guid? advertiserMasterId);

        /// <summary>
        /// Get the isci plans basis of plan ID
        /// </summary>
        /// <param name="advertiserMasterId">advertiserMasterId</param>
        /// <param name="sourcePlanId">Input plan id</param>
        /// <returns>Plan Iscis</returns>
        List<plan> GetTargetIsciPlans(Guid? advertiserMasterId, int sourcePlanId);
    }

    /// <summary>
    /// Data operations for the PlanIsci Repository.
    /// </summary>
    /// <seealso cref="BroadcastRepositoryBase" />
    /// <seealso cref="IPlanIsciRepository" />
    public class PlanIsciRepository : BroadcastRepositoryBase, IPlanIsciRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanIsciRepository"/> class.
        /// </summary>
        /// <param name="pBroadcastContextFactory">The p broadcast context factory.</param>
        /// <param name="pTransactionHelper">The p transaction helper.</param>
        /// <param name="configurationSettingsHelper">The configuration settings helper.</param>
        public PlanIsciRepository(
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }
        public List<IsciAdvertiserDto> GetAvailableIscis(DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = (from isci in context.reel_iscis
                              join isci_adr in context.reel_isci_advertiser_name_references on isci.id equals isci_adr.reel_isci_id
                              join sl in context.spot_lengths on isci.spot_length_id equals sl.id
                              where (isci.active_start_date <= startDate && isci.active_end_date >= endDate)
                              || (isci.active_start_date >= startDate && isci.active_start_date <= endDate)
                              || (isci.active_end_date >= startDate && isci.active_end_date <= endDate)
                              select new IsciAdvertiserDto
                              {
                                  AdvertiserName = isci_adr.advertiser_name_reference,
                                  Id = isci.id,
                                  SpotLengthDuration = sl.length,
                                  Isci = isci.isci,
                                  PlanIsci = context.plan_iscis
                                                    .Where(planIsci => planIsci.deleted_at == null
                                                            && (planIsci.flight_start_date <= startDate && planIsci.flight_end_date >= endDate
                                                                || planIsci.flight_start_date >= startDate && planIsci.flight_start_date <= endDate
                                                                || planIsci.flight_end_date >= startDate && planIsci.flight_end_date <= endDate))
                                                    .Count(planIsci => planIsci.isci == isci.isci)
                              }).ToList();
                return result;
            });
        }

        /// <inheritdoc />
        public List<IsciPlanDetailDto> GetAvailableIsciPlans(DateTime mediaMonthStartDate, DateTime mediaMonthEndDate)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var planEntities = (from plans in context.plans
                                    join plan_versions in context.plan_versions on plans.latest_version_id equals plan_versions.id
                                    where ((plan_versions.flight_start_date <= mediaMonthStartDate && plan_versions.flight_end_date >= mediaMonthEndDate)
                                              || (plan_versions.flight_start_date >= mediaMonthStartDate && plan_versions.flight_start_date <= mediaMonthEndDate)
                                              || (plan_versions.flight_end_date >= mediaMonthStartDate && plan_versions.flight_end_date <= mediaMonthEndDate))
                                          && (plan_versions.status == (int)PlanStatusEnum.Contracted
                                              || plan_versions.status == (int)PlanStatusEnum.Live
                                              || plan_versions.status == (int)PlanStatusEnum.Complete)
                                          && !(plans.deleted_at.HasValue)
                                    select plans)
                                    .Include(x => x.campaign)
                                    .Include(x => x.plan_versions)
                                    .Include(p => p.plan_versions.Select(x => x.plan_version_creative_lengths))
                                    .Include(p => p.plan_versions.Select(x => x.plan_version_creative_lengths.Select(y => y.spot_lengths)))
                                    .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts))
                                    .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(y => y.standard_dayparts)))
                                    .Include(p => p.plan_versions.Select(x => x.plan_version_summaries))
                                    .Include(p => p.plan_versions.Select(x => x.audience))
                                    .Include(p => p.plan_versions.Select(x => x.plan_version_flight_hiatus_days))
                                    .Include(p => p.plan_versions.Select(x => x.plan_version_flight_days))
                                    .Include(x => x.plan_iscis)
                                    .ToList();

                var result = planEntities.Select(plan =>
                {
                    var planVersion = plan.plan_versions.Where(x => x.id == plan.latest_version_id).Single();
                    var isciPlanDetail = new IsciPlanDetailDto()
                    {
                        Id = plan.id,
                        Title = plan.name,
                        AdvertiserMasterId = plan.campaign.advertiser_master_id,
                        SpotLengthValues = planVersion.plan_version_creative_lengths.Select(x => x.spot_lengths.length).ToList(),
                        AudienceCode = planVersion.audience.code,
                        PlanDayparts = planVersion.plan_version_dayparts.Select(x => new PlanDaypartDto { DaypartCodeId = x.standard_daypart_id }).ToList(),
                        FlightHiatusDays = planVersion.plan_version_flight_hiatus_days.Select(x => x.hiatus_day).ToList(),
                        FlightDays = planVersion.plan_version_flight_days.Select(x => x.day_id).ToList(),
                        Dayparts = planVersion.plan_version_dayparts.Select(d => d.standard_dayparts.code).ToList(),
                        FlightStartDate = planVersion.flight_start_date,
                        FlightEndDate = planVersion.flight_end_date,
                        ProductMasterId = plan.product_master_id,
                        UnifiedTacticLineId = plan.unified_tactic_line_id,
                        UnifiedCampaignLastSentAt = plan.unified_campaign_last_sent_at,
                        UnifiedCampaignLastReceivedAt = plan.unified_campaign_last_received_at,
                        Iscis = plan.plan_iscis
                                        .Where(planIsci => planIsci.deleted_at == null
                                                && (planIsci.flight_start_date <= mediaMonthStartDate && planIsci.flight_end_date >= mediaMonthEndDate
                                                    || planIsci.flight_start_date >= mediaMonthStartDate && planIsci.flight_start_date <= mediaMonthEndDate
                                                    || planIsci.flight_end_date >= mediaMonthStartDate && planIsci.flight_end_date <= mediaMonthEndDate))
                                        .Select(planIsci => planIsci.isci).Distinct().ToList()
                    };
                    return isciPlanDetail;
                }).ToList();
                return result;
            });
        }

        /// <inheritdoc />
        public int SaveIsciPlanMappings(List<PlanIsciDto> isciPlanMappings, string createdBy, DateTime createdAt)
        {
            return _InReadUncommitedTransaction(context =>
            {
                isciPlanMappings.ForEach(isciPlanMapping =>
                {
                    context.plan_iscis.Add(new plan_iscis()
                    {
                        plan_id = isciPlanMapping.PlanId,
                        isci = isciPlanMapping.Isci,
                        created_at = createdAt,
                        created_by = createdBy,
                        flight_start_date = isciPlanMapping.FlightStartDate,
                        flight_end_date = isciPlanMapping.FlightEndDate,
                        spot_length_id = isciPlanMapping.SpotLengthId,
                        modified_at = createdAt,
                        modified_by = createdBy,
                        start_time = isciPlanMapping.StartTime,
                        end_time = isciPlanMapping.EndTime
                    });
                });

                var savedCount = context.SaveChanges();
                return savedCount;
            });
        }

        /// <inheritdoc />
        public int UnDeleteIsciPlanMappings(List<int> idsToUnDelete)
        {
            return _InReadUncommitedTransaction(context =>
            {
                idsToUnDelete.ForEach(id =>
                {
                    var found = context.plan_iscis.Single(m => m.id == id);
                    found.deleted_at = null;
                    found.deleted_by = null;
                });

                var savedCount = context.SaveChanges();
                return savedCount;
            });
        }

        /// <inheritdoc />
        public int DeleteIsciPlanMappings(List<int> isciPlanMappingsIdsToDelete, string deletedBy, DateTime deletedAt)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var deletedCount = 0;
                if (!(isciPlanMappingsIdsToDelete?.Any() ?? false))
                {
                    return deletedCount;
                }

                var isciPlanMappingDeletedList = context.plan_iscis
                    .Where(x => isciPlanMappingsIdsToDelete.Contains(x.id) && x.deleted_at == null)
                    .ToList();

                if (!isciPlanMappingDeletedList.Any())
                {
                    return deletedCount;
                }

                foreach (var isciPlanMappingDeletedObj in isciPlanMappingDeletedList)
                {
                    isciPlanMappingDeletedObj.deleted_at = deletedAt;
                    isciPlanMappingDeletedObj.deleted_by = deletedBy;
                    deletedCount++;
                }

                context.SaveChanges();
                return deletedCount;
            });
        }

        /// <inheritdoc />
        public int DeleteIsciPlanMappings(int planId, string deletedBy, DateTime deletedAt)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var deletedCount = 0;

                var isciPlanMappingsToDelete = context.plan_iscis
                    .Where(planIsci => planIsci.plan_id == planId && !(planIsci.deleted_at.HasValue))
                    .ToList();

                if (!isciPlanMappingsToDelete.Any())
                {
                    return deletedCount;
                }

                isciPlanMappingsToDelete.ForEach(planIsci =>
                {
                    planIsci.deleted_by = deletedBy;
                    planIsci.deleted_at = deletedAt;
                });

                deletedCount = context.SaveChanges();
                return deletedCount;
            });
        }

        /// <inheritdoc />
        public int UpdateIsciPlanMappings(List<PlanIsciDto> isciPlanMappingsToUpdate, DateTime modifiedAt, string modifiedBy)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var updatedCount = 0;
                if (!isciPlanMappingsToUpdate.Any())
                {
                    return updatedCount;
                }

                isciPlanMappingsToUpdate.ForEach(plan =>
                {
                    var foundPlan = context.plan_iscis.Single(x => x.id == plan.Id);

                    foundPlan.flight_start_date = plan.FlightStartDate;
                    foundPlan.flight_end_date = plan.FlightEndDate;
                    foundPlan.spot_length_id = plan.SpotLengthId;
                    foundPlan.modified_at = modifiedAt;
                    foundPlan.modified_by = modifiedBy;
                    foundPlan.start_time = plan.StartTime;
                    foundPlan.end_time = plan.EndTime;
                    updatedCount++;
                });

                context.SaveChanges();
                return updatedCount;
            });
        }

        /// <inheritdoc />
        public List<PlanIsciDto> GetPlanIscis()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_iscis
                    .Where(x => x.deleted_at == null)
                    .Select(x => new PlanIsciDto
                    {
                        Id = x.id,
                        PlanId = x.plan_id,
                        Isci = x.isci,
                        FlightStartDate = x.flight_start_date,
                        FlightEndDate = x.flight_end_date
                    })
                    .ToList();
                return result;
            });
        }

        /// <inheritdoc />
        public List<PlanIsciDto> GetPlanIscis(List<int> planIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_iscis
                    .Where(x => planIds.Contains(x.plan_id) && x.deleted_at == null)
                    .Select(x => new PlanIsciDto
                    {
                        Id = x.id,
                        PlanId = x.plan_id,
                        Isci = x.isci,
                        FlightStartDate = x.flight_start_date,
                        FlightEndDate = x.flight_end_date
                    })
                    .ToList();
                return result;
            });
        }

        /// <inheritdoc />
        public List<PlanIsciDto> GetDeletedPlanIscis(List<int> planIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_iscis
                    .Where(x => planIds.Contains(x.plan_id) && x.deleted_at != null)
                    .Select(x => new PlanIsciDto
                    {
                        Id = x.id,
                        PlanId = x.plan_id,
                        Isci = x.isci,
                        FlightStartDate = x.flight_start_date,
                        FlightEndDate = x.flight_end_date
                    })
                    .ToList();
                return result;
            });
        }

        /// <inheritdoc />
        public List<PlanIsciDto> GetPlanIscis(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = (from isciMappings in context.plan_iscis
                              where isciMappings.plan_id == planId
                              && isciMappings.deleted_at == null
                              select new PlanIsciDto
                              {
                                  Id = isciMappings.id,
                                  PlanId = isciMappings.plan_id,
                                  Isci = isciMappings.isci,
                                  FlightStartDate = isciMappings.flight_start_date,
                                  FlightEndDate = isciMappings.flight_end_date,
                                  SpotLengthId = isciMappings.spot_length_id,
                                  StartTime = isciMappings.start_time,
                                  EndTime = isciMappings.end_time
                              }).Distinct().ToList();
                return result;
            });
        }

        /// <inheritdoc />
        public int DeletePlanIscisNotExistInReelIsci(DateTime deletedAt, string deletedBy)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var sql = $@"UPDATE plan_iscis
                                SET deleted_at = @deletedAt,
                                    deleted_by = @deletedBy
                            WHERE deleted_at IS NULL AND isci NOT IN(SELECT isci FROM reel_iscis)";
                var deletedAtParameter = new SqlParameter("@deletedAt", deletedAt);
                var deletedByParameter = new SqlParameter("@deletedBy", deletedBy);
                var deletedCount = context.Database.ExecuteSqlCommand(sql, deletedAtParameter, deletedByParameter);
                return deletedCount;
            });
        }

        /// <inheritdoc />
        public List<IsciMappedPlanCountDto> GetIsciPlanMappingCounts(List<int> isciPlanMappingIds)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var iscis = context.plan_iscis
                    .Where(d => isciPlanMappingIds.Contains(d.id))
                    .Select(s => s.isci)
                    .Distinct();

                var isciPlanCounts = context.plan_iscis
                    .Where(d => iscis.Contains(d.isci))
                    .GroupBy(d => d.isci)
                    .Select(d => new IsciMappedPlanCountDto
                    {
                        Isci = d.Key,
                        MappedPlanCount = d.Count(s => !s.deleted_at.HasValue)
                    })
                    .ToList();

                return isciPlanCounts;
            });
        }

        public List<PlanIsciDto> GetPlanIscisByMappingId(int isciPlanMappingId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.plan_iscis
                    .Where(m => m.id == isciPlanMappingId)
                    .Select(x => new PlanIsciDto
                    {
                        Id = x.id,
                        PlanId = x.plan_id,
                        Isci = x.isci,
                        FlightStartDate = x.flight_start_date,
                        FlightEndDate = x.flight_end_date,
                        SpotLengthId = x.spot_length_id,
                        StartTime = x.start_time,
                        EndTime = x.end_time
                    })
                    .ToList();
                return result;
            });
        }

        public List<PlanIsciDto> GetPlanIsciDuplicates(List<IsciPlanModifiedMappingDto> modified)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var duplicateMappings = new List<PlanIsciDto>();
                modified.ForEach(item =>
                {
                    var found = context.plan_iscis.Single(m => m.id == item.PlanIsciMappingId);

                    var result = (from isciMappings in context.plan_iscis
                                  join modifyingMappings in context.plan_iscis
                                  on new { isciMappings.isci, isciMappings.id } equals
                                  new { modifyingMappings.isci, modifyingMappings.id } into dups
                                  from duplicates in dups.DefaultIfEmpty()
                                  where duplicates.plan_id == found.plan_id
                                  && duplicates.isci == found.isci
                                  && duplicates.flight_start_date == item.FlightStartDate
                                  && duplicates.flight_end_date == item.FlightEndDate
                                  select new PlanIsciDto
                                  {
                                      Id = duplicates.id,
                                      PlanId = duplicates.plan_id,
                                      Isci = duplicates.isci,
                                      FlightStartDate = duplicates.flight_start_date,
                                      FlightEndDate = duplicates.flight_end_date,
                                      DeletedAt = duplicates.deleted_at

                                  });
                    if (result != null)
                    {
                        duplicateMappings.AddRange(result);
                    }
                });
                return duplicateMappings;
            });
        }

        public List<SearchPlan> GetMappedIscis(Guid? advertiserMasterId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var searchIscis = context.plan_iscis
                    .Include(x => x.plan)
                    .Include(x => x.plan.campaign)
                    .Include(x => x.spot_lengths)
                    .Where(x => x.plan.campaign.advertiser_master_id == advertiserMasterId)
                    .Select(d => new SearchPlan
                    {
                        Isci = d.isci                       
                    }).ToList();
                return searchIscis;
            });
        }

        public List<plan> GetTargetIsciPlans(Guid? advertiserMasterId, int sourcePlanId)
        {
            return _InReadUncommitedTransaction(context =>
            {

                var planEntities = (from plans in context.plans
                                    where (plans.campaign.advertiser_master_id == advertiserMasterId && plans.id!= sourcePlanId)
                                    select plans)
                                    .Include(x => x.campaign)
                                    .Include(x => x.plan_versions)
                                    .Include(p => p.plan_versions.Select(y => y.plan_version_creative_lengths))
                                    .Include(p => p.plan_versions.Select(y => y.plan_version_dayparts.Select(z => z.standard_dayparts)))
                                    .Include(p => p.plan_versions.Select(y => y.audience))
                                    .Include(x => x.plan_iscis)
                                    .ToList();
                return planEntities;
            });
        }
    }
}