using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.BusinessEngines
{
    public interface IAffidavitEmailSenderService
    {
        void Send(string emailBody,string subject);
    }

    public class AffidavitEmailSenderService : IAffidavitEmailSenderService
    {

        public void Send(string emailBody, string subject )
        {
            if (!BroadcastServiceSystemParameter.EmailNotificationsEnabled)
                return;

            var from = new MailAddress(BroadcastServiceSystemParameter.EmailUsername);
            var to = new List<MailAddress>() {new MailAddress(BroadcastServiceSystemParameter.WWTV_NotificationEmail)};
            Emailer.QuickSend(false,emailBody, subject,MailPriority.Normal,from ,to);
        }
    }
}
