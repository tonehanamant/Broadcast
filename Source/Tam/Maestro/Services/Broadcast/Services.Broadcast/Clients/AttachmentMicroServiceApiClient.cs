using Services.Broadcast.Entities;
using Services.Broadcast.Helpers;
using Services.Broadcast.Helpers.Json;
using System;
using System.Net.Http;
using System.Text;

namespace Services.Broadcast.Clients
{
    /// <summary>
    /// A client for retrieving the data.
    /// </summary>
    public interface IAttachmentMicroServiceApiClient
    {
       RegisterResponseDto RegisterAttachment(string sourceFileName, string username, string fileMetadata = "");
       BaseResponseDto StoreAttachment(Guid attachmentId, string fileName, byte[] fileContent);
       RetrieveResponseDto RetrieveAttachment(Guid attachmentId);
       BaseResponseDto DeleteAttachment(Guid attachmentId);
    }

    /// <summary>
    /// A client for retrieving the data.
    /// </summary>
    public class AttachmentMicroServiceApiClient : CadentSecuredClientBase, IAttachmentMicroServiceApiClient
    {
        private const string _CoreApiVersion = "api/v1/attachment";
        private readonly Lazy<string> _AttachmentMicroServiceApiUrl;
        private readonly HttpClient _HttpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentMicroServiceApiClient"/> class.
        /// </summary>
        public AttachmentMicroServiceApiClient(HttpClient httpClient,
            IApiTokenManager apiTokenManager, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper)
                : base(apiTokenManager, featureToggleHelper, configurationSettingsHelper)
        {
            _AttachmentMicroServiceApiUrl = new Lazy<string>(() => $"{_GetAttachmentMicroServiceApiBaseUrl()}");
             _HttpClient = httpClient;
        }

        public RegisterResponseDto RegisterAttachment(string sourceFileName, string username, string fileMetadata = "")
        {
            try
            {
                const string operation = "register";
                var url = $"{_AttachmentMicroServiceApiUrl.Value}{_CoreApiVersion}/{operation}";

                var request = new RegisterRequestDto
                {
                    SourceFileName = sourceFileName,
                    UploadingUserId = username,
                    AllowAccessAnyApp = true,
                    FileMetadata = fileMetadata
                };
                var httpRequestMessage = _GetRequestMessage(url, request);
                var serviceResponse = _HttpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();
                var result = serviceResponse.Content.ReadAsAsync<RegisterResponseDto>().Result;
                var registerResponse = new RegisterResponseDto
                {
                    AttachmentId = result.AttachmentId,
                    Success = result.Success,
                    Severity = result.Severity,
                    Message = result.Message,
                    TransactionId = result.TransactionId
                };
                return registerResponse;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while Register Attachement, message:{0}", ex.Message.ToString()));
            }
        }
        public BaseResponseDto StoreAttachment(Guid attachmentId, string fileName, byte[] fileContent)
        {
            try
            {
                const string operation = "store";
                var url = $"{_AttachmentMicroServiceApiUrl.Value}{_CoreApiVersion}/{operation}";
                var httpRequestMessage = _GetRequestMessage(url, attachmentId);
                httpRequestMessage.Headers.Add("attachment_id", Convert.ToString(attachmentId));
                var byteArrayContent = new ByteArrayContent(fileContent);
                var multipartContent = new MultipartFormDataContent();
                multipartContent.Add(byteArrayContent, "attachment", fileName);
                httpRequestMessage.Content = multipartContent;
                var serviceResponse = _HttpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();
                var result = serviceResponse.Content.ReadAsAsync<BaseResponseDto>().Result;
                var storeResponse = new BaseResponseDto
                {
                    success = result.success,
                    message = result.message
                };
                return storeResponse;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while Store Attachement, message:{0}", ex.Message.ToString()));
            }
        }
        public RetrieveResponseDto RetrieveAttachment(Guid attachmentId)
        {
            try
            {
                const string operation = "retrieve";
                var url = $"{_AttachmentMicroServiceApiUrl.Value}{_CoreApiVersion}/{operation}";
                var httpRequestMessage = _GetRequestMessage(url, attachmentId);
                httpRequestMessage.Headers.Add("attachment_id", Convert.ToString(attachmentId));
                var serviceResponse = _HttpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();
                var result = serviceResponse.Content.ReadAsAsync<RetrieveResponseDto>().Result;
                var retrieveResponse = new RetrieveResponseDto
                {
                    success = result.success,
                    message = result.message,
                    result = result.result
                };

                return retrieveResponse;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while Retrieve Attachement, message:{0}", ex.Message.ToString()));
            }
        }

        public BaseResponseDto DeleteAttachment(Guid attachmentId)
        {
            try
            {
                const string operation = "delete";
                var url = $"{_AttachmentMicroServiceApiUrl.Value}{_CoreApiVersion}/{operation}";
                var httpRequestMessage = _GetRequestMessage(url, attachmentId);
                httpRequestMessage.Headers.Add("attachment_id", Convert.ToString(attachmentId));
                var serviceResponse = _HttpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();
                var result = serviceResponse.Content.ReadAsAsync<BaseResponseDto>().Result;
                var deleteResponse = new BaseResponseDto
                {
                    success = result.success,
                    message = result.message,
                };
                return deleteResponse;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(String.Format("Error occured while Delete Attachement, message:{0}", ex.Message.ToString()));
            }
        }

        private string _GetAttachmentMicroServiceApiBaseUrl()
        {
            var apiBaseUrl = _ConfigurationSettingsHelper.GetConfigValue<string>(AttachmentMicroServiceApiConfigKeys.ApiBaseUrl);
            return apiBaseUrl;
        }
        private string _GetAttachmentMicroServiceApiApplicationId()
        {
            var applicationId = _ConfigurationSettingsHelper.GetConfigValue<string>(AttachmentMicroServiceApiConfigKeys.ApplicationId);
            return applicationId;
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
        private HttpRequestMessage _GetRequestMessage(string url)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequestMessage.Headers.Add("application_id", _GetAttachmentMicroServiceApiApplicationId());
            return httpRequestMessage;
        }
    }
}