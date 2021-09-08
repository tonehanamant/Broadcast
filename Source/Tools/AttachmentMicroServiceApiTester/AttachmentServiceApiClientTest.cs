using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using Services.Broadcast;
using Services.Broadcast.Clients;
using Services.Broadcast.Entities.Plan.Pricing;
using Services.Broadcast.Helpers;

namespace AttachmentMicroServiceApiTester
{
    public class AttachmentServiceApiClientTest : ITest
    {
        public async Task<bool> Run()
        {
            var sourceFileName = "1800 Flowers EM Q3'21 Plan Rev - 09-02.xlsx";
            var username = "sespiritu";

            var client = _GetClient();

            var dataDirectory = @"C:\git\Broadcast\Source\Tools\AttachmentMicroServiceApiTester\DataFiles";
            var filePath = Path.Combine(dataDirectory, sourceFileName);

            var outputFilePath = Path.Combine(dataDirectory, "Retrieved_" + sourceFileName);

            var excelPackage = new ExcelPackage();
            using (var fileStream = File.OpenRead(filePath))
            {
                excelPackage.Load(fileStream);
            }
            var fileContent = excelPackage.GetAsByteArray();

            // List out our files at the start
            var listResult = await client.ListAttachments();
            Console.WriteLine("The list at the start : ");
            Console.WriteLine(listResult);

            // verify our file isn't there so we don't upload a dup
            Console.WriteLine($"Checking if file '{sourceFileName}' exists...");
            var existingFile = listResult.resultList.FirstOrDefault(f => f.originalFileName.Equals(sourceFileName));
            if (existingFile != null)
            {
                // clean up the existing file
                Console.WriteLine($"File exists : '{existingFile.attachmentId}'.  Deleting it...");
                var deleteResult = await client.DeleteAttachment(existingFile.attachmentId);
                Console.WriteLine($"DeleteResult : {deleteResult.message}");
            }

            // Step 1 in uploading a file : Registration
            Console.WriteLine("Registering the file.");
            var registrationResult = await client.RegisterAttachment(sourceFileName, username);
            Console.WriteLine($"RegistrationResult : {registrationResult.message}");

            var attachmentId = registrationResult.result;
            Console.WriteLine($"AttachmentId = {attachmentId}");

            // Step 2 in uploading a file : Upload
            Console.WriteLine("Uploading the file.");
            var uploadResult = await client.UploadAttachment(attachmentId, sourceFileName, fileContent);
            Console.WriteLine($"UploadResult : {uploadResult}");

            // retrieve 
            Console.WriteLine("Retrieving the file.");
            var retrieveResult = await client.RetrieveAttachment(attachmentId);
            Console.WriteLine($"retrieveResult : {retrieveResult}");

            Console.WriteLine("Saving retrieved to file...");
            File.WriteAllBytes(outputFilePath, retrieveResult.result);

            // cleanup
            listResult = await client.ListAttachments();
            Console.WriteLine("The list at the end : ");
            Console.WriteLine(listResult);

            return true;
        }

        private AttachmentServiceApiClient _GetClient()
        {
            var httpClient = new HttpClient();

            var client = new AttachmentServiceApiClient(httpClient);

            return client;
        }
    }
}