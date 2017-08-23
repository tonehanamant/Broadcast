using System;
using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System.Collections.Generic;
using System.Linq;
using Tam.Maestro.Common;
using Tam.Maestro.Services.ContractInterfaces.Common;

namespace Services.Broadcast.ApplicationServices
{
    public interface IInventoryCrunchService : IApplicationService
    {
        List<RatesFileProblem> CrunchThirdPartyInventory(List<StationProgram> stationPrograms, RatesFile.RateSourceType rateSource, List<FlightWeekDto> flightWeeks);

        InventoryDetail GetInventoryDetailByDaypartCodeAndRateSource(string daypartCode,
            RatesFile.RateSourceType rateSource);
    }

    public class InventoryCrunchService : IInventoryCrunchService
    {
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IDaypartCache _DaypartCache;
        private readonly IInventoryRepository _InventoryRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly IProposalInventoryRepository _ProposalInventoryRepository;

        public InventoryCrunchService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _DaypartCache = daypartCache;
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _SpotLengthRepository = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _InventoryRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IInventoryRepository>();
            _ProposalInventoryRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IProposalInventoryRepository>();
        }

        public List<RatesFileProblem> CrunchThirdPartyInventory(List<StationProgram> stationPrograms,
            RatesFile.RateSourceType rateSource, List<FlightWeekDto> flightWeeks)
        {
            var uploadProblems = new List<RatesFileProblem>();

            if (stationPrograms != null && stationPrograms.Any())
            { 
                // set media week ids
                foreach (var week in flightWeeks.Where(week => week.Id == 0))
                {
                    week.Id = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(week.StartDate).Id;
                }
                var listMediaWeeksIds = flightWeeks.Select(a => a.Id).ToList();

                // get distinct daypartcode and spot lengths 
                var daypartCodes = stationPrograms.GroupBy(a => a.DaypartCode)
                    .ToDictionary(z => z.Key, u => u.Select(s => s.SpotLength).Distinct().ToList());

                foreach (var daypart in daypartCodes)
                {
                    // check daypart exists in order to determine if we are updating or creating
                    var inventory =
                        _InventoryRepository.GetInventoryDetailByDaypartCodeAndRateSource(daypart.Key, rateSource);

                    if (inventory == null)
                    {
                        _SaveInventoryDetail(rateSource, flightWeeks, daypart);
                    }
                    else
                    {
                        _UpdateInventoryDetail(rateSource, flightWeeks, inventory, listMediaWeeksIds, daypart, uploadProblems);
                    }
                }
            }

            return uploadProblems;
        }

        private void _SaveInventoryDetail(RatesFile.RateSourceType rateSource, List<FlightWeekDto> flightWeeks, KeyValuePair<string, List<int>> daypart)
        {
            // There will be no records created for weeks that don't have any station spots available
            var inventoryDetail = new InventoryDetail
            {
                InventorySource = rateSource,
                DaypartCode = daypart.Key,
                InventoryDetailSlots = _BuildInventoryDetailSlots(daypart.Key, flightWeeks, rateSource, daypart.Value)
            };

            _InventoryRepository.SaveInventoryDetail(inventoryDetail);
        }

