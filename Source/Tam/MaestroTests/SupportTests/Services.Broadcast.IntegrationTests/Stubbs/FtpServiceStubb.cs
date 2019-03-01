using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Common.Services;
using Tam.Maestro.Services.ContractInterfaces;


namespace Services.Broadcast.IntegrationTests
{
    public class FtpServiceStubb_Empty : IFtpService
    {
        public virtual NetworkCredential GetCredentials(string ftpUserName, string ftpPassword)
        {
            return new NetworkCredential(ftpUserName, ftpPassword);
        }

        public virtual void UploadFile(NetworkCredential credentials, string sourceFilePath, string destPath)
        {
        }

        public virtual List<string> GetFileList(NetworkCredential credentials, string remoteFTPPath)
        {
            return new List<string>();
        }

        public virtual void DeleteFile(NetworkCredential credentials, string remoteFTPPath)
        {
        }


        public virtual void DownloadFile(WebClient webClient, string path, string localPath)
        {
        }

        public virtual Stream DownloadFileToStream(WebClient webClient, string path)
        {
            return new MemoryStream();
        }

    }

    public class FtpServiceStubb_SingleFile : FtpServiceStubb_Empty
    {
        public override List<string> GetFileList(NetworkCredential credentials, string remoteFTPPath)
        {
            return new List<string>()
            {
                "Special_Ftp_Phantom_File.txt"
            };
        }

        protected virtual string GetFileContents()
        {
            return "Sample Content for Special_Ftp_Phantom_File.txt";
        }

        public override void DownloadFile(WebClient webClient, string path, string localPath)
        {
            if (!File.Exists(localPath))
            {
                using (var file = File.Create(localPath))
                {
                    var byteArray = GetFileContents().ToByteArray();
                    file.Write(byteArray, 0, byteArray.Length);
                    _FilesCreated.Add(localPath);
                }
            }
        }

        public override Stream DownloadFileToStream(WebClient webClient, string path)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(GetFileContents() ?? ""));
        }

        private readonly List<string> _FilesCreated = new List<string>();
        public void CleanUpCreatedFiles()
        {
            _FilesCreated.ForEach(f =>
            {
                if (File.Exists(f))
                {
                    File.Delete(f);
                }
            });
            _FilesCreated.Clear();
        }
    }



    #region some stubbs for specific tests
    public class DownloadAndProcessWWTVFiles_Validation_Errors_Stubb : FtpServiceStubb_SingleFile
    {
        protected override string GetFileContents()
        {
            return
                @"
 [ {
    ""EstimateId"": 3832,
    ""Market"": ""Boston"",
    ""Date"": ""11/01/2017"",
    ""InventorySource"": ""Strata"",
    ""Station"": ""WBTS-TV"",
    ""SpotLength"": 30,
    ""Time"": ""0543A"",
    ""SpotCost"": 0,
    ""ISCI"": ""32YO41TC18H"",
    ""Affiliate"": null,
    ""Program"": ""NBC Boston Today at 05:30 AM"",
    ""ShowType"": ""News"",
    ""Genre"": ""News"",
    ""LeadInProgram"": ""NBC Boston Today at 04:30 AM"",
    ""LeadInShowType"": ""News"",
    ""LeadInGenre"": ""News"",
    ""LeadInEndTime"": ""11/01/2017 05:00 AM"",
    ""LeadOutProgram"": ""NBC Boston Today at 06:00 AM"",
    ""LeadOutShowType"": ""News"",
    ""LeadOutGenre"": ""News"",
    ""LeadOutStartTime"": ""11/01/2017 06:00 AM"",
    ""Demographics"": null
  }]";
        }
    }


    public class DownloadAndProcessWWTVFiles_ValidFile_Stubb : FtpServiceStubb_SingleFile
    {
        protected override string GetFileContents()
        {
            return
                @"
 [ {
    ""EstimateId"": 3832,
    ""Market"": ""Boston"",
    ""Date"": ""11/01/2017"",
    ""InventorySource"": ""Strata"",
    ""Station"": ""WBTS-TV"",
    ""SpotLength"": 30,
    ""Time"": ""0543A"",
    ""SpotCost"": 0,
    ""ISCI"": ""32YO41TC18H"",
    ""Affiliate"": ""Affiliate"",
    ""Program"": ""NBC Boston Today at 05:30 AM"",
    ""ShowType"": ""News"",
    ""Genre"": ""News"",
    ""LeadInProgram"": ""NBC Boston Today at 04:30 AM"",
    ""LeadInShowType"": ""News"",
    ""LeadInGenre"": ""News"",
    ""LeadInEndTime"": ""11/01/2017 05:00 AM"",
    ""LeadOutProgram"": ""NBC Boston Today at 06:00 AM"",
    ""LeadOutShowType"": ""News"",
    ""LeadOutGenre"": ""News"",
    ""LeadOutStartTime"": ""11/01/2017 06:00 AM"",
    ""Demographics"": null
  }]";
        }
    }

    public class FtpServiceStubb_KeepingTrac : FtpServiceStubb_SingleFile
    {
        public string KeepingTracFile = "KeepingTracTest.xlsx.txt";
        public override List<string> GetFileList(NetworkCredential credentials, string remoteFTPPath)
        {
            return new List<string>()
            {
                KeepingTracFile
            };
        }
        protected override string GetFileContents()
        {
            return File.ReadAllText(".\\Files\\" + KeepingTracFile);
        }

    }

    public class FtpServiceStubb_KeepingTrac_BadTime : FtpServiceStubb_SingleFile
    {
        public string KeepingTracFile = "KeepingTracTest.xlsx.BadTimes.txt";
        public override List<string> GetFileList(NetworkCredential credentials, string remoteFTPPath)
        {
            return new List<string>()
            {
                KeepingTracFile
            };
        }
        protected override string GetFileContents()
        {
            return File.ReadAllText(".\\Files\\" + KeepingTracFile);
        }

    }
    #endregion

}