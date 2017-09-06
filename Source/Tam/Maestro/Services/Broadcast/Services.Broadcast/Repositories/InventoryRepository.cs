using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.Repositories
{
    public interface IInventoryRepository : IDataRepository
    {
        InventoryDetail GetInventoryDetailByDaypartCodeAndRateSource(string daypartCode, RatesFile.RateSourceType? rateSource);
        int GetMaximunNumberOfSpotsAvailableByDaypartCode(string daypartCode, RatesFile.RateSourceType? rateSource,
            int mediaWeekId, int spotLengthId);
        void SaveInventoryDetail(InventoryDetail inventoryDetail);
        List<StationProgram> GetStationProgramsPerSpotAvailable(int? spots, int mediaWeekId, RatesFile.RateSourceType? rateSource, int spotLengthId, string daypartCode);
        void UpdateInventoryDetail(InventoryDetail inventory);
    }

    public class InventoryRepository : BroadcastRepositoryBase, IInventoryRepository
    {
        public InventoryRepository(ISMSClient pSmsClient,
            IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory
            , ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public InventoryDetail GetInventoryDetailByDaypartCodeAndRateSource(string daypartCode,
            RatesFile.RateSourceType? rateSource)
        {
            return _InReadUncommitedTransaction(
                context => (from p in context.inventory_details
                            where p.inventory_source == (byte?)rateSource &&
                                  string.Compare(p.daypart_code, daypartCode, StringComparison.OrdinalIgnoreCase) == 0
                            select new InventoryDetail()
                            {
                                DaypartCode = p.daypart_code,
                                InventorySource = (RatesFile.RateSourceType)p.inventory_source,
                                Id = p.id,
                                InventoryDetailSlots = p.inventory_detail_slots.Select(slot => new InventoryDetailSlot()
                                {
                                    DetailLevel = slot.detail_level,
                                    TotalStations = (int)slot.total_stations,
                                    SpotLengthId = slot.spot_length_id,
                                    RolleupDaypartId = slot.rolled_up_daypart_id,
                                    MediaWeekId = slot.media_week_id,
                                    InventoryDetailId = slot.inventory_detail_id,
                                    Id = slot.id,
                                    SlotCost = slot.slot_cost,
                                    InventoryDetailSlotComponents =
                                        slot.inventory_detail_slot_components.Select(
                                            compo => new InventoryDetailSlotComponents()
                                            {
                                                Id = compo.id,
                                                StationCode = compo.station_code,
                                                DaypartId = compo.daypart_id,
                                                StationProgramFlightId = compo.station_program_flight_id,
                                                InventoryDetailSlotId = compo.inventory_detail_slot_id,
                                                MarketCode = compo.station_program_flights.station_programs.station.market_code
                                            }).ToList()
                                }).ToList()
                            }).SingleOrDefault());
        }

        public int GetMaximunNumberOfSpotsAvailableByDaypartCode(string daypartCode,
            RatesFile.RateSourceType? rateSource, int mediaWeekId, int spotLengthId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    //Taking all the programs that match the daypart(s) included in the file, group by daypart, week, station and find maximum spots. 
                    //That will be the maximum number of spots available.
                    var programsMatch = (from p in context.station_programs
                        join f in context.station_program_flights on p.id equals f.station_program_id
                        where string.Compare(p.daypart_code, daypartCode, StringComparison.OrdinalIgnoreCase) == 0 &&
                              p.rate_source == (byte?) rateSource.Value &&
                              f.media_week_id == mediaWeekId &&
                              p.spot_length_id == spotLengthId
                        select new {p.daypart_code, f.media_week_id, p.station_code, f.spots}).GroupBy(
                            a => new {a.daypart_code, a.media_week_id, a.station_code}).ToList();
                    
                    return programsMatch.Any()
                        ? programsMatch.Select(z => z.ToList().Max(a => a.spots ?? 0)).Max()
                        : 0;
                });
        }

        public void SaveInventoryDetail(InventoryDetail inventoryDetail)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var inventory = new inventory_details()
                    {
                        daypart_code = inventoryDetail.DaypartCode,
                        inventory_source = (byte)inventoryDetail.InventorySource,
                        inventory_detail_slots =
                            inventoryDetail.InventoryDetailSlots.Select(slot => new inventory_detail_slots()
                            {
                                detail_level = (byte)slot.DetailLevel,
                                media_week_id = slot.MediaWeekId,
                                rolled_up_daypart_id = slot.RolleupDaypartId,
                                spot_length_id = slot.SpotLengthId,
                                total_stations = (short)slot.TotalStations,
                                slot_cost = slot.SlotCost,
                                inventory_detail_slot_components = _BuildSlotComponents(slot.InventoryDetailSlotComponents)
                            }).ToList()
                    };

                    context.inventory_details.Add(inventory);
                    context.SaveChanges();
                });
        }

        public List<StationProgram> GetStationProgramsPerSpotAvailable(int? spots, int mediaWeekId,
            RatesFile.RateSourceType? rateSource, int spotLengthId, string daypartCode)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    // For each slot and week combination (looking at the weeks specified during file upload) calculate the total number of stations 
                    // that have same or more spots available as that slot level. Create slot/week records.
                    return (from p in context.station_programs
                            join f in context.station_program_flights on p.id equals f.station_program_id
                            where f.media_week_id == mediaWeekId &&
                                  p.daypart_code == daypartCode &&
                                  p.rate_source == (byte?)rateSource &&
                                  p.spot_length_id == spotLengthId &&
                                  f.spots >= spots
                            select new StationProgram()
                            {
                                DaypartCode = p.daypart_code,
                                StationCode = p.station_code,
                                Daypart = new DisplayDaypart()
                                {
                                    Id = p.daypart_id
                                },
                                FixedPrice = p.fixed_price,
                                FlightWeeks = p.station_program_flights.Select(flight => new StationProgramFlightWeek()
                                {
                                    Id = flight.id,
                                    Rate15s = flight.C15s_rate,
                                    Rate30s = flight.C30s_rate,
                                    Rate60s = flight.C60s_rate,
                                    Rate90s = flight.C90s_rate,
                                    Rate120s = flight.C120s_rate,
                                    FlightWeek = new DisplayMediaWeek()
                                    {
                                        Id = flight.media_week_id
                                    }
                                }).ToList()
                            }).ToList();
                });
        }

        public void UpdateInventoryDetail(InventoryDetail inventory)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var originalInventoryDetailSlots = context.inventory_detail_slots.Where(a => a.inventory_detail_id == inventory.Id);

                    // deal with excluded slots
                    var excludedSlotsIds = originalInventoryDetailSlots.Select(b => b.id)
                        .Except(inventory.InventoryDetailSlots.Where(a => a.Id > 0).Select(b => b.Id).ToList())
                        .ToList();

                    // remove inventory slots
                    context.inventory_detail_slots.RemoveRange(
                        context.inventory_detail_slots.Where(a => a.inventory_detail_id == inventory.Id &&
                        excludedSlotsIds.Any(b => b == a.id)));

                    // deal with new added slots
                    _SaveInventorySlots(context, inventory.Id, inventory.InventoryDetailSlots.Where(a => a.Id == 0).ToList());
                    
                    // deal with slots that have been updated
                    inventory.InventoryDetailSlots.Where(a => a.Id > 0).ForEach(slot =>
                    {
                        var updatedSlot = originalInventoryDetailSlots.FirstOrDefault(a => a.id == slot.Id);

                        if (updatedSlot != null)
                        {
                            updatedSlot.spot_length_id = slot.SpotLengthId;
                            updatedSlot.detail_level = (byte) slot.DetailLevel;
                            updatedSlot.media_week_id = slot.MediaWeekId;
                            updatedSlot.total_stations = (short)slot.TotalStations;
                            updatedSlot.rolled_up_daypart_id = slot.RolleupDaypartId;
                            updatedSlot.slot_cost = slot.SlotCost;

                            // remove slot components
                            var slotComponents = originalInventoryDetailSlots.Where(a => a.id == slot.Id)
                                .SelectMany(b => b.inventory_detail_slot_components);
                            context.inventory_detail_slot_components.RemoveRange(slotComponents);

                            // add updated slot components
                            var newSlotComponents = _BuildSlotComponents(slot.InventoryDetailSlotComponents);
                            newSlotComponents.ForEach(q => updatedSlot.inventory_detail_slot_components.Add(q));
                        }
                    });

                    context.SaveChanges();
                });

        }

        private List<inventory_detail_slot_components> _BuildSlotComponents(List<InventoryDetailSlotComponents> inventoryDetailSlotComponentses)
        {
            return inventoryDetailSlotComponentses.Select(
                component => new inventory_detail_slot_components()
                {
                    station_program_flight_id = component.StationProgramFlightId,
                    station_code = component.StationCode,
                    daypart_id = component.DaypartId
                }).ToList();
        }

        private void _SaveInventorySlots(QueryHintBroadcastContext context, int inventoryDetailId,
            List<InventoryDetailSlot> inventoryDetailSlots)
        {
            context.inventory_detail_slots.AddRange(inventoryDetailSlots.Select(slot => new inventory_detail_slots()
            {
                inventory_detail_id = inventoryDetailId,
                detail_level = (byte) slot.DetailLevel,
                media_week_id = slot.MediaWeekId,
                rolled_up_daypart_id = slot.RolleupDaypartId,
                spot_length_id = slot.SpotLengthId,
                total_stations = (short) slot.TotalStations,
                slot_cost = slot.SlotCost,
                inventory_detail_slot_components = _BuildSlotComponents(slot.InventoryDetailSlotComponents)
            }).ToList());

            context.SaveChanges();
        }


    }
}
