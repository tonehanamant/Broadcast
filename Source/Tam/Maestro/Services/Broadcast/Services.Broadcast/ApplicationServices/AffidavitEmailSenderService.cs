using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.BusinessEngines
{
    public interface IAffidavitEmailSenderService
    {
        void Send(string emailBody,string subject = null);
    }

    public class AffidavitEmailSenderService : IAffidavitEmailSenderService
    {
        private const string _EmailValidationSubject = "WWTV File Failed Validation";
        private const int _SmtpPort = 587;

        public void Send(string emailBody, string subject = null)
        {
            if (!BroadcastServiceSystemParameter.EmailNotificationsEnabled)
                return;

            if (string.IsNullOrEmpty(subject))
                subject = _EmailValidationSubject;

            var from = new MailAddress(BroadcastServiceSystemParameter.EmailUsername);
            var to = new List<MailAddress>() {new MailAddress(BroadcastServiceSystemParameter.WWTV_NotificationEmail)};
            Emailer.QuickSend(false,emailBody, _EmailValidationSubject,MailPriority.Normal,from ,to);
        }
    }
}
