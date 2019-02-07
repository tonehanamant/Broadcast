using Common.Services.WebComponents;
using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Services.Broadcast.Exceptions;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Cable.Security;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [RoutePrefix("api/RatesManager")]
    [RestrictedAccess(RequiredRole = RoleType.Broadcast_Proposer)]
    public class InventoryApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;
        private readonly IWebLogger _Logger;

        public InventoryApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(InventoryApiController).Name))
        {
            _Logger = logger;
            _ApplicationServiceFactory = applicationServiceFactory;
        }

        [HttpGet]
        [Route("Contacts/Find")]
        public BaseResponse<List<StationContact>> FindContacts([FromUri] String query)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IInventoryService>().FindStationContactsByName(query));
        }

        [HttpGet]
        [Route("{inventorySource}/Stations")]
        public BaseResponse<List<DisplayBroadcastStation>> GetAllStations(string inventorySource)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventoryService>().GetStations(inventorySource, DateTime.Now));
        }

        [HttpGet]
        [Route("{inventorySource}/Stations")]
        public BaseResponse<List<DisplayBroadcastStation>> GetAllStationsWithFilter(string inventorySource, [FromUri] string filter)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .GetStationsWithFilter(inventorySource, filter, DateTime.Now));
        }

        [HttpGet]
        [Route("{inventorySource}/Stations/{stationCode}")]
        public BaseResponse<StationDetailDto> GetStationPrograms(string inventorySource, int stationCode)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .GetStationDetailByCode(inventorySource, stationCode));
        }

        [HttpGet]
        [Route("{rateSource}/Stations/{stationCode}/Rates")]
        public BaseResponse<List<StationProgram>> GetStationProgramsByDateRange(string rateSource, int stationCode, [FromUri] DateTime startDate, [FromUri] DateTime endDate)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IInventoryService>().GetStationPrograms(rateSource, stationCode, startDate, endDate));

        }

        [HttpGet]
        [Route("{rateSource}/Stations/{stationCode}/Rates")]
        public BaseResponse<List<StationProgram>> GetAllStationPrograms(string rateSource, int stationCode)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .GetAllStationPrograms(rateSource, stationCode));
        }

        [HttpGet]
        [Route("{rateSource}/Stations/{stationCode}/Rates/{timeFrame}")]
        public BaseResponse<List<StationProgram>> GetStationPrograms(string rateSource, int stationCode, string timeFrame)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IInventoryService>().GetStationPrograms(rateSource, stationCode, timeFrame, DateTime.Now));
        }

        [HttpGet]
        [Route("{inventorySource}/Stations/{stationCode}/Contacts")]
        public BaseResponse<List<StationContact>> GetStationContacts(string inventorySource, int stationCode)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .GetStationContacts(inventorySource, stationCode));
        }

        [HttpPost]
        [Route("Contacts")]
        public BaseResponse<bool> SaveStationContact(StationContact stationContact)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .SaveStationContact(stationContact, Identity.Name));
        }

        [HttpDelete]
        [Route("{inventorySource}/Contacts/{stationContactId}")]
        public BaseResponse<bool> DeleteStationContact(string inventorySource, int stationContactId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                        .DeleteStationContact(inventorySource, stationContactId, Identity.Name));
        }
        
        [HttpPost]
        [Route("UploadInventoryFile")]
        public BaseResponse<InventoryFileSaveResult> UploadInventoryFile(HttpRequestMessage saveRequest)
        {
            if (saveRequest == null)
            {
                throw new Exception("No Rate file data received.");
            }

            var ratesSaveRequest = JsonConvert.DeserializeObject<InventoryFileSaveRequest>(saveRequest.Content.ReadAsStringAsync().Result);
            ratesSaveRequest.UserName = Identity.Name;
            try
            {
                var service = _ApplicationServiceFactory.GetApplicationService<IInventoryService>();

                var result = string.Equals(ratesSaveRequest.InventorySource, "Barter", StringComparison.InvariantCultureIgnoreCase)?
                        service.SaveBarterInventoryFile(ratesSaveRequest, ratesSaveRequest.UserName) :
                        service.SaveInventoryFile(ratesSaveRequest);

                return new BaseResponse<InventoryFileSaveResult>()
                {
                    Data = result,
                    Success = true
                };
            }
            catch (FileUploadException<InventoryFileProblem> e)
            {
                return new BaseResponseWithProblems<InventoryFileSaveResult, InventoryFileProblem>()
                {
                    Problems = e.Problems,
                    Message = "Problems found while processing file",
                    Success = false
                };
            }
            catch (Exception e)
            {
                return new BaseResponse<InventoryFileSaveResult>()
                {
                    Success = false,
                    Message = e.Message
                };
            }
        }


        [HttpGet]
        [Route("Genres")]
        public BaseResponse<List<LookupDto>> GetAllGenres()
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .GetAllGenres());
        }

        [HttpGet]
        [Route("InitialData")]
        public BaseResponse<RatesInitialDataDto> GetInitialRatesData()
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IInventoryService>().GetInitialRatesData());
        }

        [HttpGet]
        [Route("Stations/{stationCode}/Lock")]
        public BaseResponse<LockResponse> LockStation(int stationCode)
        {
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IInventoryService>().LockStation(stationCode));
        }

        [HttpGet]
        [Route("Stations/{stationCode}/UnLock")]
        public BaseResponse<ReleaseLockResponse> UnlockStation(int stationCode)
        {
            return _ConvertToBaseResponse(
                () => _ApplicationServiceFactory.GetApplicationService<IInventoryService>().UnlockStation(stationCode));
        }

        [HttpPost]
        [Route("Programs")]
        public BaseResponse<bool> SaveProgram(StationProgram stationProgram)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventoryService>().SaveProgram(stationProgram,Identity.Name));
        }

        [HttpPost]
        [Route("ConvertRate")]
        public BaseResponse<Decimal> ConvertRate(RateConversionRequest request)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .ConvertRateForSpotLength(request.Rate30, request.SpotLength));
        }

        [HttpPost]
        [Route("Conflicts")]
        public BaseResponse<List<StationProgram>> GetStationConflicts(StationProgramConflictRequest conflict)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                .GetStationProgramConflicts(conflict));
        }

        [HttpPost]
        [Route("Conflicts/{programId}")]
        public BaseResponse<bool> GetStationProgramConflicted(StationProgramConflictRequest conflict, int programId)
        {
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                .GetStationProgramConflicted(conflict, programId));
        }

        [HttpDelete]
        [Route("{inventorySourceString}/{stationCode}/Programs/{programId}")]
        public BaseResponse<bool> DeleteProgra(string inventorySourceString, int stationCode, int programId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                        .DeleteProgram(programId, inventorySourceString, stationCode, Identity.Name));
        }

        [HttpPost]
        [Route("{inventorySourceString}/{stationCode}/Programs/{ProgramId}/Flight")]
        public BaseResponse<bool> TrimProgramFlight(string inventorySourceString, int stationCode, int programId, [FromUri] DateTime endDate)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .ExpireManifest(programId, endDate, inventorySourceString, stationCode, Identity.Name));
        }

        [HttpGet]
        [Route("Programs/{ProgramId}/HasSpotsAllocated")]
        public BaseResponse<bool> HasSpotsAllocated(int programId)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .HasSpotsAllocated(programId));
        }
    }
}