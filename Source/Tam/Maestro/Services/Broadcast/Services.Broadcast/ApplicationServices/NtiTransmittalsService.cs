using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Newtonsoft.Json;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Nti;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.SystemComponentParameters;

namespace Services.Broadcast.ApplicationServices
{
    public interface INtiTransmittalsService : IApplicationService
    {
        /// <summary>
        /// Uploads a pdf file containing nti transmittals
        /// </summary>
        /// <param name="request">FileRequest object having the file content encripted as base64</param>
        /// <param name="name">Username requesting the upload</param>
        /// <returns>BaseResponse object containing message of success or failure</returns>
        BaseResponse UploadNtiTransmittalsFile(FileRequest request, string name);

        /// <summary>
        /// Send a pdf file content to Nielsen
        /// This method is public because I am using it in the integration tests
        /// </summary>
        /// <param name="request">FileRequest object</param>
        /// <returns>BaseResponse object containing a list of NielsenRatingDocumentDto objects</returns>
        BaseResponse<List<NtiRatingDocumentDto>> SendPdfDocumentToNielson(FileRequest request);
    }

    public class NtiTransmittalsService : INtiTransmittalsService
    {
        private readonly INtiTransmittalsRepository _NtiTransmittalsRepository;

        public NtiTransmittalsService(IDataRepositoryFactory broadcastDataRepositoryFactory)
        {
            _NtiTransmittalsRepository = broadcastDataRepositoryFactory.GetDataRepository<INtiTransmittalsRepository>();
        }

        /// <summary>
        /// Uploads a pdf file containing nti transmittals
        /// </summary>
        /// <param name="request">FileRequest object having the file content encripted as base64</param>
        /// <param name="name">Username requesting the upload</param>
        /// <returns>BaseResponse object containing message of success or failure</returns>
        public BaseResponse UploadNtiTransmittalsFile(FileRequest request, string name)
        {
            BaseResponse<List<NtiRatingDocumentDto>> result = SendPdfDocumentToNielson(request);
            var ntiFile = new NtiFile
            {
                CreatedDate = DateTime.Now,
                FileName = request.FileName,
                CreatedBy = name,
                Status = result.Success ? FileProcessingStatusEnum.Valid : FileProcessingStatusEnum.Invalid
            };

            if (result.Success == false)
            {
                ntiFile.FileProblems.Add(new FileProblem { ProblemDescription = result.Message });
                _NtiTransmittalsRepository.SaveFile(ntiFile);
                return new BaseResponse { Success = false, Message = result.Message };
            }

            ntiFile.Details = _MapFileDetails(result.Data);
            _NtiTransmittalsRepository.SaveFile(ntiFile);

            return new BaseResponse { Success = true, Message = "File processed, NTI data processed" };
        }

        private List<NtiRatingDocument> _MapFileDetails(List<NtiRatingDocumentDto> data)
        {
            List<NtiRatingDocument> document = new List<NtiRatingDocument>();
            document.AddRange(data.Select(x => new NtiRatingDocument
            {
                Header = x.Header,
                Ratings = _MapNielsenRatings(x.Ratings)
            }).ToList());
            return document;
        }

        private List<NtiRatingCategory> _MapNielsenRatings(List<NtiRatingCategoryDto> ratings)
        {
            List<NtiRatingCategory> categories = new List<NtiRatingCategory>();
            foreach (var rating in ratings)
            {
                if (rating.SubCategories != null)
                {
                    categories.AddRange(rating.SubCategories.Where(x => !string.IsNullOrWhiteSpace(x.Impressions)).Select(x => new NtiRatingCategory
                    {
                        Category = rating.Category,
                        SubCategory = x.SubCategory,
                        Impressions = double.Parse(x.Impressions),
                        Percent = double.Parse(x.Percent),
                        VPVH = string.IsNullOrWhiteSpace(x.VPVH) ? (int?)null : int.Parse(x.VPVH)
                    }));
                }
                else
                {
                    categories.Add(new NtiRatingCategory
                    {
                        Category = rating.Category,
                        Impressions = double.Parse(rating.Impressions),
                        Percent = double.Parse(rating.Percent),
                        VPVH = string.IsNullOrWhiteSpace(rating.VPVH) ? (int?)null : int.Parse(rating.VPVH)
                    });
                }
            }

            return categories;
        }

        /// <summary>
        /// Send a pdf file content to Nielsen
        /// This method is public because I am using it in the integration tests
        /// </summary>
        /// <param name="request">FileRequest object</param>
        /// <returns>BaseResponse object containing a list of NielsenRatingDocumentDto objects</returns>
        public BaseResponse<List<NtiRatingDocumentDto>> SendPdfDocumentToNielson(FileRequest request)
        {
            var nielsonRequest = new NtiPdfDto { Base64String = request.RawData };
            var jsonRequest = JsonConvert.SerializeObject(nielsonRequest);
            using (var webClient = new WebClient())
            {
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                var jsonResult = webClient.UploadString(BroadcastServiceSystemParameter.BroadcastNTIUploadApiUrl, jsonRequest);
                var result = JsonConvert.DeserializeObject<BaseResponse<List<NtiRatingDocumentDto>>>(jsonResult);
                return result;
            }
        }
    }
}
