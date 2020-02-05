using Common.Services.Repositories;
using ConfigurationService.Client;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities.Enums;
using System;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryProgramsJobsRepository
    {
        void UpdateJobStatus(int jobId, InventoryProgramsJobStatus status, string message = null);

        void UpdateJobMessage(int jobId, string message);

        void SetJobCompleteError(int jobId, string errorMessage);

        void SetJobCompleteWarning(int jobId, string warningMessage = null);

        void SetJobCompleteSuccess(int jobId, string message = null);
    }

    public abstract class InventoryProgramsJobsRepositoryBase : BroadcastRepositoryBase, IInventoryProgramsJobsRepository
    {
        public InventoryProgramsJobsRepositoryBase(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        protected abstract void _UpdateJob(int jobId, InventoryProgramsJobStatus status, DateTime? completedAt);

        protected abstract void _UpdateJobNotes(int jobId, string message, DateTime timestamp);

        public void UpdateJobStatus(int jobId, InventoryProgramsJobStatus status, string message = null)
        {
            _UpdateJob(jobId, status, null);
            if (string.IsNullOrWhiteSpace(message) == false)
            {
                _UpdateJobNotes(jobId, message, DateTime.Now);
            }
        }

        public void UpdateJobMessage(int jobId, string message)
        {
            _UpdateJobNotes(jobId, message, DateTime.Now);
        }

        public void SetJobCompleteError(int jobId, string errorMessage)
        {
            _UpdateJob(jobId, InventoryProgramsJobStatus.Error, DateTime.Now);
            if (string.IsNullOrWhiteSpace(errorMessage) == false)
            {
                _UpdateJobNotes(jobId, errorMessage, DateTime.Now);
            }
        }

        public void SetJobCompleteWarning(int jobId, string warningMessage = null)
        {
            _UpdateJob(jobId, InventoryProgramsJobStatus.Warning, DateTime.Now);
            if (string.IsNullOrWhiteSpace(warningMessage) == false)
            {
                _UpdateJobNotes(jobId, warningMessage, DateTime.Now);
            }
        }

        public void SetJobCompleteSuccess(int jobId, string message = null)
        {
            _UpdateJob(jobId, InventoryProgramsJobStatus.Completed, DateTime.Now);
            if (string.IsNullOrWhiteSpace(message) == false)
            {
                _UpdateJobNotes(jobId, message, DateTime.Now);
            }
        }
    }
}