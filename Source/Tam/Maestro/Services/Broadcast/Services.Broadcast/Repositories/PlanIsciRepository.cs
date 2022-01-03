using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Isci;
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
        /// <param name="IsciPlanMappingSaveDto">The List which contains save parameters</param>
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
        /// /// <param name="deletedBy">Deleted By</param>
        /// /// <param name="deletedAt">Deleted At</param>
        /// <returns>Total Number Of Deleted Plan ISCI Mappings</returns>
        int DeleteIsciPlanMappings(List<int> isciPlanMappingsIdsToDelete, string deletedBy, DateTime deletedAt);

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
        /// Gets the isci spot lengths.
        /// </summary>
        /// <param name="iscis">The iscis.</param>
        /// <returns></returns>
        List<IsciSpotLengthDto> GetIsciSpotLengths(List<string> iscis);

        /// <summary>
        /// Updates the isci plan mappings.
        /// </summary>
        /// <param name="isciPlanMappings">The isci plan mappings.</param>
        /// <returns></returns>
        int UpdateIsciPlanMappings(List<IsciPlanModifiedMappingDto> isciPlanMappings);

        /// <summary>
        /// Uns the delete isci plan mappings.
        /// </summary>
        /// <param name="idsToUnDelete">The ids to un delete.</param>
        /// <returns></returns>
        int UnDeleteIsciPlanMappings(List<int> idsToUnDelete);
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
                              join pro in context.reel_isci_products on isci.isci equals pro.isci into ps
                              from pro in ps.DefaultIfEmpty()
                              join sl in context.spot_lengths on isci.spot_length_id equals sl.id
                              where (isci.active_start_date <= startDate && isci.active_end_date >= endDate)
                              || (isci.active_start_date >= startDate && isci.active_start_date <= endDate)
                              || (isci.active_end_date >= startDate && isci.active_end_date <= endDate)
                              select new IsciAdvertiserDto
                              {
                                  AdvertiserName = isci_adr.advertiser_name_reference,
                                  Id = isci.id,
                                  SpotLengthDuration = sl.length,
                                  ProductName = pro.product_name,
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
                                    select plans)
                                    .Include(x => x.campaign)
                                    .Include(x => x.plan_versions)
                                    .Include(p => p.plan_versions.Select(x => x.plan_version_creative_lengths))
                                    .Include(p => p.plan_versions.Select(x => x.plan_version_creative_lengths.Select(y => y.spot_lengths)))
                                    .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts))
                                    .Include(p => p.plan_versions.Select(x => x.plan_version_dayparts.Select(y => y.standard_dayparts)))
                                    .Include(p => p.plan_versions.Select(x => x.plan_version_summaries))
                                    .Include(p => p.plan_versions.Select(x => x.audience))
                                    .Include(x => x.plan_iscis)
                                    .ToList();

                var result = planEntities.Select(plan =>
                {
                    var planVersion = plan.plan_versions.Where(x => x.id == plan.latest_version_id).Single();
                    var planVersionSummary = planVersion.plan_version_summaries.Single();
                    var isciPlanDetail = new IsciPlanDetailDto()
                    {
                        Id = plan.id,
                        Title = plan.name,
                        AdvertiserMasterId = plan.campaign.advertiser_master_id,
                        SpotLengthValues = planVersion.plan_version_creative_lengths.Select(x => x.spot_lengths.length).ToList(),
                        AudienceCode = planVersion.audience.code,
                        Dayparts = planVersion.plan_version_dayparts.Select(d => d.standard_dayparts.code).ToList(),
                        FlightStartDate = planVersion.flight_start_date,
                        FlightEndDate = planVersion.flight_end_date,
                        ProductName = planVersionSummary.product_name,
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
                        flight_end_date = isciPlanMapping.FlightEndDate
                    });
                });
                
                var savedCount = context.SaveChanges();
                return savedCount;
            });
        }

        /// <inheritdoc />
        public int UpdateIsciPlanMappings(List<IsciPlanModifiedMappingDto> isciPlanMappings)
        {
            return _InReadUncommitedTransaction(context =>
            {
                isciPlanMappings.ForEach(item =>
                {
                    var found = context.plan_iscis.Single(m => m.id == item.PlanIsciMappingId);
                    found.flight_start_date = item.FlightStartDate;
                    found.flight_end_date = item.FlightEndDate;
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
                var result = context.plan_iscis
                    .Where(x => x.plan_id == planId && x.deleted_at == null)
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
        public List<IsciSpotLengthDto> GetIsciSpotLengths(List<string> iscis)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var isciDetails = context.reel_iscis
                    .Where(s => iscis.Contains(s.isci))
                    .Select(s => new IsciSpotLengthDto
                    {
                        Isci = s.isci,
                        SpotLengthId = s.spot_length_id
                    })
                    .Distinct()
                    .ToList();

                return isciDetails;
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
    }
}