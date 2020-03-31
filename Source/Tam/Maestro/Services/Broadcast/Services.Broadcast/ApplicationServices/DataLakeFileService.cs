﻿using Common.Services;
using Common.Services.ApplicationServices;
using Services.Broadcast.ApplicationServices.Security;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace Services.Broadcast.ApplicationServices
{
    public interface IDataLakeFileService : IApplicationService
    {
        /// <summary>
        /// Saves a file to data lake folder
        /// </summary>
        /// <param name="filePath">Filepath for the file to save</param>
        void Save(string filePath);

        /// <summary>
        /// Saves a stream file to data lake folder
        /// </summary>
        /// <param name="fileRequest">FileRequest object for the file to save</param>
        void Save(FileRequest fileRequest);
    }

    public class DataLakeFileService : BroadcastBaseClass, IDataLakeFileService
    {
        private readonly IDataLakeSystemParameters _DataLakeSystemParameter;
        private readonly IEmailerService _EmailerService;
        private readonly IFileService _FileService;
        private readonly IImpersonateUser _ImpersonateUser;

        private readonly string _DataLakeFolder;
        private readonly string _DataLakeUsername;
        private readonly string _DataLakePassword;

        public DataLakeFileService(IDataLakeSystemParameters dataLakeSystemParameter
            , IEmailerService emailerService
            , IImpersonateUser impersonateUser
            , IFileService fileService)
        {
            _DataLakeSystemParameter = dataLakeSystemParameter;
            _EmailerService = emailerService;
            _ImpersonateUser = impersonateUser;
            _FileService = fileService;
            _DataLakeFolder = _DataLakeSystemParameter.GetSharedFolder();
            _DataLakeUsername = _DataLakeSystemParameter.GetUserName();
            _DataLakePassword = _DataLakeSystemParameter.GetPassword();
        }
        
        /// <summary>
        /// Saves a stream file to data lake folder
        /// </summary>
        /// <param name="fileRequest">FileRequest object for the file to save</param>
        public void Save(FileRequest fileRequest)
        {
            //This is to prevent errors in Debug/Development sessions writing to CD environment
            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine("Running in debug mode. Skipping data lake file save");
                return;
            }

            var filePath = Path.Combine(_DataLakeFolder, fileRequest.FileName);

            try
            {
                if (String.IsNullOrEmpty(_DataLakeUsername))
                {
                    _FileService.Copy(fileRequest.StreamData, filePath, true);
                }
                else
                {
                    _ImpersonateUser.Impersonate(string.Empty, _DataLakeUsername, _DataLakePassword, delegate
                    {
                            _FileService.Copy(fileRequest.StreamData, filePath, true);                        
                    });
                }
            }
            catch (Exception exception)
            {
                _SendErrorEmail(filePath, exception.Message);
            }

        }

        /// <summary>
        /// Saves a file to data lake folder
        /// </summary>
        /// <param name="filePath">Filepath for the file to save</param>
        public void Save(string filePath)
        {
            var newFilePath = Path.Combine(_DataLakeFolder, Path.GetFileName(filePath));

            _ImpersonateUser.Impersonate(string.Empty, _DataLakeUsername, _DataLakePassword, delegate
            {
                try
                {
                    _FileService.Copy(filePath, newFilePath, true);
                }
                catch (Exception exception)
                {
                    _SendErrorEmail(newFilePath, exception.Message);
                }
            });
        }

        private void _SendErrorEmail(string filePath, string errorMessage)
        {
            try
            {
                var to = new List<MailAddress>() { new MailAddress(_DataLakeSystemParameter.GetNotificationEmail()) };
                _EmailerService.QuickSend(false, _CreateErrorEmail(filePath, errorMessage), "Data Lake File Failure",
                    MailPriority.Normal, to);
            }
            catch (Exception ex)
            {
                var msg = $"Exception attempting to send email about an error communicating with the DataLake Files : File Path ='{filePath}'; ErrorMessage = '{errorMessage}';";
                _LogError(msg, ex);
            }
        }

        private string _CreateErrorEmail(string filePath, string errorMessage)
        {
            var emailBody = new StringBuilder();

            emailBody.AppendFormat("File {0} could not be properly processed.  Including technical information to help figure out the issue.", Path.GetFileName(filePath));
            emailBody.AppendFormat("\n\nFile located in {0}\n", filePath);
            emailBody.AppendFormat("\nTechnical Information:\n\n{0}", errorMessage);

            return emailBody.ToString();
        }
    }
}
