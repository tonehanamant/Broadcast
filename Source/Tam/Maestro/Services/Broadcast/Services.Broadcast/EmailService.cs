using Common.Services.ApplicationServices;
using ConfigurationService.Client;
using Services.Broadcast;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.ServiceModel;
using Tam.Maestro.Common;
using Tam.Maestro.Common.Utilities.Logging;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Common.Services
{
    public interface IEmailerService : IApplicationService
    {
        bool QuickSend(bool pIsHtmlBody, string pBody, string pSubject, MailPriority pPriority, string[] pTos, List<string> attachFileNames = null);
        bool QuickSend(bool pIsHtmlBody, string pBody, string pSubject, MailPriority pPriority, List<MailAddress> pTos, List<string> attachFileNames = null);
    }


    public class EmailerService : IEmailerService
    {
        private IConfigurationWebApiClient _configurationWebApiClient;
        public EmailerService(IConfigurationWebApiClient configurationWebApiClient)
        {
            _configurationWebApiClient = configurationWebApiClient;
        }

        public bool QuickSend(bool pIsHtmlBody, string pBody, string pSubject, MailPriority pPriority, string[] pTos, List<string> attachFileNames = null)
        {
            List<MailAddress> lTos = new List<MailAddress>();
            foreach (string lTo in pTos)
            {
                lTos.Add(new MailAddress(lTo));
            }

            return QuickSend(pIsHtmlBody, pBody, pSubject, pPriority, lTos, attachFileNames);
        }

        public bool QuickSend(bool pIsHtmlBody, string pBody, string pSubject, MailPriority pPriority, List<MailAddress> pTos, List<string> attachFileNames = null)
        {
            if (!BroadcastServiceSystemParameter.EmailNotificationsEnabled)
                return false;

            LogHelper.Log.ServiceEvent("Broadcast EmailerService", ",sg test", "user test",
                _configurationWebApiClient.TAMEnvironment.ToString());

            try
            {
                if (pTos == null || pTos.Count == 0)
                    return false;

                var fromEmail = _GetFromEmail();

                using (SmtpClient lSmtpClient = new SmtpClient())
                {
                    lSmtpClient.Host = BroadcastServiceSystemParameter.EmailHost;
                    lSmtpClient.EnableSsl = true;
                    lSmtpClient.Port = 587;
                    lSmtpClient.Credentials = GetSMTPNetworkCredential();

                    var lMessage = BuildEmailMessage(pIsHtmlBody, pBody, pSubject, pPriority, fromEmail, pTos, attachFileNames);

                    var whiteList = BroadcastServiceSystemParameter.EmailWhiteList;
                    if (string.IsNullOrEmpty(whiteList))
                    {
                        lSmtpClient.Send(lMessage);
                    }
                    else
                    {
                        lMessage.Subject = string.Format("To:{0};From:{1};Subject:{2}", lMessage.To, lMessage.From,
                            lMessage.Subject);

                        lMessage.To.Clear();
                        string[] lTos = whiteList.Split(new char[] { ';' });
                        foreach (string lTo in lTos)
                            lMessage.To.Add(new MailAddress(lTo));

                        lSmtpClient.Send(lMessage);
                    }
                }
                return true;
            }
            catch (System.Exception exc)
            {
                LogHelper.Log.ServiceError("Broadcast EmailerService", exc.Message, exc.ToString(),
                    GetWindowsUserName(), _configurationWebApiClient.TAMEnvironment.ToString());
                throw;
            }
        }

        public static MailMessage BuildEmailMessage(bool pIsHtmlBody,
                                                        string pBody,
                                                        string pSubject,
                                                        MailPriority pPriority,
                                                        MailAddress @from,
                                                        List<MailAddress> pTos,
                                                        List<string> attachFileNames)
        {
            MailMessage lMessage = new MailMessage();
            lMessage.IsBodyHtml = pIsHtmlBody;
            lMessage.Body = pBody;
            lMessage.From = @from;
            lMessage.Priority = pPriority;
            lMessage.Subject = pSubject;
            lMessage.To.Clear();
            if (attachFileNames != null)
                foreach (var fileName in attachFileNames)
                    lMessage.Attachments.Add(new Attachment(fileName));

            if (pTos == null || pTos.Count == 0)
                throw new InvalidOperationException("Must contain at least one email for Tos parameter");

            foreach (MailAddress lMailAddress in pTos)
                lMessage.To.Add(lMailAddress);
            return lMessage;
        }

        private string GetWindowsUserName()
        {
            var userName = String.Empty;
            if (ServiceSecurityContext.Current != null)
            {
                userName = ServiceSecurityContext.Current.WindowsIdentity.Name;
            }

            return userName;
        }

        private MailAddress _GetFromEmail()
        {
            // office365 requires the 'from' email to be for the credentialed account.
            // in office365 the username is the same as the 'from' email.
            var account = BroadcastServiceSystemParameter.EmailUsername;
            return new MailAddress(account);
        }

        public static NetworkCredential GetSMTPNetworkCredential()
        {
            var pwd = BroadcastServiceSystemParameter.EmailPassword;
            var usr = BroadcastServiceSystemParameter.EmailUsername;

            if (!string.IsNullOrEmpty(pwd))
                pwd = EncryptionHelper.DecryptString(pwd, BroadcastConstants.EMAIL_PROFILE_SEED).Replace("\n", "\\n");

            return new NetworkCredential(usr, pwd);
        }

    }
}