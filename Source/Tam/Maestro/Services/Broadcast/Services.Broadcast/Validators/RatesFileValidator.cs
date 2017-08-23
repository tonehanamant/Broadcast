using Common.Services.Repositories;
using Services.Broadcast.Entities;
using Services.Broadcast.Exceptions;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.Validators
{
    public interface IRatesFileValidator
    {
        RatesFileValidatorResult ValidateRatesFile(RatesFile ratesFile);
    }

    public class RatesFileValidator : IRatesFileValidator
    {
        private readonly IStationRepository _stationRepository;
        private readonly IStationProgramRepository _stationProgramRepository;
        private readonly IRatesRepository _ratesRepository;
        private readonly List<StationProgram> _invalidPrograms = new List<StationProgram>();
        private readonly Dictionary<int, int> _SpotLengthMap; 

        public RatesFileValidator(IDataRepositoryFactory dataRepositoryFactory)
        {
            _stationRepository = dataRepositoryFactory.GetDataRepository<IStationRepository>();
            _ratesRepository = dataRepositoryFactory.GetDataRepository<IRatesRepository>();
            _stationProgramRepository = dataRepositoryFactory.GetDataRepository<IStationProgramRepository>();
            _SpotLengthMap = dataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthAndIds();
        }

        public RatesFileValidatorResult ValidateRatesFile(RatesFile ratesFile)
        {
            var validatorResult = new RatesFileValidatorResult();
            var fileProblems = new List<RatesFileProblem>();
            
            ValidateFileDates(ratesFile);
            var stationProblems = ValidateStations(ratesFile);
            fileProblems.AddRange(stationProblems);

            var flightProblems = ValidateFlights(ratesFile);            
            fileProblems.AddRange(flightProblems);

            var rateProblems = ValidateProgramsWithoutRates(ratesFile);
            fileProblems.AddRange(rateProblems);            

            var duplicateRatesProblems = ValidateDuplicateRates(ratesFile);
            fileProblems.AddRange(duplicateRatesProblems);
            
            validatorResult.RatesFileProblems.AddRange(fileProblems);
            validatorResult.InvalidRates.AddRange(_invalidPrograms);

            return validatorResult;
        }

        private List<RatesFileProblem> ValidateDuplicateRates(RatesFile incomingRatesFile)
        {
            var duplicateRates = new List<RatesFileProblem>();
            foreach (var incomingProgram in incomingRatesFile.StationPrograms.ToList())
            {
                // ttnw, cnn and tvb have a different approach to what is done to the duplicated rates (it does update the inventory when there is  match criteria).
                if (IsThirdPartyRateSource(incomingRatesFile.RateSource))
                    continue;

                if (ExistsMatchingProgramInDatabase(incomingProgram) || HasDuplicateIncomingPrograms(incomingProgram, incomingRatesFile))
                { 
                    _invalidPrograms.Add(incomingProgram);
                    duplicateRates.Add(new RatesFileProblem()
                    {
                        ProblemDescription = String.Format("Program with duplicate rates"),
                        ProgramName = incomingProgram.ProgramName,
                        StationLetters = incomingProgram.StationLegacyCallLetters
                    });
                }
            }

            return duplicateRates;
        }

        private bool HasDuplicateIncomingPrograms(StationProgram incomingProgram, RatesFile incomingRatesFile)
        {
            var count = incomingRatesFile.StationPrograms.Count(
                p => 
                    incomingProgram.ProgramName.Equals(p.ProgramName, StringComparison.InvariantCultureIgnoreCase)
                    && incomingProgram.Daypart.Id == p.Daypart.Id
                    && incomingProgram.StartDate.Equals(p.StartDate)
                    && incomingProgram.EndDate.Equals(p.EndDate)
                    && incomingProgram.StationCode == p.StationCode);

            return count > 1;
        }

        private bool ExistsMatchingProgramInDatabase(StationProgram incomingProgram)
        {
            var matchingPrograms = _stationProgramRepository.GetStationProgramsByNameDaypartFlight(
                                        incomingProgram.ProgramName,
                                        incomingProgram.Daypart.Id,
                                        incomingProgram.StartDate,
                                        incomingProgram.EndDate);

            foreach (var matchingProgram in matchingPrograms)
            {
                if (AllFlightsMatch(incomingProgram.FlightWeeks, matchingProgram.FlightWeeks))
                {
                    return true;
                }
            }

            return false;
        }

        private bool AllFlightsMatch(List<StationProgramFlightWeek> incomingFlights, List<StationProgramFlightWeek> existingFlights)
        {
            return incomingFlights.Count == existingFlights.Count
                   && !incomingFlights.Except(existingFlights, new StationProgramFlightWeek.StationProgramFlightWeekEqualityComparer()).Any();
        }


        private List<RatesFileProblem> ValidateProgramsWithoutRates(RatesFile ratesFile)
        {
            var fileProblems = new List<RatesFileProblem>();

            foreach (var stationProgram in ratesFile.StationPrograms.ToList())
            {
                var removeProgram = false;
                if (stationProgram.FlightWeeks.Count == 0)
                {
                    removeProgram = true;
                    fileProblems.Add(new RatesFileProblem()
                    {
                        ProblemDescription = String.Format("Program without valid flights"),
                        ProgramName = stationProgram.ProgramName,
                        StationLetters = stationProgram.StationLegacyCallLetters
                    });
                }

                foreach (var stationProgramFlightWeek in stationProgram.FlightWeeks.ToList())
                {
                    // tvb and cnn files don´t have rates
                    if (IsThirdPartyRateSource(ratesFile.RateSource))
                        continue;
                    
                    if (AreAllFlightRatesEmpty(stationProgramFlightWeek))
                    {
                        removeProgram = true;
                        fileProblems.Add(new RatesFileProblem()
                        {
                            ProgramName = stationProgram.ProgramName,
                            StationLetters = stationProgram.StationLegacyCallLetters,
                            ProblemDescription = string.Format("Program with missing rate")
                        });
                        break;
                    }
                }

                if (removeProgram)
                {
                    //ratesFile.StationPrograms.Remove(stationProgram);
                    _invalidPrograms.Add(stationProgram);
                }
            }

            return fileProblems;
        }

        private bool IsThirdPartyRateSource(RatesFile.RateSourceType rateSource)
        {
            return rateSource == RatesFile.RateSourceType.TVB ||
                   rateSource == RatesFile.RateSourceType.CNN ||
                   rateSource == RatesFile.RateSourceType.TTNW;
        }

        private static bool AreAllFlightRatesEmpty(StationProgramFlightWeek f)
        {
            return (f.Rate15s == null || f.Rate15s == 0)
                    && (f.Rate30s == null || f.Rate30s == 0)
                    && (f.Rate60s == null || f.Rate60s == 0)
                    && (f.Rate90s == null || f.Rate90s == 0)
                    && (f.Rate120s == null || f.Rate120s == 0);
        }

        private void ValidateFileDates(RatesFile ratesFile)
        {
            if (ratesFile.StartDate == null || ratesFile.EndDate == null || ratesFile.StartDate > ratesFile.EndDate)
            {
                throw new BroadcastRatesFileValidationException(string.Format("Invalid rates file dates: {0} - {1}", ratesFile.StartDate, ratesFile.EndDate));
            }
        }

        private List<RatesFileProblem> ValidateStations(RatesFile ratesFile)
        {
            var fileStationLettersList = ratesFile.StationPrograms.Select(p => p.StationLegacyCallLetters).Distinct().ToList();
            var unknownStationList = new List<string>();
            foreach (var fileStationLetters in fileStationLettersList)
            {
                var station = _stationRepository.GetBroadcastStationByLegacyCallLetters(fileStationLetters);
                if (station == null)
                {
                    unknownStationList.Add(fileStationLetters);
                }
            }

            if (fileStationLettersList.Count == unknownStationList.Count)
            {
                throw new BroadcastRatesFileValidationException(
                    string.Format(
                        "Unable to load a rates file with all unknown stations: {0}",
                        string.Join(",", fileStationLettersList)));
            }

            var stationProblems = new List<RatesFileProblem>();

            var unknownStationPrograms =
                ratesFile.StationPrograms.Where(
                    p =>
                        unknownStationList.Contains(p.StationLegacyCallLetters)).ToList();

            //Add list of issues fore each problematic record
            stationProblems.AddRange(unknownStationPrograms.Select(sp => new RatesFileProblem()
            {
                ProblemDescription = string.Format("Unknown station with legacy call letters: {0}", sp.StationLegacyCallLetters),
                ProgramName = sp.ProgramName,
                StationLetters = sp.StationLegacyCallLetters
            }).ToList());

            //Remove problematic records from the list of data to be loaded
            foreach (var unknownStationProgram in unknownStationPrograms.ToList())
            {
                //ratesFile.StationPrograms.Remove(unknownStationProgram);
                _invalidPrograms.Add(unknownStationProgram);
            }           

            return stationProblems;
        }

        private List<RatesFileProblem> ValidateFlights(RatesFile ratesFile)
        {
            var fileProblems = new List<RatesFileProblem>();
            foreach (var stationProgram in ratesFile.StationPrograms.ToList())
            {
                if (stationProgram.FlightWeeks.GroupBy(f => f.FlightWeek.Id).Any(g => g.Count() > 1))
                {
                    //ratesFile.StationPrograms.Remove(stationProgram);
                    _invalidPrograms.Add(stationProgram);
                    fileProblems.Add(new RatesFileProblem()
                    {
                        ProblemDescription = String.Format("Program with overlapping week"),
                        ProgramName = stationProgram.ProgramName,
                        StationLetters = stationProgram.StationLegacyCallLetters
                    });
                }
            }

            return fileProblems;
        }
    }
}
