using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Common.Services.ApplicationServices;
using Services.Broadcast.Clients;
using Services.Broadcast.Helpers;
using System;
using System.IO;
using Tam.Maestro.Common;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class LogToAmazonS3Stub : ILogToAmazonS3
    {
        /// <summary>
        /// Uploads a file to S3.
        /// </summary>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="data">The data.</param>
        public async void UploadFile(string bucketName, string keyName, byte[] data)
        {
        }

        /// <summary>
        /// Downloads File from an S3.
        /// </summary>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public GetObjectResponse DownloadFile(string bucketName, string keyName, string fileName)
        {
            var response = new GetObjectResponse();

            return response;
        }

        /// <summary>
        /// Validates the parameters.
        /// </summary>
        /// <param name="bucketName">Name of the bucket.</param>
        private void _ValidateParameters(string bucketName)
        {
        }
    }
}
