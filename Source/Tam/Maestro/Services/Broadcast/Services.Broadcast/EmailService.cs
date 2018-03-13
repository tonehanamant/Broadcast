using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Common.Services.Extensions;
using Services.Broadcast;
using Services.Broadcast.Services;
using Tam.Maestro.Common;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Services.Cable.SystemComponentParameters;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces;

public static class Emailer
{
    public static bool QuickSend(bool pIsHtmlBody, string pBody, string pSubject,
        MailPriority pPriority, string from,string[] pTos, List<string> attachFileNames = null)
    {
        List<MailAddress> lTos = new List<MailAddress>();
        foreach (string lTo in pTos)
            lTos.Add(new MailAddress(lTo));
        var fm = new MailAddress(from);

        return QuickSend(pIsHtmlBody, pBody, pSubject, pPriority, fm,lTos,attachFileNames);
    }

    public static bool QuickSend(bool pIsHtmlBody, string pBody, string pSubject,
        MailPriority pPriority, MailAddress from, List<MailAddress> pTos,List<string> attachFileNames = null)
    {
        try
        {
            if (pTos == null || pTos.Count == 0)
                return false;

            SmtpClient lSmtpClient = new SmtpClient();
            lSmtpClient.Host = BroadcastServiceSystemParameter.EmailHost;
            lSmtpClient.EnableSsl = true;
            lSmtpClient.Port = 587;
            lSmtpClient.Credentials = GetNetworkCredential();

            MailMessage lMessage = new MailMessage();
            lMessage.IsBodyHtml = pIsHtmlBody;
            lMessage.Body = pBody;
            lMessage.From = from;
            lMessage.Priority = pPriority;
            lMessage.Subject = pSubject;
            lMessage.To.Clear();
            if (attachFileNames != null)
                foreach (var fileName in attachFileNames)
                    lMessage.Attachments.Add(new Attachment(fileName));

            if (pTos == null || pTos.Count == 0)
                throw new InvalidOperationException("Must contain at least one email fot Tos parameter");

            foreach (MailAddress lMailAddress in pTos)
                lMessage.To.Add(lMailAddress);

            if (SMSClient.Handler.TamEnvironment == TAMEnvironment.PROD)
            {
                lSmtpClient.Send(lMessage);
            }
            else
            {
                var whiteList = BroadcastServiceSystemParameter.EmailWhiteList;
                if (!string.IsNullOrEmpty(whiteList))
                {
                    lMessage.Subject = string.Format("To:{0};From:{1};Subject:{2}", lMessage.To, lMessage.From,
                        lMessage.Subject);

                    lMessage.To.Clear();
                    string[] lTos = whiteList.Split(new char[] {';'});
                    foreach (string lTo in lTos)
                        lMessage.To.Add(new MailAddress(lTo));
                    lSmtpClient.Send(lMessage);
                }
            }

            return true;
        }
        catch (System.Exception exc)
        {
            ServiceBase.LogServiceError(ServiceBase.GetServiceNameStaticPlaceholder(), exc.Message, exc);
            return false;
        }
    }

    private static NetworkCredential GetNetworkCredential()
    {
        var pwd = BroadcastServiceSystemParameter.EmailPassword;
        var usr = BroadcastServiceSystemParameter.EmailUsername;

        if (!string.IsNullOrEmpty(pwd))
            pwd = EncryptionHelper.DecryptString(pwd, BroadcastConstants.EMAIL_PROFILE_SEED);

        return new NetworkCredential(usr, pwd);
    }

}