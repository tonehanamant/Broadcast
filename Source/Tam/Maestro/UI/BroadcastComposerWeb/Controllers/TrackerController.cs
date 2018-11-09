using Common.Services.WebComponents;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/Tracker")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class TrackerController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private readonly IWebLogger _Logger;

        public TrackerController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever("TrackerController"))
        {
            _Logger = logger;
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        // POST api/Tracker/UploadSchedules
        [Route("UploadSchedules")]
        public BaseResponse<int> UploadSchedules(ScheduleSaveRequest request)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().SaveSchedule(request));
        }

        [HttpGet]
        [Route("LoadSchedules")]
        public BaseResponse<LoadSchedulesDto> LoadSchedules(DateTime? startDate = null, DateTime? endDate = null)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().GetSchedulesByDate(startDate, endDate));
        }

        [HttpGet]
        [Route("GetInitialData")]
        public BaseResponse<BvsLoadDto> GetInitialData()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().GetBvsLoadData(DateTime.Now));
        }

        [HttpPost]
        [Route("UploadBvsFile")]
        public BaseResponse<List<int>> UploadBvsFile(HttpRequestMessage saveRequest)
        {
            if (saveRequest == null)
            {
                throw new Exception("No BVS file data received.");
            }

            FileSaveRequest bvsRequest = JsonConvert.DeserializeObject<FileSaveRequest>(saveRequest.Content.ReadAsStringAsync().Result);
            return _ConvertToBaseResponseSuccessWithMessage(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().SaveBvsFiles(bvsRequest, Identity.Name));
        }

        [HttpPost]
        [Route("UploadSigmaFile")]
        public BaseResponse<List<int>> UploadSigmaFile(HttpRequestMessage saveRequest)
        {
            if (saveRequest == null)
            {
                throw new Exception("No Sigma file data received.");
            }

            FileSaveRequest bvsRequest = JsonConvert.DeserializeObject<FileSaveRequest>(saveRequest.Content.ReadAsStringAsync().Result);
            return _ConvertToBaseResponseSuccessWithMessage(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().SaveBvsFiles(bvsRequest, Identity.Name, true));
        }

        [HttpPost]
        [Route("UploadBvsFtp")]
        public BaseResponse<string> UploadViaFTP()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().SaveBvsViaFtp(Identity.Name));
        }

        [HttpPost]
        [Route("UploadScheduleFile")]
        public BaseResponse<int> UploadScheduleFile(HttpRequestMessage saveSchedule)
        {
            if (saveSchedule == null)
            {
                throw new Exception("No Schedule file data received.");
            }
            ScheduleSaveRequest saveRequest = JsonConvert.DeserializeObject<ScheduleSaveRequest>(saveSchedule.Content.ReadAsStringAsync().Result, new IsoDateTimeConverter { DateTimeFormat = "MM-dd-yyyy" });
            saveRequest.Schedule.UserName = Identity.Name;
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().SaveSchedule(saveRequest));
        }

        [HttpGet]
        [Route("ScheduleExists")]
        public BaseResponse<bool> SchedulesExist(int estimateId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().ScheduleExists(estimateId));
        }

        [HttpGet]
        [Route("ScheduleHeader/{estimateId}")]
        public BaseResponse<ScheduleHeaderDto> GetScheduleHeader(int estimateId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().GetScheduleHeader(estimateId));
        }

        [HttpGet]
        [Route("GetBvsScrubbingData")]
        public BaseResponse<BvsScrubbingDto> GetBvsScrubbingData(int estimateId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().GetBvsScrubbingData(estimateId));
        }

        public BaseResponse<List<LookupDto>> GetSchedulePrograms(int scheduleId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().GetSchedulePrograms(scheduleId));
        }

        public BaseResponse<List<LookupDto>> GetScheduleStations(int scheduleId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().GetScheduleStations(scheduleId));
        }

        [HttpPost]
        [Route("SaveScrubbingMappings")]
        public BaseResponse<List<BvsTrackingDetail>> SaveScrubbingMappings(ScrubbingMap map)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().SaveScrubbingMapping(map));
        }

        [HttpPost]
        [Route("ScrubSchedule")]
        public BaseResponse<bool> UpdateSchedule(ScheduleScrubbing scheduleScrubbing)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().ScrubSchedule(scheduleScrubbing));
        }

        [HttpPost]
        [Route("AcceptScheduleLeadin")]
        public BaseResponse<BvsTrackingDetail> AcceptScheduleLeadin(AcceptScheduleLeadinRequest request)
        {
            return
                _ConvertToBaseResponse(() =>
                        _ApplicationServiceFactory.GetApplicationService<ITrackingEngine>().AcceptScheduleLeadIn(request));
        }

        [HttpPost]
        [Route("AcceptScheduleBlock")]
        public BaseResponse<BvsTrackingDetail> AcceptScheduleBlock(AcceptScheduleBlockRequest request)
        {
            return
                _ConvertToBaseResponse(() =>
                        _ApplicationServiceFactory.GetApplicationService<ITrackingEngine>().AcceptScheduleBlock(request));
        }

        [HttpGet]
        [Route("GetProgramMapping")]
        public BaseResponse<ProgramMappingDto> GetProgramMapping(int bvsDetailId)
        {
            return _ConvertToBaseResponse(
                () =>
                        _ApplicationServiceFactory.GetApplicationService<ITrackingEngine>().GetProgramMappingDto(bvsDetailId));
        }

        [HttpGet]
        [Route("ScheduleReport/{scheduleId:int}")]
        public HttpResponseMessage DownloadScheduleReport(int scheduleId)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<ISchedulesReportService>().GenerateScheduleReport(scheduleId);
            var result = Request.CreateResponse(HttpStatusCode.OK);

            result.Content = new StreamContent(report.Stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = report.Filename
            };

            return result;
        }

        [HttpGet]
        [Route("ClientReport/{scheduleId:int}")]
        public HttpResponseMessage DownloadClientReport(int scheduleId)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<ISchedulesReportService>().GenerateClientReport(scheduleId);
            var result = Request.CreateResponse(HttpStatusCode.OK);

            result.Content = new StreamContent(report.Stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = report.Filename
            };

            return result;
        }

        [HttpGet]
        [Route("ProviderReport/{scheduleId:int}")]
        public HttpResponseMessage DownloadProviderReport(int scheduleId)
        {
            var report = _ApplicationServiceFactory.GetApplicationService<ISchedulesReportService>().Generate3rdPartyProviderReport(scheduleId);
            var result = Request.CreateResponse(HttpStatusCode.OK);

            result.Content = new StreamContent(report.Stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = report.Filename
            };

            return result;
        }

        [HttpGet]
        [Route("Mappings/{mappingType}")]
        public BaseResponse<BvsMap>  GetMappingsList(string mappingType)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<ITrackerService>()
                            .GetBvsMapByType(mappingType));
        }

        [HttpPost]
        [Route("Mappings/{mappingType}/Delete")]
        public BaseResponse<bool> DeleteMapping(string mappingType, TrackingMapValue mapping)
        {
            return
                    _ConvertToBaseResponse(
                        () =>
                            _ApplicationServiceFactory.GetApplicationService<ITrackerService>()
                                .DeleteMapping(mappingType, mapping));    
        }

        [HttpGet]
        [Route("GetSchedule/{scheduleId}")]
        public BaseResponse<DisplaySchedule> GetScheduleSettings(int scheduleId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().GetDisplayScheduleById(scheduleId));
        }

        [HttpPost]
        [Route("RatingAdjustments")]
        public BaseResponse<bool> UpdateRatingAdjustments(List<RatingAdjustmentsDto> ratingAdjustments)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().UpdateRatingAdjustments(ratingAdjustments));
        }

        [HttpGet]
        [Route("RatingAdjustments")]
        public BaseResponse<RatingAdjustmentsResponse> GetRatingAdjustments()
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().GetRatingAdjustments());
        }

        [HttpGet]
        [Route("BvsFileSummaries")]
        public BaseResponse<List<BvsFileSummary>> GetBvsFileSummaries()
        {
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().GetBvsFileSummaries());
        }
        [HttpPut]
        [Route("TrackSchedule/{scheduleId}")]
        public BaseResponse<bool> TrackSchedule(int scheduleId)
        {
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().TrackSchedule(scheduleId));
        }
        [HttpDelete]
        [Route("BvsFile/{bvsFileId}")]
        public BaseResponse<bool> DeleteBvsFile(int bvsFileId)
        {
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<ITrackerService>().DeleteBvsFile(bvsFileId));
        }        
    }
}