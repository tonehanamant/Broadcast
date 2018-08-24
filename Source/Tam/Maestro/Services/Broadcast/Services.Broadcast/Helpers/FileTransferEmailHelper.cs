using Common.Services;
using Common.Services.ApplicationServices;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

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

    public class FileTransferEmailHelper : IFileTransferEmailHelper
    {
        private readonly IEmailerService _EmailerService;

        public FileTransferEmailHelper(IEmailerService emailerService)
        {
            _EmailerService = emailerService;
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
            if (!BroadcastServiceSystemParameter.EmailNotificationsEnabled)
                return;

            var from = new MailAddress(BroadcastServiceSystemParameter.EmailUsername);
            var to = new List<MailAddress>() { new MailAddress(BroadcastServiceSystemParameter.WWTV_NotificationEmail) };
            _EmailerService.QuickSend(false, emailBody, subject, MailPriority.Normal, from, to);
        }

    }
}
