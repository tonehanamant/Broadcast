using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Newtonsoft.Json;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Nti;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Tam.Maestro.Services.Cable.Entities;

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
        /// Processes the document received from Nielsen and saves it to db
        /// This method is public because it's used in the integration tests
        /// </summary>
        /// <param name="ntiFile">NtiFile object used in the integration tests</param>
        /// <param name="pdfDocumentFromNielson">List of NtiRatingDocumentDto objects receved from Nielsen</param>
        /// <returns>BaseResponse object</returns>
        BaseResponse ProcessFileContent(NtiFile ntiFile, BaseResponse<List<NtiRatingDocumentDto>> pdfDocumentFromNielson);

        BaseResponse<List<NtiRatingDocumentDto>> _SendPdfDocumentToNielson(FileRequest request);
    }

    public class NtiTransmittalsService : INtiTransmittalsService
    {
        private readonly INtiTransmittalsRepository _NtiTransmittalsRepository;
        private readonly IProposalRepository _ProposalRepository;
        private readonly INsiComponentAudienceRepository _NsiComponentAudienceRepository;
        private readonly IBroadcastAudienceRepository _BroadcastAudienceRepository;
        private readonly IConfigurationSettingsHelper _ConfigurationSettingsHelper;
        private Lazy<string> _BroadcastNTIUploadApiUrl;

        public NtiTransmittalsService(IDataRepositoryFactory broadcastDataRepositoryFactory, IConfigurationSettingsHelper configurationSettingsHelper)
        {
            _NtiTransmittalsRepository = broadcastDataRepositoryFactory.GetDataRepository<INtiTransmittalsRepository>();
            _BroadcastAudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<IBroadcastAudienceRepository>();
            _ProposalRepository = broadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
            _NsiComponentAudienceRepository = broadcastDataRepositoryFactory.GetDataRepository<INsiComponentAudienceRepository>();
            _ConfigurationSettingsHelper = configurationSettingsHelper;
            _BroadcastNTIUploadApiUrl = new Lazy<string>(_GetBroadcastNTIUploadApiUrl);
        }

        /// <summary>
        /// Uploads a pdf file containing nti transmittals
        /// </summary>
        /// <param name="request">FileRequest object having the file content encripted as base64</param>
        /// <param name="name">Username requesting the upload</param>
        /// <returns>BaseResponse object containing message of success or failure</returns>
        public BaseResponse UploadNtiTransmittalsFile(FileRequest request, string name)
        {
            BaseResponse<List<NtiRatingDocumentDto>> pdfDocumentFromNielson = _SendPdfDocumentToNielson(request);
            var ntiFile = new NtiFile
            {
                CreatedDate = DateTime.Now,
                FileName = request.FileName,
                CreatedBy = name,
                Status = pdfDocumentFromNielson.Success ? FileProcessingStatusEnum.Valid : FileProcessingStatusEnum.Invalid
            };
            var result = ProcessFileContent(ntiFile, pdfDocumentFromNielson);
            return result;
        }

        /// <summary>
        /// Processes the document received from Nielsen and saves it to db
        /// This method is public because it's used in the integration tests
        /// </summary>
        /// <param name="ntiFile">NtiFile object used in the integration tests</param>
        /// <param name="pdfDocumentFromNielson">List of NtiRatingDocumentDto objects receved from Nielsen</param>
        /// <returns>BaseResponse object</returns>
        public BaseResponse ProcessFileContent(NtiFile ntiFile, BaseResponse<List<NtiRatingDocumentDto>> pdfDocumentFromNielson)
        {
            if (pdfDocumentFromNielson.Success == false)
            {
                ntiFile.FileProblems.Add(new FileProblem { ProblemDescription = pdfDocumentFromNielson.Message });
                _NtiTransmittalsRepository.SaveFile(ntiFile);
                return new BaseResponse { Success = false, Message = pdfDocumentFromNielson.Message };
            }

            ntiFile.Details = _MapFileDetails(pdfDocumentFromNielson.Data);
            var notFoundedReports = _CheckReportNamesAndLoadProposalMatchingWeeks(ntiFile.Details);
            ntiFile.FileProblems.AddRange(notFoundedReports.Select(x => new FileProblem { ProblemDescription = $"Could not find report {x}" }).ToList());

            foreach (var detail in ntiFile.Details)
            {
                var totalImpressions = _LoadNtiTransmittalsAudiences(detail);
                _SetProposalWeekImpressions(detail, totalImpressions);
            }

            _NtiTransmittalsRepository.SaveFile(ntiFile);

            return new BaseResponse
            {
                Success = true,
                Data = notFoundedReports,
                Message = notFoundedReports.Any() ? "File processed, NTI data partially processed" : "File processed, NTI data processed"
            };
        }

        private void _SetProposalWeekImpressions(NtiRatingDocument detail, List<NtiComponentAudiencesImpressions> totalImpressionsForComponents)
        {
            if (detail.ProposalWeeks.Count == 0)
            {
                return;
            }
                        
            foreach (var week in detail.ProposalWeeks)
            {                
                foreach (var component in totalImpressionsForComponents)
                {
                    double totalImpressionsForWeeks = detail.ProposalWeeks.Sum(x => x.NsiImpressions.Where(w=>w.Key == component.AudienceId).Sum(w=>w.Value));
                    var impressions = component.Impressions;
                    if (totalImpressionsForWeeks > 0)
                    {
                        //we split the impression based on the NSI impressions ratio between the weeks matched
                        impressions = component.Impressions * week.NsiImpressions.Where(w => w.Key == component.AudienceId).Sum(w=>w.Value) / totalImpressionsForWeeks;
                    }
                    else
                    {   //if there are no NSI impressions we split the NTI impressions evenly
                        impressions = component.Impressions / detail.ProposalWeeks.Count;
                    }

                    week.Audiences.Add(new NtiComponentAudiencesImpressions
                    {
                        AudienceId = component.AudienceId,
                        Impressions = impressions
                    });
                }
            }
        }

        private List<string> _CheckReportNamesAndLoadProposalMatchingWeeks(List<NtiRatingDocument> details)
        {
            List<string> notFoundedReports = new List<string>();

            foreach (var report in details)
            {
                report.ProposalWeeks = _NtiTransmittalsRepository.GetProposalWeeksByReportName(report.Header.ReportName);
                if (!report.ProposalWeeks.Any())
                {
                    notFoundedReports.Add(report.Header.ReportName);
                }
            }

            return notFoundedReports;
        }

        private List<NtiComponentAudiencesImpressions> _LoadNtiTransmittalsAudiences(NtiRatingDocument detail)
        {
            List<NtiComponentAudiencesImpressions> ntiAudiences = new List<NtiComponentAudiencesImpressions>();
            var componentAudiences = _NsiComponentAudienceRepository.GetAllNsiComponentAudiences();
            foreach (var component in componentAudiences)
            {
                NtiComponentAudiencesImpressions ntiAudience = new NtiComponentAudiencesImpressions { AudienceId = component.Id };

                double rK611 = detail.Ratings.Where(x => x.Category.Equals("CHILDREN") && x.SubCategory.Equals("CHILDREN 6-11")).Single().Impressions;
                double rK211 = detail.Ratings.Where(x => x.Category.Equals("CHILDREN") && x.SubCategory.Equals("CHILDREN 2-11")).Single().Impressions;

                double rF18 = detail.Ratings.Where(x => x.Category.Equals("WOMEN") && x.SubCategory.Equals("TOTAL")).Single().Impressions;
                double rF1849 = detail.Ratings.Where(x => x.Category.Equals("WOMEN") && x.SubCategory.Equals("18-49")).Single().Impressions;
                double rF55 = detail.Ratings.Where(x => x.Category.Equals("WOMEN") && x.SubCategory.Equals("55+")).Single().Impressions;
                double rF1834 = detail.Ratings.Where(x => x.Category.Equals("WOMEN") && x.SubCategory.Equals("18-34")).Single().Impressions;
                double rF3564 = detail.Ratings.Where(x => x.Category.Equals("WOMEN") && x.SubCategory.Equals("35-64")).Single().Impressions;
                double rF2554 = detail.Ratings.Where(x => x.Category.Equals("WOMEN") && x.SubCategory.Equals("25-54")).Single().Impressions;

                double rM18 = detail.Ratings.Where(x => x.Category.Equals("MEN") && x.SubCategory.Equals("TOTAL")).Single().Impressions;
                double rM1849 = detail.Ratings.Where(x => x.Category.Equals("MEN") && x.SubCategory.Equals("18-49")).Single().Impressions;
                double rM55 = detail.Ratings.Where(x => x.Category.Equals("MEN") && x.SubCategory.Equals("55+")).Single().Impressions;
                double rM1834 = detail.Ratings.Where(x => x.Category.Equals("MEN") && x.SubCategory.Equals("18-34")).Single().Impressions;
                double rM3564 = detail.Ratings.Where(x => x.Category.Equals("MEN") && x.SubCategory.Equals("35-64")).Single().Impressions;
                double rM2554 = detail.Ratings.Where(x => x.Category.Equals("MEN") && x.SubCategory.Equals("25-54")).Single().Impressions;

                switch (component.Code)
                {
                    //we don't have a way to get impression for these cases, so we ignore them
                    case "F12-14":
                    case "F15-17":
                    case "M12-14":
                    case "M15-17":
                    case "F18-20":
                    case "F21-24":
                    case "M18-20":
                    case "M21-24":
                        break;

                    case "HH":
                        ntiAudience.Impressions = detail.Ratings.Where(x => x.Category.Equals("TOTAL US HHLD")).Single().Impressions;
                        break;
                    case "K6-11":
                        ntiAudience.Impressions = rK611;
                        break;
                    case "K2-5":
                        ntiAudience.Impressions = rK211 - rK611;
                        break;
                    case "F50-54":
                        ntiAudience.Impressions = (rF18 - rF1849) - rF55;
                        break;
                    case "F55-64":
                        ntiAudience.Impressions = rF55 - (rF18 - (rF1834 + rF3564));
                        break;
                    case "F65+":
                        ntiAudience.Impressions = rF18 - (rF1834 + rF3564);
                        break;
                    case "F35-49":
                        ntiAudience.Impressions = rF1849 - rF1834;
                        break;
                    case "F25-34":
                        ntiAudience.Impressions = rF1834 - (rF18 - (rF2554 + rF55));
                        break;
                    case "M50-54":
                        ntiAudience.Impressions = (rM18 - rM1849) - rM55;
                        break;
                    case "M55-64":
                        ntiAudience.Impressions = rM55 - (rM18 - (rM1834 + rM3564));
                        break;
                    case "M65+":
                        ntiAudience.Impressions = rM18 - (rM1834 + rM3564);
                        break;
                    case "M35-49":
                        ntiAudience.Impressions = rM1849 - rM1834;
                        break;
                    case "M25-34":
                        ntiAudience.Impressions = rM1834 - (rM18 - (rM2554 + rM55));
                        break;
                }
                //nti impressions are stored as (0000)
                //so we are going to multiply by 10 to get it in (000)
                ntiAudience.Impressions = ntiAudience.Impressions * 10;

                ntiAudiences.Add(ntiAudience);
            }
            return ntiAudiences;
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
                    categories.AddRange(rating.SubCategories.Select(x => new NtiRatingCategory
                    {
                        Category = rating.Category,
                        SubCategory = x.SubCategory,
                        Impressions = string.IsNullOrWhiteSpace(x.Impressions) ? 0 : double.Parse(x.Impressions),
                        Percent = string.IsNullOrWhiteSpace(x.Percent) ? 0 : double.Parse(x.Percent),
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
                
        public BaseResponse<List<NtiRatingDocumentDto>> _SendPdfDocumentToNielson(FileRequest request)
        {
            var nielsonRequest = new NtiPdfDto { Base64String = request.RawData };
            var jsonRequest = JsonConvert.SerializeObject(nielsonRequest);
            using (var webClient = new WebClient())
            {
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                var jsonResult = webClient.UploadString(_BroadcastNTIUploadApiUrl.Value, jsonRequest);
                var result = JsonConvert.DeserializeObject<BaseResponse<List<NtiRatingDocumentDto>>>(jsonResult);
                return result;
            }
        }
        private string _GetBroadcastNTIUploadApiUrl()
        {
            return _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.BroadcastNTIUploadApiUrl);
        }
    }
}
