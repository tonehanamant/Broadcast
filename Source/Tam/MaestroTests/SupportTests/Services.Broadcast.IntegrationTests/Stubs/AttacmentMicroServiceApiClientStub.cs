using Services.Broadcast.Clients;
using Services.Broadcast.Entities;
using System;

namespace Services.Broadcast.IntegrationTests.Stubs
{
    public class AttacmentMicroServiceApiClientStub : IAttachmentMicroServiceApiClient
    {
        public RegisterResponseDto RegisterAttachment(string sourceFileName, string username, string fileMetadata = "")
        {
            var response = new RegisterResponseDto
            {
                AttachmentId = new Guid("33ba8c97-f723-4c14-9657-cd7b422c5b2c"),
                Success = true
            };
            return response;
        }

        public BaseResponseDto StoreAttachment(Guid attachmentId, string fileName, byte[] fileContent)
        {
            var response = new BaseResponseDto
            {
                success = true
            };
            return response;
        }
        public RetrieveResponseDto RetrieveAttachment(Guid attachmentId)
        {
            var response = new RetrieveResponseDto
            {
                success = true,
                result = new byte[0]
            };

            return response;
        }

        public BaseResponseDto DeleteAttachment(Guid attachmentId)
        {
            var response = new BaseResponseDto
            {
                success = true
            };
            return response;
        }
    }
}
