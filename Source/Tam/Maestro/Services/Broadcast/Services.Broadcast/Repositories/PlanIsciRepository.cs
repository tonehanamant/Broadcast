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
        /// <param name="isciPlanMappings">The List which contains save parameters</param>
        /// /// <param name="createdBy">Created By</param>
        /// /// <param name="createdAt">Created At</param>
        /// <returns>Total number of inserted Plan Mappings</returns>
        int SaveIsciPlanMappings(List<IsciPlanMappingDto> isciPlanMappings, string createdBy, DateTime createdAt);

        /// <summary>
        /// Save Product Isci mapping
        /// </summary>
        /// <param name="isciProductMappings">The List which contains save parameters</param>
        /// /// <param name="createdBy">Created By</param>
        /// /// <param name="createdAt">Created At</param>
        /// <returns>Total number of inserted Product Mappings</returns>
        int SaveIsciProductMappings(List<IsciProductMappingDto> isciProductMappings, string createdBy, DateTime createdAt);

        /// <summary>
        /// Gets the isci product mappings for the given list of icsis.
        /// </summary>
        /// <param name="iscis">The iscis.</param>
        /// <returns></returns>
        List<IsciProductMappingDto> GetIsciProductMappings(List<string> iscis);

        /// <summary>
        /// Get PlanIsci List
        /// </summary>
        /// <returns>List of IsciPlanMappingDto</returns>
        List<IsciPlanMappingDto> GetPlanIscis();

        /// <summary>
        /// Gets the plan iscis.
        /// </summary>
        /// <param name="planId">The plan identifier.</param>
        /// <returns></returns>
        List<IsciPlanMappingDto> GetPlanIscis(int planId);

        /// <summary>
        /// Delete Plan Isci mappings
        /// </summary>
        /// <param name="isciPlanMappingsDeleted">The List which contains save parameters</param>
        /// /// <param name="deletedBy">Created By</param>
        /// /// <param name="deletedAt">Created At</param>
        /// <returns>Total Number Of Deleted Plan ISCI Mappings</returns>
        int DeleteIsciPlanMappings(List<IsciPlanMappingDto> isciPlanMappingsDeleted, string deletedBy, DateTime deletedAt);

        /// <summary>
        /// Deletes plan isci that do not exist in reel isci
        /// </summary>
        /// <param name="deletedAt">The time when plan isci deletes</param>
        /// <param name="deletedBy">The user who deletes plan isci</param>
        /// <returns>Total number of deleted plan isci</returns>
        int DeletePlanIscisNotExistInReelIsci(DateTime deletedAt, string deletedBy);

        List<IsciPlanMappingIsciDetailsDto> GetIsciDetails(List<string> iscis);
    }
    /// <summary>
    /// Data operations for the PlanIsci Repository.
    /// </summary>
    /// <seealso cref="BroadcastRepositoryBase" />
    /// <seealso cref="IPlanIsciRepository" />
    public class PlanIsciRepository : BroadcastRepositoryBase, IPlanIsciRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignRepository"/> class.
        /// </summary>
        /// <param name="pBroadcastContextFactory">The p broadcast context factory.</param>
        /// <param name="pTransactionHelper">The p transaction helper.</param>
        /// <param name="pConfigurationWebApiClient">The p configuration web API client.</param>
        /// <param name="featureToggleHelper">The p configuration web API client.</param>
        /// <param name="configurationSettingsHelper">The p configuration web API client.</param>
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
                                  PlanIsci = context.plan_iscis.Count(x => x.isci == isci.isci)
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
                        Iscis = plan.plan_iscis.Where(x => x.deleted_at == null).Select(x => x.isci).ToList()
                    };
                    return isciPlanDetail;
                }).ToList();
                return result;
            });
        }

        public int SaveIsciPlanMappings(List<IsciPlanMappingDto> isciPlanMappings, string createdBy, DateTime createdAt)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var isciPlanMappingsToAdd = isciPlanMappings.Select(isciPlanMapping => new plan_iscis()
                {
                    plan_id = isciPlanMapping.PlanId,
                    isci = isciPlanMapping.Isci,
                    created_at = createdAt,
                    created_by = createdBy
                }).ToList();

                var addedCount = context.plan_iscis.AddRange(isciPlanMappingsToAdd).Count();
                context.SaveChanges();
                return addedCount;
            });
        }

        public List<IsciProductMappingDto> GetIsciProductMappings(List<string> iscis)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = context.reel_isci_products
                    .Where(i => iscis.Contains(i.isci))
                    .Select(i => new IsciProductMappingDto
                    {
                        Isci = i.isci,
                        ProductName = i.product_name
                    })
                    .ToList();
                return result;
            });
        }

        public int SaveIsciProductMappings(List<IsciProductMappingDto> isciProductMappings, string createdBy, DateTime createdAt)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var isciProductMappingsToAdd = isciProductMappings.Select(isciProductMapping => new reel_isci_products()
                {
                    product_name = isciProductMapping.ProductName,
                    isci = isciProductMapping.Isci,
                    created_at = createdAt,
                    created_by = createdBy
                }).ToList();
                var addedCount = context.reel_isci_products.AddRange(isciProductMappingsToAdd).Count();
                context.SaveChanges();
                return addedCount;
            });
        }
        public List<IsciPlanMappingDto> GetPlanIscis()
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = (from planIsci in context.plan_iscis.Where(x => x.deleted_at == null)
                              select new IsciPlanMappingDto
                              {
                                  PlanId = planIsci.plan_id,
                                  Isci = planIsci.isci
                              }).ToList();
                return result;
            });
        }

        public List<IsciPlanMappingDto> GetPlanIscis(int planId)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var result = (from planIsci in context.plan_iscis.Where(x => x.plan_id == planId && x.deleted_at == null)
                    select new IsciPlanMappingDto
                    {
                        PlanId = planIsci.plan_id,
                        Isci = planIsci.isci
                    }).ToList();
                return result;
            });
        }

        public int DeleteIsciPlanMappings(List<IsciPlanMappingDto> isciPlanMappingsDeleted, string deletedBy, DateTime deletedAt)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var deletedCount = 0;
                if (isciPlanMappingsDeleted == null)
                {
                    return deletedCount;
                }
                foreach (var item in isciPlanMappingsDeleted)
                {
                    var isciPlanMappingDeletedList = context.plan_iscis.Where(x => item.PlanId == x.plan_id && item.Isci == x.isci && x.deleted_at == null);

                    if (isciPlanMappingDeletedList != null)
                    {
                        foreach (var isciPlanMappingDeletedObj in isciPlanMappingDeletedList)
                        {
                            isciPlanMappingDeletedObj.deleted_at = deletedAt;
                            isciPlanMappingDeletedObj.deleted_by = deletedBy;
                            deletedCount++;
                        }
                    }
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

        public List<IsciPlanMappingIsciDetailsDto> GetIsciDetails(List<string> iscis)
        {
            return _InReadUncommitedTransaction(context =>
            {
                var isciDetails = context.reel_iscis
                    .Where(s => iscis.Contains(s.isci))
                    .Select(s => new IsciPlanMappingIsciDetailsDto
                    {
                        Isci = s.isci,
                        SpotLengthId = s.spot_length_id,
                        AdvertiserName = s.reel_isci_advertiser_name_references.First().advertiser_name_reference,
                        FlightStartDate = s.active_start_date,
                        FlightEndDate = s.active_end_date
                    })
                    .ToList();

                return isciDetails;
            });
        }

    }
}