        private void _UpdateInventoryDetail(RatesFile.RateSourceType rateSource, List<FlightWeekDto> flightWeeks, InventoryDetail inventory,
            List<int> mediaWeeksIds, KeyValuePair<string, List<int>> daypart, List<RatesFileProblem> uploadProblems)
        {
           // media weeks need to match
            if (!Enumerable.SequenceEqual(
                    inventory.InventoryDetailSlots.Select(a => a.MediaWeekId).Distinct().OrderBy(a => a).ToList(),
                    mediaWeeksIds.OrderBy(a => a))) return;

            // update inventory object with new (or updated) set of details. Returns what details and media weeks have been updated
            var affectedSlotIds = _UpdateInventoryDetailSlots(daypart.Key, inventory, flightWeeks, rateSource, daypart.Value);

            // delete inventory alloacations
            var deletedProposalSlotsIds = _ProposalInventoryRepository.DeleteInventoryAllocationsByInventory(inventory);
            affectedSlotIds.AddRange(deletedProposalSlotsIds);

            // update inventory detail
            _InventoryRepository.UpdateInventoryDetail(inventory);

            // get proposals that have been affected by updated inventory details
            var affectedProposalsByUpdatedInventory =
                _ProposalInventoryRepository.GetAffectedProposalsByUpdatedInventorySlotAndMediaWeekIds(affectedSlotIds);

            // deal with inventory allocation
            affectedProposalsByUpdatedInventory.ForEach(p => uploadProblems.Add(new RatesFileProblem()
            {
                ProblemDescription = string.Format("{0}({1})", p.Value, p.Key)
            }));
        }

        private List<int> _UpdateInventoryDetailSlots(string daypartCode, InventoryDetail inventory,
            List<FlightWeekDto> flightWeeks, RatesFile.RateSourceType? rateSource, List<int> spotLengths)
        {
            var updatedSlotIds = new List<int>();
            // get detail slots that is generated by the updated file
            var newDetailSlots = _BuildInventoryDetailSlots(daypartCode, flightWeeks, rateSource, spotLengths);
            foreach (var inventorySlot in newDetailSlots)
            {
                // set the id for the existing slots, everything else will be added or removed
                var slot =
                    inventory.InventoryDetailSlots.Find(
                        w =>
                            w.MediaWeekId == inventorySlot.MediaWeekId &&
                            w.DetailLevel == inventorySlot.DetailLevel &&
                            w.SpotLengthId == inventorySlot.SpotLengthId);
                if (slot == null) continue;

                inventorySlot.Id = slot.Id;
                updatedSlotIds.Add(inventorySlot.Id);
            }

            // add updated inventory slots 
            inventory.InventoryDetailSlots.Clear();
            inventory.InventoryDetailSlots.AddRange(newDetailSlots);

            return updatedSlotIds;
        }

        private List<InventoryDetailSlot> _BuildInventoryDetailSlots(string dayPartCode, List<FlightWeekDto> flights,
            RatesFile.RateSourceType? rateSource, List<int> spotLengthsList)
        {
            var inventorySlots = new List<InventoryDetailSlot>();
            var spotLengths = _SpotLengthRepository.GetSpotLengthAndIds();

            foreach (var mediaWeek in flights)
            {
                if (mediaWeek.IsHiatus) continue; // no data to crunch

                // loop through spot lengths that were in the file
                foreach (var spotLength in spotLengthsList)
                {
                    //Taking all the programs that match the daypart(s) included in the file, group by daypart, week, station and find maximum spots. 
                    //That will be the maximum number of spots available.
                    var maximumSpots = _InventoryRepository.GetMaximunNumberOfSpotsAvailableByDaypartCode(dayPartCode,
                        rateSource, mediaWeek.Id, spotLengths[spotLength]);

                    // loop through maximum spots found
                    for (var spot = maximumSpots; spot > 0; spot--)
                    {
                        // get stations programs that have same or more spots at slot level
                        var stationProgramsPerSpot = _InventoryRepository.GetStationProgramsPerSpotAvailable(
                            spot, mediaWeek.Id, rateSource, spotLengths[spotLength], dayPartCode);

                        _CheckIfAllProgramsInSlotHaveTheSameFixedPrice(stationProgramsPerSpot);

                        var spotCost = _CalculateSpotCost(stationProgramsPerSpot, spotLength, mediaWeek);

                        var hasStationsProgramsPerSpot = stationProgramsPerSpot != null && stationProgramsPerSpot.Any();
                        var detailSlot = new InventoryDetailSlot
                        {
                            SpotLengthId = spotLengths[spotLength],
                            DetailLevel = spot,
                            MediaWeekId = mediaWeek.Id,
                            SlotCost = spotCost,
                            TotalStations =
                                hasStationsProgramsPerSpot
                                    ? stationProgramsPerSpot.Select(a => a.StationCode).Distinct().Count()
                                    : 0,
                            RolleupDaypartId =
                                hasStationsProgramsPerSpot
                                    ? _SetRolledUpDaypartId(stationProgramsPerSpot.Select(a => a.Daypart.Id).ToList())
                                    : 0,
                            InventoryDetailSlotComponents =
                                hasStationsProgramsPerSpot
                                    ? _GetInventoryDetailSlotComponents(stationProgramsPerSpot, mediaWeek.Id)
                                    : new List<InventoryDetailSlotComponents>()
                        };

                        inventorySlots.Add(detailSlot);
                    }
                }
            }

            return inventorySlots;
        }

