using Services.Broadcast.Entities;
using System.Net;
using System.Net.Mail;
using System.Text;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.BusinessEngines
{
    public interface IAffidavitEmailSenderService
    {
        void Send(OutboundAffidavitFileValidationResultDto invalidFile, string invalidFilePath);
    }

    public class AffidavitEmailSenderService : IAffidavitEmailSenderService
    {
        private const string _EmailSubject = "WWTV File Failed Validation";
        private const int _SmtpPort = 587;

        public void Send(OutboundAffidavitFileValidationResultDto invalidFile, string invalidFilePath)
        {
            if (!BroadcastServiceSystemParameter.EmailNotificationsEnabled)
                return;

            var mailBody = _CreateInvalidFileEmailBody(invalidFile, invalidFilePath);

            _SenInvalidFileEmail(invalidFile, mailBody);
        }

        private void _SenInvalidFileEmail(OutboundAffidavitFileValidationResultDto invalidFile, string mailBody)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(BroadcastServiceSystemParameter.EmailUsername),
                Subject = _EmailSubject,
                Body = mailBody
            };

            mailMessage.To.Add(BroadcastServiceSystemParameter.WWTV_NotificationEmail);

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Host = BroadcastServiceSystemParameter.EmailHost;
                smtpClient.EnableSsl = true;
                smtpClient.Port = _SmtpPort;
                smtpClient.Credentials = new NetworkCredential(BroadcastServiceSystemParameter.EmailUsername, BroadcastServiceSystemParameter.EmailPassword);
                smtpClient.Send(mailMessage);
            }
        }

        private string _CreateInvalidFileEmailBody(OutboundAffidavitFileValidationResultDto invalidFile, string invalidFilePath)
        {
            var mailBody = new StringBuilder();

            mailBody.AppendFormat("File {0} failed validation for WWTV upload", invalidFile.FileName);

            foreach (var errorMessage in invalidFile.ErrorMessages)
            {
                mailBody.Append(errorMessage);
            }

            mailBody.AppendFormat("File located in {0}", invalidFilePath);

            return mailBody.ToString();
        }
    }
}
