using Common.Services;
using Common.Services.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface IDataLakeFileService : IApplicationService
    {
        void Save(FileRequest fileRequest);
    }

    public class DataLakeFileService : IDataLakeFileService
    {
        private readonly IEmailerService _EmailerService;
        private readonly IImpersonateUser _ImpersonateUser;

        public DataLakeFileService(IEmailerService emailerService, IImpersonateUser impersonateUser)
        {
            _EmailerService = emailerService;
            _ImpersonateUser = impersonateUser;
        }

        public void Save(FileRequest fileRequest)
        {
            var folderToSave = BroadcastServiceSystemParameter.DataLake_SharedFolder;
            var filePath = Path.Combine(folderToSave, fileRequest.FileName);
            var userName = BroadcastServiceSystemParameter.DataLake_SharedFolder_UserName;
            var password = BroadcastServiceSystemParameter.DataLake_SharedFolder_Password;

            _ImpersonateUser.Impersonate(string.Empty, userName, password, delegate
            {
                try
                {
                    CopyFileToFolder(fileRequest, filePath);
                }
                catch(Exception exception)
                {
                    SendErrorEmail(filePath, exception.Message);
                }
            });
        }

        private void SendErrorEmail(string filePath, string errorMessage)
        {
            var from = new MailAddress(BroadcastServiceSystemParameter.EmailFrom);
            var to = new List<MailAddress>() { new MailAddress(BroadcastServiceSystemParameter.DataLake_NotificationEmail) };

            _EmailerService.QuickSend(false, CreateErrorEmail(filePath, errorMessage), "Data Lake File Failure", MailPriority.Normal, from, to);
        }

        private string CreateErrorEmail(string filePath, string errorMessage)
        {
            var emailBody = new StringBuilder();

            emailBody.AppendFormat("File {0} could not be properly processed.  Including technical information to help figure out the issue.", Path.GetFileName(filePath));
            emailBody.AppendFormat("\n\nFile located in {0}\n", filePath);
            emailBody.AppendFormat("\nTechnical Information:\n\n{0}", errorMessage);

            return emailBody.ToString();
        }

        private void CopyFileToFolder(FileRequest fileRequest, string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                fileRequest.StreamData.CopyTo(fileStream);
            }
        }
    }
}
