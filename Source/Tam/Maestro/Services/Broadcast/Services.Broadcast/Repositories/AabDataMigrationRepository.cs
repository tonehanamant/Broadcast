using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Linq;
using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    /// <summary>
    /// Performs data migration operations.
    /// </summary>
    /// <seealso cref="Common.Services.Repositories.IDataRepository" />
    public interface IAabDataMigrationRepository : IDataRepository
    {
        /// <summary>
        /// Migrates the Aab Source data from the TrafficApi to the AabApi.
        /// </summary>
        void MigrateAabToAabApi();

        /// <summary>
        /// Migrates the Aab Source data from the AabApi to the TrafficApi.
        /// </summary>
        void MigrateAabToTrafficApi();

        /// <summary>
        /// Reports the referenced Maestro Agencies that are not in Aab.
        /// </summary>
        List<AgencyDto> GetMissingAgencies();

        /// <summary>
        /// Reports the referenced Maestro Advertisers that are not in Aab.
        /// </summary>
        List<AdvertiserDto> GetMissingAdvertisers();

        /// <summary>
        /// Reports the referenced Maestro Products that are not in Aab.
        /// </summary>
        List<ProductDto> GetMissingProducts();

        /// <summary>
        /// Reports the migration metrics.
        /// </summary>
        AabMigrationValidationReport ReportMigrationMetrics();
    }

    /// <summary>
    /// This repository deals with everything related to isci_blacklist and isci_mapping tables
    /// </summary>
    public class AabDataMigrationRepository : BroadcastRepositoryBase, IAabDataMigrationRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AabDataMigrationRepository"/> class.
        /// </summary>
        /// <param name="pBroadcastContextFactory">The p broadcast context factory.</param>
        /// <param name="pTransactionHelper">The p transaction helper.</param>
        /// <param name="pConfigurationWebApiClient">The p configuration web API client.</param>
        public AabDataMigrationRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }


        /// <inheritdoc />
        public List<AgencyDto> GetMissingAgencies()
        {
            const string sql = "SELECT DISTINCT m.id, m.[name], m.master_id as MasterId"
                      + " FROM broadcast.dbo.campaigns b"
                      + " JOIN maestro.dbo.companies m"
                      + " ON b.agency_id = m.id"
                      + " WHERE b.agency_id IS NOT NULL"
                      + " AND m.master_id IS NULL"
                      + " UNION"
                      + " SELECT DISTINCT bb.id, bb.[name], bb.master_id"
                      + " FROM "
                      + " ("
                      + " SELECT m.id, m.[name], m.master_id"
                      + " FROM broadcast.dbo.campaigns b"
                      + " JOIN maestro.dbo.companies m"
                      + " ON b.agency_id = m.id"
                      + " WHERE b.agency_id IS NOT NULL"
                      + " AND m.master_id is not null"
                + " ) bb"
                + " LEFT OUTER JOIN aab.dbo.companies a"
                + " ON a.master_guid = bb.master_id"
                + " WHERE a.id is null";

            return _InReadUncommitedTransaction(
                context =>
                {
                    var deltas = context.Database.SqlQuery<AgencyDto>(sql).ToList();
                    return deltas;
                });
        }

        /// <inheritdoc />
        public List<AdvertiserDto> GetMissingAdvertisers()
        {
            const string sql = "SELECT DISTINCT m.id, m.[name], m.master_id as MasterId"
                               + " FROM broadcast.dbo.campaigns b"
                               + " JOIN maestro.dbo.companies m"
                               + " ON b.advertiser_id = m.id"
                               + " WHERE b.advertiser_id IS NOT NULL"
                               + " AND m.master_id IS NULL"
                               + " UNION"
                               + " SELECT DISTINCT bb.id, bb.[name], bb.master_id"
                               + " FROM "
                               + " ("
                               + " SELECT m.id, m.[name], m.master_id"
                               + " FROM broadcast.dbo.campaigns b"
                               + " JOIN maestro.dbo.companies m"
                               + " ON b.advertiser_id = m.id"
                               + " WHERE b.advertiser_id IS NOT NULL"
                               + " AND m.master_id is not null"
                               + " ) bb"
                               + " LEFT OUTER JOIN aab.dbo.companies a"
                               + " ON a.master_guid = bb.master_id"
                               + " WHERE a.id is null";

            return _InReadUncommitedTransaction(
                context =>
                {
                    var deltas = context.Database.SqlQuery<AdvertiserDto>(sql).ToList();
                    return deltas;
                });
        }

        /// <inheritdoc />
        public List<ProductDto> GetMissingProducts()
        {
            const string sql = "SELECT DISTINCT m.id, m.[name], m.master_id"
                      + " FROM broadcast.dbo.plans b"
                      + " JOIN maestro.dbo.Products m"
                      + " ON b.product_id = m.id"
                      + " WHERE b.product_id is not null"
                      + " AND m.master_id IS NULL"
                      + " UNION"
                      + " SELECT DISTINCT bb.id, bb.[name], bb.master_id as MasterId"
                      + " FROM "
                      + " ("
                      + " SELECT m.id, m.[name], m.master_id"
                      + " FROM broadcast.dbo.plans b"
                      + " JOIN maestro.dbo.products m"
                      + " ON b.product_id = m.id"
                      + " WHERE b.product_id is not null"
                      + " AND m.master_id IS NULL"
                      + " ) bb"
                      + " LEFT OUTER JOIN aab.dbo.products a"
                      + " ON a.master_id = bb.master_id"
                      + " WHERE a.id is null";

            return _InReadUncommitedTransaction(
                context =>
                {
                    var deltas = context.Database.SqlQuery<ProductDto>(sql).ToList();
                    return deltas;
                });
        }

        /// <inheritdoc />
        public AabMigrationValidationReport ReportMigrationMetrics()
        {
            const string sql = "SELECT *"
                               + " FROM "
                               + " (SELECT COUNT(1) as CampaignCount from broadcast.dbo.campaigns) c,"
                               + " (SELECT COUNT(1) as NullAgencyMasterCount from broadcast.dbo.campaigns WHERE agency_master_id IS NULL) d,"
                               + " (SELECT COUNT(1) as NullAdvertiserMasterCount from broadcast.dbo.campaigns WHERE advertiser_master_id IS NULL) e,"
                               + " ("
                               + " SELECT COUNT(1) AS AgencyMasterAabMismatchCount"
                               + " from broadcast.dbo.campaigns b"
                               + " LEFT OUTER JOIN aab.dbo.companies a"
                               + " ON b.agency_master_id = a.master_guid"
                               + " WHERE a.master_guid IS NULL"
                               + " ) a,"
                               + " ("
                               + " SELECT COUNT(1) AS AgencyIdAabMismatchCount"
                               + " from broadcast.dbo.campaigns b"
                               + " LEFT OUTER JOIN aab.dbo.companies a"
                               + " ON b.agency_id = a.id"
                               + " WHERE a.id IS NULL"
                               + " ) b,"
                               + " ("
                               + " SELECT COUNT(1) AS AdvertiserMasterAabMismatchCount"
                               + " from broadcast.dbo.campaigns b"
                               + " LEFT OUTER JOIN aab.dbo.companies a"
                               + " ON b.advertiser_master_id = a.master_guid"
                               + " WHERE a.master_guid IS NULL"
                               + " ) f,"
                               + " ("
                               + " SELECT COUNT(1) AS AdvertiserIdAabMismatchCount"
                               + " from broadcast.dbo.campaigns b"
                               + " LEFT OUTER JOIN aab.dbo.companies a"
                               + " ON b.advertiser_id = a.id"
                               + " WHERE a.id IS NULL"
                               + " ) g,"
                               + " (SELECT COUNT(1) as PlanCount from broadcast.dbo.plans) h,"
                               + " (SELECT COUNT(1) as NullProductMasterCount from broadcast.dbo.plans where product_master_id IS NULL) i,"
                               + " ("
                               + " SELECT COUNT(1) AS ProductMasterAabMismatch"
                               + " FROM broadcast.dbo.plans b"
                               + " LEFT OUTER JOIN aab.dbo.products a"
                               + " on b.product_master_id = a.master_id"
                               + " WHERE a.master_id IS NULL"
                               + " ) j,"
                               + " ("
                               + " SELECT COUNT(1) AS ProductIdAabMismatch"
                               + " FROM broadcast.dbo.plans b"
                               + " LEFT OUTER JOIN aab.dbo.products a"
                               + " on b.product_id = a.id"
                               + " WHERE a.id IS NULL"
                               + " ) k";

            return _InReadUncommitedTransaction(
                context =>
                {
                    var deltas = context.Database.SqlQuery<AabMigrationValidationReport>(sql).FirstOrDefault();
                    return deltas;
                });
        }

        /// <inheritdoc />
        public void MigrateAabToAabApi()
        {
            const string agenciesMasterIdUpdateSql = "UPDATE a SET"
                                            + " agency_master_id = o.master_id"
                                            + " FROM broadcast.dbo.campaigns a"
                                            + " JOIN maestro.dbo.companies o"
                                            + " ON a.agency_id = o.id"
                                            + " WHERE a.agency_master_id IS NULL";

            const string advertisersMasterIdUpdateSql = "UPDATE a SET"
                                                        + " advertiser_master_id = o.master_id"
                                                        + " FROM broadcast.dbo.campaigns a"
                                                        + " JOIN maestro.dbo.companies o"
                                                        + " ON a.advertiser_id = o.id"
                                                        + " WHERE a.advertiser_master_id IS NULL";

            const string productsMasterIdUpdateSql = "UPDATE a SET"
                                                     + " product_master_id = o.master_id"
                                                     + " FROM broadcast.dbo.plans a"
                                                     + " JOIN maestro.dbo.products o"
                                                     + " ON a.product_id = o.id"
                                                     + " WHERE a.product_master_id IS NULL";

            const string agenciesIdUpdateSql = "UPDATE a SET"
                                               + " agency_id = o.id"
                                               + " FROM broadcast.dbo.campaigns a"
                                               + " JOIN aab.dbo.companies o"
                                               + " ON a.agency_master_id = o.master_guid"
                                               + " WHERE agency_id<> o.id";

            const string advertisersIdUpdateSql = "UPDATE a SET"
                                                  + " advertiser_id = o.id"
                                                  + " FROM broadcast.dbo.campaigns a"
                                                  + " JOIN aab.dbo.companies o"
                                                  + " ON a.advertiser_master_id = o.master_guid"
                                                  + " WHERE advertiser_id<> o.id";

            const string productsIdUpdateSql = "UPDATE a SET"
                                               + " product_id = o.id"
                                               + " FROM broadcast.dbo.plans a"
                                               + " JOIN aab.dbo.products o"
                                               + " ON a.product_master_id = o.master_id"
                                               + " WHERE product_id<> o.id";

            _InReadUncommitedTransaction(
                context =>
                {
                    context.Database.ExecuteSqlCommand(agenciesMasterIdUpdateSql);
                    context.Database.ExecuteSqlCommand(advertisersMasterIdUpdateSql);
                    context.Database.ExecuteSqlCommand(productsMasterIdUpdateSql);

                    context.Database.ExecuteSqlCommand(agenciesIdUpdateSql);
                    context.Database.ExecuteSqlCommand(advertisersIdUpdateSql);
                    context.Database.ExecuteSqlCommand(productsIdUpdateSql);
                });
        }

        /// <inheritdoc />
        public void MigrateAabToTrafficApi()
        {
            // leave the master ids alone.
            const string agenciesIdUpdateSql = "UPDATE a SET"
                                               + " agency_id = o.id"
                                               + " FROM broadcast.dbo.campaigns a"
                                               + " JOIN maestro.dbo.companies o"
                                               + " ON a.agency_master_id = o.master_id"
                                               + " WHERE agency_id<> o.id";

            const string advertisersIdUpdateSql = "UPDATE a SET"
                                                  + " advertiser_id = o.id"
                                                  + " FROM broadcast.dbo.campaigns a"
                                                  + " JOIN maestro.dbo.companies o"
                                                  + " ON a.advertiser_master_id = o.master_id"
                                                  + " WHERE advertiser_id<> o.id";

            const string productsIdUpdateSql = "UPDATE a SET"
                                               + " product_id = o.id"
                                               + " FROM broadcast.dbo.plans a"
                                               + " JOIN maestro.dbo.products o"
                                               + " ON a.product_master_id = o.master_id"
                                               + " WHERE product_id<> o.id";

            _InReadUncommitedTransaction(
                context =>
                {
                    context.Database.ExecuteSqlCommand(agenciesIdUpdateSql);
                    context.Database.ExecuteSqlCommand(advertisersIdUpdateSql);
                    context.Database.ExecuteSqlCommand(productsIdUpdateSql);
                });
        }
    }
}