        private void _CheckIfAllProgramsInSlotHaveTheSameFixedPrice(IEnumerable<StationProgram> stationProgramsPerSpot)
        {
            var fixedPrices = stationProgramsPerSpot.GroupBy(station => new { station.FixedPrice });
            if (fixedPrices.Count() <= 1)
                return;
            throw new Exception("One or more programs have multiple fixed costs for the same inventory slot, please correct the error");
        }

        private decimal? _CalculateSpotCost(List<StationProgram> stationProgramsPerSpot, int slotSpotLength, FlightWeekDto mediaWeek)
        {
            var firstStationProgram = stationProgramsPerSpot.FirstOrDefault();

            decimal? spotCost;

            if (firstStationProgram != null)
            {
                spotCost = firstStationProgram.FixedPrice;

                if (spotCost != null)
                    return spotCost;
            }

            var flightWeeks = stationProgramsPerSpot.SelectMany(station => station.FlightWeeks);
            var flightsForWeek = flightWeeks.Where(flightWeek => flightWeek.FlightWeek.Id == mediaWeek.Id);

            switch (slotSpotLength)
            {
                case 15:
                    spotCost = flightsForWeek.Sum(flightWeek => flightWeek.Rate15s);
                    break;
                case 30:
                    spotCost = flightsForWeek.Sum(flightWeek => flightWeek.Rate30s);
                    break;
                case 60:
                    spotCost = flightsForWeek.Sum(flightWeek => flightWeek.Rate60s);
                    break;
                case 90:
                    spotCost = flightsForWeek.Sum(flightWeek => flightWeek.Rate90s);
                    break;
                case 120:
                    spotCost = flightsForWeek.Sum(flightWeek => flightWeek.Rate120s);
                    break;
                default:
                    spotCost = flightsForWeek.Sum(flightWeek => flightWeek.Rate30s);
                    break;
            }

            return spotCost;
        }

        private int _SetRolledUpDaypartId(List<int> lisOfDaypartIds)
        {
            var unionDisplayDaypart =
                DisplayDaypart.Union(lisOfDaypartIds.Select(id => _DaypartCache.GetDisplayDaypart(id)).ToArray());
            return unionDisplayDaypart == null ? 0 : _DaypartCache.GetIdByDaypart(unionDisplayDaypart);
        }

        private List<InventoryDetailSlotComponents> _GetInventoryDetailSlotComponents(List<StationProgram> stationPrograms, int mediaWeekId)
        {
            return stationPrograms.Select(program => new InventoryDetailSlotComponents()
            {
                DaypartId = program.Daypart.Id,
                StationProgramFlightId = program.FlightWeeks.Single(p => p.FlightWeek.Id == mediaWeekId).Id,
                StationCode = program.StationCode
            }).ToList();
        }


        public InventoryDetail GetInventoryDetailByDaypartCodeAndRateSource(string daypartCode,
            RatesFile.RateSourceType rateSource)
        {
            return _InventoryRepository.GetInventoryDetailByDaypartCodeAndRateSource(daypartCode, rateSource);
        }
    }

}
