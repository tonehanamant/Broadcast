using Common.Services.WebComponents;
using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.ContractInterfaces;
using Tam.Maestro.Web.Common;

namespace BroadcastComposerWeb.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [RoutePrefix("api/RatesManager")]
    public class RatesApiController : BroadcastControllerBase
    {
        private readonly BroadcastApplicationServiceFactory _ApplicationServiceFactory;

        public RatesApiController(
            IWebLogger logger,
            BroadcastApplicationServiceFactory applicationServiceFactory)
            : base(logger, new ControllerNameRetriever(typeof(RatesApiController).Name))
        {
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
            try
            {
                var result = string.Equals(ratesSaveRequest.InventorySource, "Barter", StringComparison.InvariantCultureIgnoreCase)
                    ? _ApplicationServiceFactory.GetApplicationService<IProprietaryInventoryService>().SaveProprietaryInventoryFile(ratesSaveRequest, Identity.Name, DateTime.Now)
                    : _ApplicationServiceFactory.GetApplicationService<IInventoryService>().SaveInventoryFile(ratesSaveRequest, Identity.Name, DateTime.Now);

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
