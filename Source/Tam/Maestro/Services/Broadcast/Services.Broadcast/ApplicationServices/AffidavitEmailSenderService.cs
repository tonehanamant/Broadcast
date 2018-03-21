using System.Net;
using System.Net.Mail;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.BusinessEngines
{
    public interface IAffidavitEmailSenderService
    {
        void Send(string emailBody);
    }

    public class AffidavitEmailSenderService : IAffidavitEmailSenderService
    {
        private const string _EmailSubject = "WWTV File Failed Validation";
        private const int _SmtpPort = 587;

        public void Send(string emailBody)
        {
            if (!BroadcastServiceSystemParameter.EmailNotificationsEnabled)
                return;

            _SendInvalidFileEmail(emailBody);
        }

        private void _SendInvalidFileEmail(string mailBody)
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
                smtpClient.Credentials = Emailer.GetSMTPNetworkCredential();
                smtpClient.Send(mailMessage);
            }
        }
    }
}
