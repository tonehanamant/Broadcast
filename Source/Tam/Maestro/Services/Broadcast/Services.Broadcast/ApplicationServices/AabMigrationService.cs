using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;

namespace Services.Broadcast.ApplicationServices
{
    public interface IAabMigrationService : IApplicationService
    {
        /// <summary>
        /// Validate it's safe to migrate to the AabApi and reports the state of the migration data.
        /// </summary>
        ServiceExecutionResultWithData<AabMigrationValidationReport> ValidateAndReportCanMigrateToAabApi();

        /// <summary>
        /// Migrates the Aab Source data from the TrafficApi to the AabApi.
        /// </summary>
        ServiceExecutionResult MigrateAabToAabApi();

        /// <summary>
        /// Migrates the Aab Source data from the AabApi to the TrafficApi.
        /// </summary>
        ServiceExecutionResult MigrateAabToTrafficApi();
    }

    public class AabMigrationService : BroadcastBaseClass, IAabMigrationService
    {
        private readonly IAabDataMigrationRepository _AabDataMigrationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AabMigrationService"/> class.
        /// </summary>
        /// <param name="dataRepositoryFactory">The data repository factory.</param>
        public AabMigrationService(IDataRepositoryFactory dataRepositoryFactory)
        {
            _AabDataMigrationRepository = dataRepositoryFactory.GetDataRepository<IAabDataMigrationRepository>();
        }

        /// <inheritdoc />
        public ServiceExecutionResultWithData<AabMigrationValidationReport> ValidateAndReportCanMigrateToAabApi()
        {
            var result = new ServiceExecutionResultWithData<AabMigrationValidationReport>();

            var report = _AabDataMigrationRepository.ReportMigrationMetrics();
            report.MissingAgencies = _AabDataMigrationRepository.GetMissingAgencies();
            report.MissingAdvertisers = _AabDataMigrationRepository.GetMissingAdvertisers();
            report.MissingProducts = _AabDataMigrationRepository.GetMissingProducts();

            result.IsSuccess = true;
            result.Data = report;

            return result;
        }

        /// <inheritdoc />
        public ServiceExecutionResult MigrateAabToAabApi()
        {
            _LogInfo("Attempting to migrate the Aab Source data from the TrafficApi to the AabApi.");

            var result = new ServiceExecutionResult();

            try
            {
                _LogInfo("Performing pre-migration validation...");
                var validationReport = ValidateAndReportCanMigrateToAabApi();
                if (!validationReport.Data.IsSafeToMigrate)
                {
                    result.IsSuccess = false;
                    result.Message = "Validation failed.  Unable to Migrate.  Run the validation report for more info.";
                    return result;
                }

                _AabDataMigrationRepository.MigrateAabToAabApi();
                _LogInfo("Finished migrating the Aab Source data from the TrafficApi to the AabApi.");
                result.IsSuccess = true;
                result.Message = "Aab data migrated to the AabApi.";
                return result;
            }
            catch (Exception e)
            {
                _LogError("Exception caught attempting to migrate the Aab Source data from the TrafficApi to the AabApi.", e);
                throw new ApplicationException("Failed to migrate the data.", e);
            }
        }

        /// <inheritdoc />
        public ServiceExecutionResult MigrateAabToTrafficApi()
        {
            _LogInfo("Attempting to migrate the Aab Source data from the AabApi to the TrafficApi.");

            var result = new ServiceExecutionResult();

            try
            {
                _AabDataMigrationRepository.MigrateAabToTrafficApi();
                _LogInfo("Finished migrating the Aab Source data from the AabApi to the TrafficApi.");

                result.IsSuccess = true;
                result.Message = "Aab data migrated to the TrafficApi.";
                return result;
            }
            catch (Exception e)
            {
                _LogError("Exception caught attempting to migrate the Aab Source data from the AabApi to the TrafficApi.", e);
                throw new ApplicationException("Failed to migrate the data.", e);
            }
        }
    }
}
