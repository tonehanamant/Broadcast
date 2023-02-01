﻿using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities.SpotExceptions.OutOfSpecs;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using Services.Broadcast.Repositories.SpotExceptions;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Services.Broadcast.Entities.DTO.Program;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities.ProgramMapping;
using System.Web.WebPages;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Common;

namespace Services.Broadcast.ApplicationServices.SpotExceptions
{
    public interface ISpotExceptionsOutOfSpecServiceV2 : IApplicationService
    {
        /// <summary>
        /// Get the list of reason codes
        /// </summary>
        Task<List<SpotExceptionsOutOfSpecReasonCodeResultDtoV2>> GetSpotExceptionsOutOfSpecReasonCodesAsyncV2(
SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest);

        /// <summary>
        /// Gets the inventory Sources and the Count
        /// </summary>
        /// <param name="spotExceptionsOutOfSpecSpotsRequest">plan id , start date ,end date</param>
        /// <returns>count and inventory source List</returns>
        Task<List<SpotExceptionOutOfSpecSpotInventorySourcesDtoV2>> GetSpotExceptionsOutOfSpecSpotInventorySourcesAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest);
    }
    public class SpotExceptionsOutOfSpecServiceV2 : BroadcastBaseClass, ISpotExceptionsOutOfSpecServiceV2
    {
        private readonly ISpotExceptionsOutOfSpecRepositoryV2 _SpotExceptionsOutOfSpecRepositoryV2;

        public SpotExceptionsOutOfSpecServiceV2(
          IDataRepositoryFactory dataRepositoryFactory,
          IFeatureToggleHelper featureToggleHelper,
          IConfigurationSettingsHelper configurationSettingsHelper)
          : base(featureToggleHelper, configurationSettingsHelper)
        {
            _SpotExceptionsOutOfSpecRepositoryV2 = dataRepositoryFactory.GetDataRepository<ISpotExceptionsOutOfSpecRepositoryV2>();
        }
        /// <inheritdoc />
        public async Task<List<SpotExceptionsOutOfSpecReasonCodeResultDtoV2>> GetSpotExceptionsOutOfSpecReasonCodesAsyncV2(
SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var spotExceptionsOutOfSpecReasonCodeResults = new List<SpotExceptionsOutOfSpecReasonCodeResultDtoV2>();

            _LogInfo($"Starting: Retrieving Spot Exception Out Of Spec Spot Reason Codes V2");
            try
            {
                var spotExceptionsOutOfSpecReasonCodes = await _SpotExceptionsOutOfSpecRepositoryV2.GetSpotExceptionsOutOfSpecReasonCodesV2(spotExceptionsOutOfSpecSpotsRequest.PlanId,
                    spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);

                spotExceptionsOutOfSpecReasonCodeResults = spotExceptionsOutOfSpecReasonCodes.Select(spotExceptionsOutOfSpecReasonCode => new SpotExceptionsOutOfSpecReasonCodeResultDtoV2
                {
                    Id = spotExceptionsOutOfSpecReasonCode.Id,
                    ReasonCode = spotExceptionsOutOfSpecReasonCode.ReasonCode,
                    Description = spotExceptionsOutOfSpecReasonCode.Reason,
                    Label = spotExceptionsOutOfSpecReasonCode.Label,
                    Count = spotExceptionsOutOfSpecReasonCode.Count
                }).OrderBy(x => x.Label).ToList();

                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Spot Reason Codes V2");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Spot Reason Codes V2";
                throw new CadentException(msg, ex);
            }

            return spotExceptionsOutOfSpecReasonCodeResults;
        }

        /// <inheritdoc />
        public async Task<List<SpotExceptionOutOfSpecSpotInventorySourcesDtoV2>> GetSpotExceptionsOutOfSpecSpotInventorySourcesAsync(SpotExceptionsOutOfSpecSpotsRequestDto spotExceptionsOutOfSpecSpotsRequest)
        {
            var outOfSpecSpotsToDo = new List<string>();
            var outOfSpecSpotsDone = new List<string>();
            List<SpotExceptionOutOfSpecSpotInventorySourcesDtoV2> inventorySources = new List<SpotExceptionOutOfSpecSpotInventorySourcesDtoV2>();

            _LogInfo($"Starting: Retrieving Spot Exceptions Out Of Spec Spot Inventory Sources V2");
            try
            {
                outOfSpecSpotsToDo = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotsToDoInventorySourcesV2Async(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);
                outOfSpecSpotsDone = await _SpotExceptionsOutOfSpecRepositoryV2.GetOutOfSpecSpotsDoneInventorySourcesV2Async(spotExceptionsOutOfSpecSpotsRequest.PlanId, spotExceptionsOutOfSpecSpotsRequest.WeekStartDate, spotExceptionsOutOfSpecSpotsRequest.WeekEndDate);

                var concatTodoAndDoneStringList = outOfSpecSpotsToDo.Concat(outOfSpecSpotsDone).OrderBy(y => y).ToList();
                var groupedInventorySources = concatTodoAndDoneStringList.GroupBy(x => x).ToList();
                foreach (var inventorySource in groupedInventorySources)
                {
                    int count = inventorySource.Count();
                    string name = inventorySource.Select(x => x).FirstOrDefault();
                    var result = new SpotExceptionOutOfSpecSpotInventorySourcesDtoV2
                    {
                        Name = name,
                        Count = count
                    };
                    inventorySources.Add(result);
                }
                _LogInfo($"Finished: Retrieving Spot Exceptions Out Of Spec Inventory Sources V2");
            }
            catch (Exception ex)
            {
                var msg = $"Could not retrieve Spot Exceptions Out Of Spec Inventory Sources V2";
                throw new CadentException(msg, ex);
            }
            return inventorySources;
        }

    }
}