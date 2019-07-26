﻿using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Linq;
using ConfigurationService.Client;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.Repositories
{
    public interface ITrackerMappingRepository : IDataRepository
    {
        BvsMap GetMap(int mapId);
        void SaveScrubbingMapping(ScrubbingMap map, TrackingMapType mapType);
        void DeleteMapping(string bvsValue, string scheduleValue, TrackingMapType mappingType);
    }
    public class TrackerMappingRepository : BroadcastRepositoryBase, ITrackerMappingRepository
    {
        public TrackerMappingRepository(IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper, IConfigurationWebApiClient pConfigurationWebApiClient)
            : base(pBroadcastContextFactory, pTransactionHelper, pConfigurationWebApiClient) { }

        public BvsMap GetMap(int mapId)
        {
            return _InReadUncommitedTransaction(
                     context =>
                     {
                         var map = new BvsMap();

                         var mapTypeQuery = (from x in context.bvs_map_types
                                             where x.id == mapId
                                             select x);
                         var mapType = mapTypeQuery.Single();

                         var query = (
                             from x in context.bvs_maps
                             where x.bvs_map_type_id == mapId
                             select x);

                         map.Id = mapId;
                         map.Version = mapType.version;
                         map.TrackingMapType = (TrackingMapType) mapId;
                         map.TrackingMapValues = query.ToList().Select(
                             x => new TrackingMapValue
                             {
                                 BvsValue = x.bvs_value,
                                 ScheduleValue = x.schedule_value
                             }).ToList();
                         return map;
                     });
        }

        public void SaveScrubbingMapping(ScrubbingMap map, TrackingMapType mapType)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var bvsMap = new bvs_maps();
                    bvsMap.bvs_map_type_id = (int)mapType;
                    bvsMap.bvs_value = mapType == TrackingMapType.Program ? map.BvsProgram : map.BvsStation ;
                    bvsMap.schedule_value = mapType == TrackingMapType.Program ? map.ScheduleProgram : map.ScheduleStation;
                    context.bvs_maps.Add(bvsMap);

                    try
                    {
                        context.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        //This is here for a very specific exception. 
                        if (e.InnerException != null && e.InnerException.InnerException !=null && e.InnerException.InnerException.Message.Contains("Violation of PRIMARY KEY constraint"))
                        {
                            throw new Exception("Mapping already exists.");
                        }
                        throw;
                    }
                });
        }

        public void DeleteMapping(string bvsValue, string scheduleValue, TrackingMapType mappingType)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var bvsMap =
                        context.bvs_maps.Where(
                            m =>
                                m.bvs_map_type_id == (int) mappingType &&
                                m.bvs_value.Equals(bvsValue, StringComparison.InvariantCultureIgnoreCase) &&
                                m.schedule_value.Equals(scheduleValue, StringComparison.InvariantCultureIgnoreCase))
                            .Single(
                                string.Format("Unable to find mapping to delete for {0}, {1}", bvsValue, scheduleValue));
                    context.bvs_maps.Remove(bvsMap);
                    context.SaveChanges();

                });
        }
    }
}
