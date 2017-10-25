using Common.Services.WebComponents;
using Newtonsoft.Json;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
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
    public class InventoryApiController : ControllerBase
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
        public BaseResponse<List<DisplayBroadcastStation>> GetAllStationsWithFilter(string rateSource, [FromUri] string filter)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .GetStationsWithFilter(rateSource, filter, DateTime.Now));
        }

        [HttpGet]
        [Route("{inventorySource}/Stations/{stationCode}")]
        public BaseResponse<StationDetailDto> GetStationPrograms(string rateSource, int stationCode)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .GetStationDetailByCode(rateSource, stationCode));
        }

        [HttpGet]
        [Route("{inventorySource}/Stations/{stationCode}/Contacts")]
        public BaseResponse<List<StationContact>> GetStationContacts(string rateSource, int stationCode)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .GetStationContacts(rateSource, stationCode));
        }

        [HttpPost]
        [Route("Contacts")]
        public BaseResponse<bool> SaveStationContact(StationContact stationContact)
        {
            return
                _ConvertToBaseResponse(
                    () =>
                        _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                            .SaveStationContact(stationContact, User.Identity.Name));
        }

        [HttpDelete]
        [Route("Contacts/{stationContactId}")]
        public BaseResponse<bool> DeleteStationContact(int stationContactId)
        {
            return
                _ConvertToBaseResponse(
                    () => _ApplicationServiceFactory.GetApplicationService<IInventoryService>()
                        .DeleteStationContact(stationContactId, User.Identity.Name));
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
            ratesSaveRequest.UserName = User.Identity.Name;
            return _ConvertToBaseResponse(() => _ApplicationServiceFactory.GetApplicationService<IInventoryService>().SaveInventoryFile(ratesSaveRequest));
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
    }
}