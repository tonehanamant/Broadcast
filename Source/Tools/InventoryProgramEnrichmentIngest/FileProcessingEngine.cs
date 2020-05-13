using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace InventoryProgramEnrichmentIngest
{
    public interface IFileProcessingEngine
    {
        void BringFilesLocal(string sourceDirectoryPath, string targetDirectoryPath, string fileSearchPattern);

        void CleanupLocal(string localDirectoryPath);

        List<string> GetFilesToProcess(string directoryPath, string fileSearchPattern);

        void ProcessFile(string filePath, string targetUrl, int timeoutMinutes);
    }

    public class FileProcessingEngine : ProcessingBaseClass, IFileProcessingEngine
    {
        public void BringFilesLocal(string sourceDirectoryPath, string targetDirectoryPath, string fileSearchPattern)
        {
            _LogInfo("Starting to bring the files local.");
            _LogInfo($"SourceDirectoryPath = '{sourceDirectoryPath}'");
            _LogInfo($"TargetDirectoryPath = '{targetDirectoryPath}'");

            if (Directory.Exists(sourceDirectoryPath) == false)
            {
                throw new DirectoryNotFoundException(sourceDirectoryPath);
            }

            if (Directory.Exists(targetDirectoryPath) == false)
            {
                _LogInfo($"Creating directory '{targetDirectoryPath}'");
                Directory.CreateDirectory(targetDirectoryPath);
            }

            var files = Directory.GetFiles(sourceDirectoryPath, fileSearchPattern);
            var fileCount = files.Length;
            _LogInfo($"{files.Length} files found to process.");
            var fileNumber = 0;

            var fileCopyStopWatch = new Stopwatch();
            fileCopyStopWatch.Start();
            try
            {
                foreach (var file in files)
                {
                    fileNumber++;
                    _LogInfo($"Beginning to copy file {fileNumber} of {fileCount}");

                    var fileName = Path.GetFileName(file);
                    _LogInfo($"FileName = '{fileName}'");

                    var destinationFileName = Path.Combine(targetDirectoryPath, fileName);
                    _LogInfo($"DestinationFileName = '{fileName}'");
                    File.Copy(file, destinationFileName);
                    _LogInfo("Copy complete.");
                }
            }
            finally
            {
                fileCopyStopWatch.Stop();
            }
            
            _LogInfo($"Completed copying {fileCount} files.  Duration = {fileCopyStopWatch.ElapsedMilliseconds}ms");
        }

        public void CleanupLocal(string localDirectoryPath)
        {
            var cleanupStopWatch = new Stopwatch();
            cleanupStopWatch.Start();
            _LogInfo($"Beginning to cleanup the local directory '{localDirectoryPath}'");
            try
            {
                if (Directory.Exists(localDirectoryPath))
                {
                    Directory.Delete(localDirectoryPath, true);
                }
                Directory.CreateDirectory(localDirectoryPath);
            }
            finally
            {
                cleanupStopWatch.Stop();
            }
            _LogInfo($"Completed cleanup of local directory. Duration = '{cleanupStopWatch.ElapsedMilliseconds}'ms");
        }

        public List<string> GetFilesToProcess(string directoryPath, string fileSearchPattern)
        {
            var files = Directory.GetFiles(directoryPath, fileSearchPattern);
            return files.ToList();
        }

        public void ProcessFile(string filePath, string targetUrl, int timeoutMinutes)
        {
            _LogInfo("");
            _LogInfo("*** File Processing Start ***");
            _LogInfo($"Beginning to process file '{filePath}'");
            _LogInfo($"TargetUrl = '{targetUrl}'");

            var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            var processingStopWatch = new Stopwatch();
            processingStopWatch.Start();

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, timeoutMinutes, 0);
                    var serviceResponse = client.PostAsync(targetUrl, form).Result;
                    if (serviceResponse.IsSuccessStatusCode == false)
                    {
                        throw new Exception(
                            $"Error connecting to BroadcastApp to process the enriched file : {serviceResponse}");
                    }
                    // the status is good enough.

                    /*
                        // Example of using Nuget HtmlAgilityPack (v1.11.23) to get the file processing result message.

                        var returnedPage = new HtmlDocument();
                        returnedPage.LoadHtml(response);
                        // these are the coordinates to the text area containing the response 
                        var contentNode = returnedPage.DocumentNode.ChildNodes[3].ChildNodes[3].ChildNodes[3].ChildNodes[3]
                            .ChildNodes[1].ChildNodes[5].ChildNodes[11].ChildNodes[1].ChildNodes[7];
                        var fileProcessingResultMessage = contentNode.InnerText;
                    */
                }
            }
            finally
            {
                processingStopWatch.Stop();
            }
            
            _LogInfo($"Processing completed. Duration = {processingStopWatch.ElapsedMilliseconds}ms");
            _LogInfo("*** File Processing End ***");
        }
    }
}