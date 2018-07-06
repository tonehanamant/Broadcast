using System.Collections.Generic;
using System.Net.Mail;
using Common.Services;


namespace Services.Broadcast.IntegrationTests
{
    public class EmailerServiceStubb : IEmailerService
    {

        public static MailMessage LastMailMessageGenerated { get; private set; }

        public static void ClearLastMessage()
        {
            LastMailMessageGenerated.Dispose(); 
            LastMailMessageGenerated = null;
        }

        public bool QuickSend(bool pIsHtmlBody, string pBody, string pSubject, MailPriority pPriority, string from,
            string[] pTos, List<string> attachFileNames = null)
        {
            List<MailAddress> lTos = new List<MailAddress>();
            foreach (string lTo in pTos)
                lTos.Add(new MailAddress(lTo));
            var fm = new MailAddress(from);

            return QuickSend(pIsHtmlBody, pBody, pSubject, pPriority, fm, lTos, attachFileNames);
        }

        public bool QuickSend(bool pIsHtmlBody, string pBody, string pSubject, MailPriority pPriority, MailAddress from,
            List<MailAddress> pTos, List<string> attachFileNames = null)
        {
            LastMailMessageGenerated = EmailerService.BuildEmailMessage(pIsHtmlBody, pBody, pSubject, pPriority, @from,
                pTos, attachFileNames);

            return true;
        }
    }

}