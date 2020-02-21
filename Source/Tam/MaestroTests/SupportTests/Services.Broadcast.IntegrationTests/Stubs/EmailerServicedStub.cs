using System.Collections.Generic;
using System.Net.Mail;
using Common.Services;


namespace Services.Broadcast.IntegrationTests
{
    public class EmailerServiceStub : IEmailerService
    {

        public static MailMessage LastMailMessageGenerated { get; set; }

        public static void ClearLastMessage()
        {
            if (LastMailMessageGenerated != null)
            {
                LastMailMessageGenerated.Dispose();
                LastMailMessageGenerated = null;
            }
        }

        public bool QuickSend(bool pIsHtmlBody, string pBody, string pSubject, MailPriority pPriority,
            string[] pTos, List<string> attachFileNames = null)
        {
            List<MailAddress> lTos = new List<MailAddress>();
            foreach (string lTo in pTos)
                lTos.Add(new MailAddress(lTo));

            return QuickSend(pIsHtmlBody, pBody, pSubject, pPriority, lTos, attachFileNames);
        }

        public bool QuickSend(bool pIsHtmlBody, string pBody, string pSubject, MailPriority pPriority, 
            List<MailAddress> pTos, List<string> attachFileNames = null)
        {
            var configuredFrom = new MailAddress("broadcastsmtp@crossmw.com");
            LastMailMessageGenerated = EmailerService.BuildEmailMessage(pIsHtmlBody, pBody, pSubject, pPriority, configuredFrom,
                pTos, attachFileNames);

            return true;
        }
    }

}