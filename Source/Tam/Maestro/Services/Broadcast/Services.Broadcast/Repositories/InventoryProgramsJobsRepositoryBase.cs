using Common.Services.Repositories;
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

        void UpdateJobNotes(int jobId, string message);

        void SetJobCompleteError(int jobId, string statusMessage, string notesMessage);

        void SetJobCompleteWarning(int jobId, string statusMessage, string notesMessage);

        void SetJobCompleteSuccess(int jobId, string statusMessage, string notesMessage);
    }

    public abstract class InventoryProgramsJobsRepositoryBase : BroadcastRepositoryBase, IInventoryProgramsJobsRepository
    {
        public InventoryProgramsJobsRepositoryBase(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationSettingsHelper configurationSettingsHelper)
            : base(pBroadcastContextFactory, pTransactionHelper, configurationSettingsHelper) { }

        protected abstract void _UpdateJob(int jobId, InventoryProgramsJobStatus status, string statusMessage, DateTime? completedAt);

        protected abstract void _UpdateJobNotes(int jobId, string message, DateTime timestamp);

        public void UpdateJobStatus(int jobId, InventoryProgramsJobStatus status, string message = null)
        {
            _UpdateJob(jobId, status, null, null);
            if (string.IsNullOrWhiteSpace(message) == false)
            {
                _UpdateJobNotes(jobId, message, DateTime.Now);
            }
        }

        public void UpdateJobNotes(int jobId, string message)
        {
            _UpdateJobNotes(jobId, message, DateTime.Now);
        }

        public void SetJobCompleteError(int jobId, string statusMessage, string notesMessage)
        {
            _UpdateJob(jobId, InventoryProgramsJobStatus.Error, statusMessage, DateTime.Now);
            if (string.IsNullOrWhiteSpace(notesMessage) == false)
            {
                _UpdateJobNotes(jobId, notesMessage, DateTime.Now);
            }
        }

        public void SetJobCompleteWarning(int jobId, string statusMessage, string notesMessage)
        {
            _UpdateJob(jobId, InventoryProgramsJobStatus.Warning, statusMessage, DateTime.Now);
            if (string.IsNullOrWhiteSpace(notesMessage) == false)
            {
                _UpdateJobNotes(jobId, notesMessage, DateTime.Now);
            }
        }

        public void SetJobCompleteSuccess(int jobId, string statusMessage, string notesMessage)
        {
            _UpdateJob(jobId, InventoryProgramsJobStatus.Completed, statusMessage, DateTime.Now);
            if (string.IsNullOrWhiteSpace(notesMessage) == false)
            {
                _UpdateJobNotes(jobId, notesMessage, DateTime.Now);
            }
        }

        protected string _GetSizedStatusMessage(string candidate)
        {
            const int statusMessageFieldSize = 200;
            if (candidate?.Length > statusMessageFieldSize)
            {
                return candidate.Substring(0, statusMessageFieldSize - 4) + "...";
            }
            return candidate;
        }
    }
}