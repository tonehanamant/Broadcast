using Common.Services;
using Common.Services.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace Services.Broadcast.Helpers
{
    public interface IFileTransferEmailHelper : IApplicationService
    {
        /// <summary>
        /// Creates an email body containing all the errors from a specific file
        /// </summary>
        /// <param name="errorMessages">Error messages present in the file</param>
        /// <param name="invalidFilePath">Invalid file path</param>
        /// <param name="fileName">File name</param>
        /// <returns>Email body</returns>
        string CreateInvalidDataFileEmailBody(List<string> errorMessages, string invalidFilePath, string fileName);

        /// <summary>
        /// Sends an email using the emailBody and subject to WWTV_NotificationEmail email address
        /// </summary>
        /// <param name="emailBody">Email body</param>
        /// <param name="subject">Email subject</param>
        void SendEmail(string emailBody, string subject);
    }

    public class FileTransferEmailHelper : BroadcastBaseClass, IFileTransferEmailHelper
    {
        private readonly IEmailerService _EmailerService;
        private readonly Lazy<bool> _IsEmailNotificationsEnabled;

        public FileTransferEmailHelper(IEmailerService emailerService, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
        : base(featureToggleHelper, configurationSettingsHelper)
        {
            _EmailerService = emailerService;
            _IsEmailNotificationsEnabled = new Lazy<bool>(() => _FeatureToggleHelper.IsToggleEnabledUserAnonymous(FeatureToggles.EMAIL_NOTIFICATIONS));
        }

        /// <summary>
        /// Creates an email body containing all the errors from a specific file
        /// </summary>
        /// <param name="errorMessages">Error messages present in the file</param>
        /// <param name="invalidFilePath">Invalid file path</param>
        /// <param name="fileName">File name</param>
        /// <returns>Email body</returns>
        public string CreateInvalidDataFileEmailBody(List<string> errorMessages, string invalidFilePath, string fileName)
        {
            var mailBody = new StringBuilder();

            mailBody.AppendFormat("File {0} failed validation for WWTV upload\n\n", fileName);

            foreach (var errorMessage in errorMessages)
            {
                mailBody.AppendFormat("{0}\n\n", errorMessage);
            }

            mailBody.AppendFormat("\nFile located in {0}\n", invalidFilePath);

            return mailBody.ToString();
        }

        /// <summary>
        /// Sends an email using the emailBody and subject to WWTV_NotificationEmail email address
        /// </summary>
        /// <param name="emailBody">Email body</param>
        /// <param name="subject">Email subject</param>
        public void SendEmail(string emailBody, string subject)
        {
            if (!_IsEmailNotificationsEnabled.Value)
                return;

            var to = new List<MailAddress>() { new MailAddress(_ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.BroadcastNotificationEmail)) };
            _EmailerService.QuickSend(false, emailBody, subject, MailPriority.Normal, to);
        }

    }
}
