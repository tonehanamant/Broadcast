using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Services.Broadcast.Helpers.Json;

namespace AttachmentMicroServiceApiTester
{

    public class ListRequest
    {
        public List<string> applicationIds { get; set; } = new List<string>();
    }

    public class ListResponse : BaseResponse
    {
        public List<AttachmentDetails> resultList { get; set; } = new List<AttachmentDetails>();

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("******************");

            sb.AppendLine($"success = {success}");
            sb.AppendLine($"message = {message}");
            sb.AppendLine($"count = {resultList?.Count ?? 0}");

            foreach (var result in resultList)
            {
                sb.AppendLine(result.ToString());
            }

            sb.AppendLine("******************");

            return sb.ToString();
        }
    }

    public class AttachmentDetails
    {
        public string attachmentId { get; set; }
        public string originalFileName { get; set; }
        public string applicationId { get; set; }
        public bool allowAnyAppAccess { get; set; }
        public string uploadUser { get; set; }
        public DateTime createDate { get; set; }
        public string attachmentMetadata { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("******************");

            sb.AppendLine($"attachmentId = {attachmentId}");
            sb.AppendLine($"originalFileName = {originalFileName}");
            sb.AppendLine($"applicationId = {applicationId}");
            sb.AppendLine($"allowAnyAppAccess = {allowAnyAppAccess}");
            sb.AppendLine($"uploadUser = {uploadUser}");
            sb.AppendLine($"createDate = {createDate}");
            sb.AppendLine($"attachmentMetadata = {attachmentMetadata}");

            sb.AppendLine("******************");

            return sb.ToString();
        }
    }

    public class RegisterRequest
    {
        public string SourceFileName { get; set; }
        public string UploadingUserId { get; set; }
        public bool AllowAccessAnyApp { get; set; }
        public string FileMetadata{ get; set; }
    }

    public class RegisterResponse : BaseResponse
    {
        public string result { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("******************");

            sb.AppendLine($"success = {success}");
            sb.AppendLine($"message = {message}");
            sb.AppendLine($"result = {result}");

            sb.AppendLine("******************");

            return sb.ToString();
        }
    }

    public class BaseResponse
    {
        public bool success { get; set; }
        public string message { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("******************");

            sb.AppendLine($"success = {success}");
            sb.AppendLine($"message = {message}");

            sb.AppendLine("******************");

            return sb.ToString();
        }
    }

    public class RetrieveResponse : BaseResponse
    {
        public byte[] result { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("******************");

            sb.AppendLine($"success = {success}");
            sb.AppendLine($"message = {message}");
            sb.AppendLine($"result = {result}");

            sb.AppendLine("******************");

            return sb.ToString();
        }
    }


    public class AttachmentServiceApiClient
    {
        protected const int ASYNC_API_TIMEOUT_MILLISECONDS = 900000;
        private readonly HttpClient _HttpClient;

        private string _ApplicationId = "BD9C9B56-1F78-4C47-B99D-67E0340C7232";
        private string _UrlBase = "http://cpa-dev-cd1.dev.cadent.tv/attachmentmicroservice/api/v1/attachment";
        

        public AttachmentServiceApiClient(HttpClient httpClient)
        {
            _HttpClient = httpClient;
        }

        private T _SendData<T>(HttpRequestMessage httpRequestMessage)
        {
            var serviceResponse = _HttpClient.SendAsync(httpRequestMessage).Result;
            var result = serviceResponse.Content.ReadAsAsync<T>().Result;
            return result;
        }

        private string _SendDataGetRaw(HttpRequestMessage httpRequestMessage)
        {
            var serviceResponse = _HttpClient.SendAsync(httpRequestMessage).Result;
            var result = serviceResponse.Content.ReadAsStringAsync().Result;
            return result;
        }

        private HttpRequestMessage _GetRequestMessage(string url)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequestMessage.Headers.Add("application_id", _ApplicationId);
            return httpRequestMessage;
        }

        private HttpRequestMessage _GetRequestMessage(string url, object data)
        {
            var httpRequestMessage = _GetRequestMessage(url);

            if (data != null)
            {
                var bodyJson = JsonSerializerHelper.ConvertToJson(data);
                httpRequestMessage.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
            }

            return httpRequestMessage;
        }

        private HttpRequestMessage _GetRequestMessage(string url, string attachmentId, object data)
        {
            var httpRequestMessage = _GetRequestMessage(url, data);
            httpRequestMessage.Headers.Add("attachment_id", attachmentId);

            return httpRequestMessage;
        }

        private HttpRequestMessage _GetRequestMessage(string url, string attachmentId)
        {
            var httpRequestMessage = _GetRequestMessage(url);
            httpRequestMessage.Headers.Add("attachment_id", attachmentId);

            return httpRequestMessage;
        }

        public async Task<ListResponse> ListAttachments()
        {
            const string operation = "list";
            var url = $"{_UrlBase}/{operation}";

            var request = new ListRequest();
            request.applicationIds.Add(_ApplicationId);

            var httpRequestMessage = _GetRequestMessage(url, request);

            var result = _SendData<ListResponse>(httpRequestMessage);

            return result;
        }

        public async Task<RegisterResponse> RegisterAttachment(string sourceFileName, string username, string fileMetadata = "")
        {
            const string operation = "register";
            var url = $"{_UrlBase}/{operation}";
            
            var request = new RegisterRequest
            {
                SourceFileName = sourceFileName,
                UploadingUserId = username,
                AllowAccessAnyApp = false,
                FileMetadata = fileMetadata
            };

            var httpRequestMessage = _GetRequestMessage(url, request);
            var result = _SendData<RegisterResponse>(httpRequestMessage);

            return result;
        }

        public async Task<BaseResponse> DeleteAttachment(string attachmentId)
        {
            const string operation = "delete";
            var url = $"{_UrlBase}/{operation}";

            var httpRequestMessage = _GetRequestMessage(url, attachmentId);

            var result = _SendData<BaseResponse>(httpRequestMessage);

            return result;
        }

        public async Task<BaseResponse> UploadAttachment(string attachmentId, string fileName, byte[] fileContent)
        {
            const string operation = "store";
            var url = $"{_UrlBase}/{operation}";

            var httpRequestMessage = _GetRequestMessage(url, attachmentId);

            var byteArrayContent = new ByteArrayContent(fileContent);
            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(byteArrayContent, "attachment", fileName);
            httpRequestMessage.Content = multipartContent;

            var result = _SendData<BaseResponse>(httpRequestMessage);

            return result;
        }

        public async Task<RetrieveResponse> RetrieveAttachment(string attachmentId)
        {
            const string operation = "retrieve";
            var url = $"{_UrlBase}/{operation}";

            var httpRequestMessage = _GetRequestMessage(url, attachmentId);

            var result = _SendData<RetrieveResponse>(httpRequestMessage);

            return result;
        }
    }
}