using Common.Services.Extensions;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Data.EntityFrameworkMapping;
using Tam.Maestro.Services.Clients;
using Tam.Maestro.Services.ContractInterfaces.AudienceAndRatingsBusinessObjects;
using Tam.Maestro.Services.ContractInterfaces.Common;
using System.Transactions;

namespace Services.Broadcast.Repositories
{
    public interface IStationProgramRepository : IDataRepository
    {
        station_programs GetStationProgramById(int stationProgramId);
        List<StationProgram> GetStationProgramsWithPrimaryAudienceRatesByStationCode(RatesFile.RateSourceType rateSource, int stationCode);
        int CreateStationProgramRate(DisplayBroadcastStation station, StationProgram newProgram, RatesFile.RateSourceType rateSource, int spotLengthId, string userName, int? rateFileId);
        void DeleteProgramRates(int programId, int startWeek, int endWeek, string userName, IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache);
        void UpdateProgramRate(int programId, StationProgramAudienceRateDto programRate, string userName, int startWeek, int endWeek);
        List<StationProgram> GetStationProgramsByNameDaypartFlight(string programName, int daypartId, DateTime startDate, DateTime endDate);
        List<StationProgram> GetStationProgramsWithPrimaryAudienceRatesByStationCodeAndDates(RatesFile.RateSourceType rateSource, int stationCode, DateTime startDateValue, DateTime endDateValue);
        void TrimProgramFlight(int programId, List<MediaWeek> programFlightWeeksToRemove, DateTime? startDate, DateTime endDate, string userName);
        void UpdateProgramFlights(int programId, List<FlightWeekDto> flights, string userName);
        List<station_program_flights> GetStationProgramFlightsById(int stationProgramId);
        station_program_flights GetStationProgramFlightByProgramAndWeek(int stationProgramId, int mediaWeekId);
        void AddRateFilePrograms(RatesFile ratesFile, string userName, Dictionary<int, int> spotLengthMap);
        bool RemoveStationProgramGenres(int programId);
        void AddStationProgramGenres(int programId, List<LookupDto> programGenres);
        void UpdateStationProgram(int programId, DateTime timeStamp, string userName);
        void UpdateStationProgram(int programId, DateTime timeStamp, string userName, decimal? fixedPrice);int GetStationProgramIdByStationProgram(StationProgram program, int spotLengthId);
        void UpdateStationProgramFlights(int programId, StationProgram stationProgram, string userName, int? rateFileId);
        void DeleteStationProgramRates(int programId, string userName);
        void UpdateFlightWeekSpot(int programId, StationProgramFlightWeek flightWeek, DateTime currentTime,
            string userName);
        bool ProgramFlightAudienceExists(int stationProgramFlightId, int audienceId);
        void UpdateProgramFligthAudienceInventory(int programId, int mediaWeekId, int spotLength, StationProgramFlightWeekAudience flightWeekAudience);
        void AddProgramFligthAudienceInventory(int programId, int mediaWeekId, StationProgramFlightWeekAudience flightWeekAudience, string userName, DateTime currentTime);

        List<ProposalProgramDto> GetStationProgramsForProposalDetail(DateTime flightStart, DateTime flightEnd, int spotLength, int rateSource, List<int> proposalMarketIds, int proposalDetailId);
    }

    public class StationProgramRepository : BroadcastRepositoryBase, IStationProgramRepository
    {

        public StationProgramRepository(ISMSClient pSmsClient, IContextFactory<QueryHintBroadcastContext> pBroadcastContextFactory,
            ITransactionHelper pTransactionHelper)
            : base(pSmsClient, pBroadcastContextFactory, pTransactionHelper)
        {
        }

        public void AddRateFilePrograms(RatesFile ratesFile, string userName, Dictionary<int, int> spotLengthMap)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    context.Configuration.ValidateOnSaveEnabled = false;
                    context.Configuration.AutoDetectChangesEnabled = false;
                    System.Diagnostics.Debug.WriteLine(string.Format("==> Timeout set to {0}", context.Database.CommandTimeout));
                    System.Diagnostics.Debug.WriteLine(
                        string.Format(
                            "Adding {0} programs for rate file {1}.",
                            ratesFile.StationPrograms.Count,
                            ratesFile.FileName));
                    int batchSize = 0;
                    //context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s); //Only for debugging EF generate SQL statements
                    var startTime = DateTime.Now;
                    var programList = new List<station_programs>();
                    foreach (var program in ratesFile.StationPrograms)
                    {
                        var start = DateTime.Now;
                        var stationProgram = BuildStationProgram(context, program, ratesFile.Id, ratesFile.RateSource, userName, spotLengthMap[program.SpotLength]);
                        var end = DateTime.Now;
                        //System.Diagnostics.Debug.WriteLine(string.Format("Build program in {0}", end-start));
                        programList.Add(stationProgram);
                        batchSize++;

                        if (batchSize % 100 == 0)
                        {
                            start = DateTime.Now;
                            context.station_programs.AddRange(programList);
                            context.SaveChanges();
                            context.Dispose();
                            context = CreateMeastroDBContext(true);
                            programList = new List<station_programs>();
                            end = DateTime.Now;

                            System.Diagnostics.Debug.WriteLine(string.Format("Save changes for batch {0} in {1}", batchSize, end - start));
                            System.Diagnostics.Debug.WriteLine(string.Format("Elapsed time: {0}", DateTime.Now - startTime));
                        }

                    }
                    if (programList.Count > 0)
                    {
                        context.station_programs.AddRange(programList);
                        context.SaveChanges();
                    }

                    var inserStart = DateTime.Now;
                    //BulkInsert(context, programList);
                    System.Diagnostics.Debug.WriteLine(string.Format("Completed bulk insert in {0}", DateTime.Now - inserStart));
                    System.Diagnostics.Debug.WriteLine(string.Format("Completed program save in {0}", DateTime.Now - startTime));

                });

        }

        public station_programs GetStationProgramById(int stationProgramId)
        {
            return _InReadUncommitedTransaction(
                context =>
                    context.station_programs.Single(
                        q => q.id == stationProgramId,
                        string.Format("Could not find program {0}.", stationProgramId)));
        }

        public List<StationProgram> GetStationProgramsWithPrimaryAudienceRatesByStationCode(RatesFile.RateSourceType rateSource, int stationCode)
        {

            return _InReadUncommitedTransaction(
                context => (
                    from sp in context.station_programs
                    join g in context.genres on sp.genres.FirstOrDefault().id equals g.id into genres
                    from g in genres.DefaultIfEmpty()
                    where sp.station_code == (short)stationCode
                    && sp.rate_source == (byte)rateSource
                    && (sp.rate_files.status == (byte)RatesFile.FileStatusEnum.Loaded || sp.rate_file_id == null)

                    select new StationProgram()
                    {
                        Id = sp.id,
                        Daypart = new DisplayDaypart() { Id = sp.daypart_id },
                        StartDate = sp.start_date,
                        EndDate = sp.end_date,
                        ProgramName = sp.program_name,
                        DaypartCode = sp.daypart_code,
                        Genres = sp.genres.Select(genre => new LookupDto()
                        {
                            Id = genre.id,
                            Display = genre.name
                        }).ToList(),
                        FlightWeeks = sp.station_program_flights.Select(fw => new StationProgramFlightWeek()
                        {
                            FlightWeek = new DisplayMediaWeek() { Id = fw.media_week_id },
                            Active = fw.active,
                            Rate15s = fw.C15s_rate,
                            Rate30s = fw.C30s_rate,
                            Rate60s = fw.C60s_rate,
                            Rate90s = fw.C90s_rate,
                            Rate120s = fw.C120s_rate,
                            Spots = fw.spots,
                            Audiences = fw.station_program_flight_audiences
                                .Select(a => new StationProgramFlightWeekAudience()
                                {
                                    Audience = new DisplayAudience() { Id = a.audience_id },
                                    Rating = a.rating,
                                    Impressions = a.impressions,
                                    Cpm120 = a.cpm120,
                                    Cpm15 = a.cpm15,
                                    Cpm30 = a.cpm30,
                                    Cpm60 = a.cpm60,
                                    Cpm90 = a.cpm90
                                }).ToList()
                        }).ToList()
                    }).ToList());
        }

        public void DeleteProgramRates(int programId, int startWeek, int endWeek, string userName, IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var programFlightList = context.station_program_flights
                        .Where(
                            pf =>
                                pf.station_program_id == programId && pf.media_week_id >= startWeek &&
                                pf.media_week_id <= endWeek).ToList();
                    foreach (var programFlight in programFlightList)
                    {
                        context.station_program_flight_audiences.RemoveRange(
                            programFlight.station_program_flight_audiences);
                        context.station_program_flights.Remove(programFlight);
                    }
                    //Remove program if no more flights
                    var program = context.station_programs.Single(p => p.id == programId);
                    if (program.station_program_flights.Count == 0)
                    {
                        context.station_programs.Remove(program);
                    }
                    else //or update start and end dates for the program flight
                    {
                        program.start_date = mediaMonthAndWeekAggregateCache.GetMediaWeekById(program.station_program_flights.Min(f => f.media_week_id)).StartDate;
                        program.end_date = mediaMonthAndWeekAggregateCache.GetMediaWeekById(program.station_program_flights.Max(f => f.media_week_id)).EndDate;
                    }

                    context.SaveChanges();
                });
        }

        public void DeleteStationProgramRates(int programId, string userName)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var program = context.station_programs.Single(pf => pf.id == programId);

                    foreach (var flight in program.station_program_flights)
                    {
                        context.station_program_flight_audiences.RemoveRange(flight.station_program_flight_audiences);
                    }

                    context.station_program_flights.RemoveRange(program.station_program_flights);

                    program.modified_by = userName;
                    program.modified_date = DateTime.Now;

                    context.SaveChanges();
                });
        }

        public int CreateStationProgramRate(DisplayBroadcastStation station, StationProgram newProgram, RatesFile.RateSourceType rateSource, int spotLengthId, string userName, int? rateFileId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var stationProgram = BuildStationProgram(context, newProgram, rateFileId, rateSource, userName, spotLengthId);

                    context.station_programs.Add(stationProgram);

                    context.SaveChanges();

                    return stationProgram.id;
                });
        }

        public void UpdateProgramRate(int programId, StationProgramAudienceRateDto programRate, string userName, int startWeek, int endWeek)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var programRateList =
                        context.station_program_flights
                            .Where(pf => pf.station_program_id == programId && pf.media_week_id >= startWeek &&
                                pf.media_week_id <= endWeek).ToList();

                    // update each program rate and audience data
                    foreach (var pr in programRateList)
                    {
                        if (pr.C15s_rate != programRate.Rate15 || pr.C30s_rate != programRate.Rate30)
                        {
                            pr.C15s_rate = programRate.Rate15;
                            pr.C30s_rate = programRate.Rate30;
                            pr.modified_by = userName;
                            pr.modified_date = DateTime.Now;
                        }

                        foreach (var a in pr.station_program_flight_audiences.Where(fa => fa.audience_id == programRate.AudienceId))
                        {
                            if (a.impressions != programRate.Impressions || a.rating != programRate.Rating)
                            {
                                a.impressions = programRate.Impressions;
                                a.rating = programRate.Rating;
                                a.modified_by = userName;
                                a.modified_date = DateTime.Now;
                            }
                        }
                    }

                    context.SaveChanges();
                });
        }


        public void UpdateStationProgramFlights(int programId, StationProgram stationProgram, string userName, int? rateFileId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var program =
                        context.station_programs
                            .Single(pf => pf.id == programId);

                    program.rate_file_id = rateFileId;
                    program.fixed_price = stationProgram.FixedPrice;

                    var flights = BuildStationProgramFlights(context, stationProgram, userName);
                    foreach (var f in flights)
                    {
                        program.station_program_flights.Add(f);
                    }

                    context.SaveChanges();
                });
        }

        public void UpdateFlightWeekSpot(int programId, StationProgramFlightWeek flightWeek, DateTime currentTime, string userName)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var programFlight =
                        context.station_program_flights
                            .Single(pf => pf.station_program_id == programId && pf.media_week_id == flightWeek.FlightWeek.Id);

                    programFlight.spots = flightWeek.Spots;
                    programFlight.C15s_rate = flightWeek.Rate15s;
                    programFlight.C30s_rate = flightWeek.Rate30s;
                    programFlight.C60s_rate = flightWeek.Rate60s;
                    programFlight.C90s_rate = flightWeek.Rate90s;
                    programFlight.C120s_rate = flightWeek.Rate120s;
                    programFlight.modified_by = userName;
                    programFlight.modified_date = currentTime;
                    context.SaveChanges();
                });
        }

        public bool ProgramFlightAudienceExists(int stationProgramFlightId, int audienceId)
        {
            return _InReadUncommitedTransaction(
                 context => context.station_program_flight_audiences.Any(
                     a =>
                         a.station_program_flight_id == stationProgramFlightId && a.audience_id == audienceId));
        }

        public void UpdateProgramFligthAudienceInventory(int programId, int mediaWeekId, int spotLength, StationProgramFlightWeekAudience flightWeekAudience)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var stationProgramFlight =
                        context.station_program_flights.Single(
                            q => q.station_program_id == programId && q.media_week_id == mediaWeekId);

                    var programFlightAudience =
                        context.station_program_flight_audiences.Single(
                            a =>
                                a.station_program_flight_id == stationProgramFlight.id &&
                                a.audience_id == flightWeekAudience.Audience.Id);

                    // can import files that have same programs etc but only change the cpm for an audience
                    // this allows import files where every entry has a different spot length
                    switch (spotLength)
                    {
                        case 15:
                            programFlightAudience.cpm15 = flightWeekAudience.Cpm15;
                            break;
                        case 30:
                            programFlightAudience.cpm30 = flightWeekAudience.Cpm30;
                            break;
                        case 60:
                            programFlightAudience.cpm60 = flightWeekAudience.Cpm60;
                            break;
                        case 90:
                            programFlightAudience.cpm90 = flightWeekAudience.Cpm90;
                            break;
                        case 120:
                            programFlightAudience.cpm120 = flightWeekAudience.Cpm120;
                            break;
                    }

                    context.SaveChanges();
                });
        }

        public void AddProgramFligthAudienceInventory(int programId, int mediaWeekId,
            StationProgramFlightWeekAudience flightWeekAudience, string userName, DateTime currentTime)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var stationProgramFlight =
                        context.station_program_flights.First(
                            q => q.station_program_id == programId && q.media_week_id == mediaWeekId);

                    var programFlightAudience = new station_program_flight_audiences
                    {
                        station_program_flight_id = stationProgramFlight.id,
                        audience_id = flightWeekAudience.Audience.Id,
                        impressions = flightWeekAudience.Impressions,
                        rating = flightWeekAudience.Rating,
                        cpm120 = flightWeekAudience.Cpm120,
                        cpm15 = flightWeekAudience.Cpm15,
                        cpm30 = flightWeekAudience.Cpm30,
                        cpm60 = flightWeekAudience.Cpm60,
                        cpm90 = flightWeekAudience.Cpm90,
                        created_by = userName,
                        created_date = currentTime,
                        modified_by = userName,
                        modified_date = currentTime
                    };

                    context.station_program_flight_audiences.Add(programFlightAudience);
                    context.SaveChanges();
                });
        }


        private station_programs BuildStationProgram
            (QueryHintBroadcastContext context, StationProgram program, int? rateFileId, RatesFile.RateSourceType rateSource, string userName, int SpotLengthId)
        {
            var timestamp = DateTime.Now;
            var stationProgram = new station_programs()
            {
                program_name = program.ProgramName,
                station_code = program.StationCode,
                created_by = userName,
                created_date = timestamp,
                modified_by = userName,
                modified_date = timestamp,
                daypart_id = program.Daypart.Id,
                daypart_name = program.DayPartName,
                start_date = program.StartDate,
                end_date = program.EndDate,
                rate_file_id = rateFileId,
                rate_source = (byte)rateSource,
                daypart_code = program.DaypartCode,
                spot_length_id = SpotLengthId,
                fixed_price = program.FixedPrice,
                station_program_flights = BuildStationProgramFlights(context, program, userName)
            };

            return stationProgram;
        }

        private ICollection<station_program_flights> BuildStationProgramFlights
            (QueryHintBroadcastContext context, StationProgram program, string userName)
        {
            List<station_program_flights> programFlights = new List<station_program_flights>();

            foreach (var flight in program.FlightWeeks)
            {
                var timestamp = DateTime.Now;
                var programFlight = new station_program_flights()
                {
                    media_week_id = flight.FlightWeek.Id,
                    active = flight.Active,
                    C15s_rate = flight.Rate15s,
                    C30s_rate = flight.Rate30s,
                    C60s_rate = flight.Rate60s,
                    C90s_rate = flight.Rate90s,
                    C120s_rate = flight.Rate120s,
                    created_by = userName,
                    created_date = timestamp,
                    modified_by = userName,
                    modified_date = timestamp,
                    spots = flight.Spots,
                    station_program_flight_audiences = BuildStationProgramFlightAudiences(context, program, flight, userName)
                };
                programFlights.Add(programFlight);
            }

            return programFlights;
        }

        private ICollection<station_program_flight_audiences> BuildStationProgramFlightAudiences
            (QueryHintBroadcastContext context, StationProgram program, StationProgramFlightWeek flight, string userName)
        {
            List<station_program_flight_audiences> flightAudiences = new List<station_program_flight_audiences>();

            foreach (var audience in flight.Audiences)
            {
                var timestamp = DateTime.Now;
                var flightAudience = new station_program_flight_audiences()
                {
                    audience_id = audience.Audience.Id,
                    impressions = audience.Impressions,
                    rating = audience.Rating,
                    created_by = userName,
                    created_date = timestamp,
                    modified_by = userName,
                    modified_date = timestamp,
                    cpm120 = audience.Cpm120,
                    cpm15 = audience.Cpm15,
                    cpm30 = audience.Cpm30,
                    cpm60 = audience.Cpm60,
                    cpm90 = audience.Cpm90
                };
                flightAudiences.Add(flightAudience);
            }

            return flightAudiences;
        }

        public List<StationProgram> GetStationProgramsByNameDaypartFlight(string programName, int daypartId, DateTime startDate, DateTime endDate)
        {
            return _InReadUncommitedTransaction(
                context => (
                    from sp in context.station_programs
                    where sp.program_name == programName
                    && sp.daypart_id == daypartId
                    && sp.start_date == startDate
                    && sp.end_date == endDate
                    && sp.rate_files.status == (byte)RatesFile.FileStatusEnum.Loaded
                    select new StationProgram()
                    {
                        Id = sp.id,
                        StationCode = sp.station_code,
                        Daypart = new DisplayDaypart() { Id = sp.daypart_id },
                        StartDate = sp.start_date,
                        EndDate = sp.end_date,
                        ProgramName = sp.program_name,
                        DaypartCode = sp.daypart_code,
                        Genres = sp.genres.Select(g => new LookupDto()
                        {
                            Id = g.id,
                            Display = g.name
                        }).ToList(),
                        FlightWeeks = sp.station_program_flights.Select(fw => new StationProgramFlightWeek()
                        {
                            FlightWeek = new DisplayMediaWeek() { Id = fw.media_week_id },
                            Active = fw.active,
                            Rate15s = fw.C15s_rate,
                            Rate30s = fw.C30s_rate,
                            Rate60s = fw.C60s_rate,
                            Rate90s = fw.C90s_rate,
                            Rate120s = fw.C120s_rate,
                            Spots = fw.spots,
                            Audiences = fw.station_program_flight_audiences
                                .Select(a => new StationProgramFlightWeekAudience()
                                {
                                    Audience = new DisplayAudience() { Id = a.audience_id },
                                    Rating = a.rating,
                                    Impressions = a.impressions,
                                    Cpm120 = a.cpm120,
                                    Cpm15 = a.cpm15,
                                    Cpm30 = a.cpm30,
                                    Cpm60 = a.cpm60,
                                    Cpm90 = a.cpm90
                                }).ToList()
                        }).ToList(),

                    }).ToList());

        }

        public List<StationProgram> GetStationProgramsWithPrimaryAudienceRatesByStationCodeAndDates(
            RatesFile.RateSourceType rateSource, int stationCode, DateTime startDateValue, DateTime endDateValue)
        {
            return _InReadUncommitedTransaction(
                context => (
                    from sp in context.station_programs
                    where sp.station_code == (short)stationCode
                    && sp.rate_source == (byte)rateSource
                    && (sp.rate_files.status == (byte)RatesFile.FileStatusEnum.Loaded || sp.rate_file_id == null)
                    && ((sp.start_date >= startDateValue && sp.start_date <= endDateValue)
                        || (sp.end_date >= startDateValue && sp.end_date <= endDateValue)
                        || (sp.start_date < startDateValue && sp.end_date > endDateValue))
                    select new StationProgram()
                    {
                        Id = sp.id,
                        StationCode = sp.station_code,
                        Daypart = new DisplayDaypart() { Id = sp.daypart_id },
                        StartDate = sp.start_date,
                        EndDate = sp.end_date,
                        ProgramName = sp.program_name,
                        DaypartCode = sp.daypart_code,
                        Genres = sp.genres.Select(g => new LookupDto()
                        {
                            Id = g.id,
                            Display = g.name
                        }).ToList(),
                        FlightWeeks = sp.station_program_flights.Select(fw => new StationProgramFlightWeek()
                        {
                            FlightWeek = new DisplayMediaWeek() { Id = fw.media_week_id },
                            Active = fw.active,
                            Rate15s = fw.C15s_rate,
                            Rate30s = fw.C30s_rate,
                            Rate60s = fw.C60s_rate,
                            Rate90s = fw.C90s_rate,
                            Rate120s = fw.C120s_rate,
                            Spots = fw.spots,
                            Audiences = fw.station_program_flight_audiences
                                .Select(a => new StationProgramFlightWeekAudience()
                                {
                                    Audience = new DisplayAudience() { Id = a.audience_id },
                                    Rating = a.rating,
                                    Impressions = a.impressions,
                                    Cpm120 = a.cpm120,
                                    Cpm15 = a.cpm15,
                                    Cpm30 = a.cpm30,
                                    Cpm60 = a.cpm60,
                                    Cpm90 = a.cpm90
                                }).ToList()
                        }).ToList()
                    }).ToList());
        }

        public List<station_program_flights> GetStationProgramFlightsById(int stationProgramId)
        {
            return _InReadUncommitedTransaction(
                context => context.station_program_flights.Where(q => q.station_program_id == stationProgramId).ToList());
        }

        public station_program_flights GetStationProgramFlightByProgramAndWeek(int stationProgramId, int mediaWeekId)
        {
            return _InReadUncommitedTransaction(
                context => context.station_program_flights.Single(q => q.station_program_id == stationProgramId && q.media_week_id == mediaWeekId));
        }

        public void TrimProgramFlight(int programId, List<MediaWeek> programFlightWeeksToRemove, DateTime? startDate, DateTime endDate, string userName)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    //set the end date
                    var programToUpdate = (from c in context.station_programs
                                           where c.id == programId
                                           select c).Single(string.Format("Could not find program {0}.", programId));

                    // remove the unusable flightweeks
                    if (programFlightWeeksToRemove.Any())
                    {
                        foreach (var mediaWeek in programFlightWeeksToRemove)
                        {
                            var programFlight = (from c in context.station_program_flights
                                                 where c.media_week_id == mediaWeek.Id && c.station_program_id == programId
                                                 select c).Single(string.Format("Could not find program {0} with flight ending {1}.", programId, mediaWeek.EndDate));

                            if (programFlight.station_program_flight_audiences != null &&
                                programFlight.station_program_flight_audiences.Any())
                            {
                                context.station_program_flight_audiences.RemoveRange(
                                                                programFlight.station_program_flight_audiences);
                            }
                            // cnn programs imported from files might not have audiences
                            context.station_program_flights.Remove(programFlight);
                        }
                    }

                    if (startDate != null)
                    {
                        programToUpdate.start_date = (DateTime)startDate;
                    }

                    programToUpdate.end_date = endDate;
                    programToUpdate.modified_by = userName;
                    programToUpdate.modified_date = DateTime.Now;

                    context.SaveChanges();
                });
        }

        public void UpdateProgramFlights(int programId, List<FlightWeekDto> flights, string userName)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var programToUpdate = context.station_programs.Single(q => q.id == programId);

                    // update flight weeks isHaitus flag
                    foreach (var flight in flights)
                    {
                        var foundFlight = context.station_program_flights.SingleOrDefault(f => f.station_program_id == programId && f.media_week_id == flight.Id);

                        if (foundFlight != null)
                        {
                            foundFlight.active = !flight.IsHiatus;
                        }
                    }

                    programToUpdate.modified_by = userName;
                    programToUpdate.modified_date = DateTime.Now;

                    context.SaveChanges();
                });
        }

        public bool RemoveStationProgramGenres(int programId)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var program = context.station_programs.Single(sp => sp.id == programId, string.Format("Cannot find program {0}.", programId));

                    program.genres.Clear();
                    context.SaveChanges();
                });

            return true;
        }

        public void AddStationProgramGenres(int programId, List<LookupDto> programGenres)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var program = context.station_programs.Single(sp => sp.id == programId, string.Format("Cannot find program {0}.", programId));
                    // get the genres
                    var genresToBeAdded = (from g in context.genres
                                           select g).ToList().Where(z => programGenres.Any(q => q.Id == z.id));

                    foreach (var g in genresToBeAdded)
                    {
                        program.genres.Add(g);
                    }

                    context.SaveChanges();
                });
        }

        public void UpdateStationProgram(int programId, DateTime timeStamp, string userName)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var program = context.station_programs.Single(sp => sp.id == programId, string.Format("Cannot find program {0}.", programId));
                    program.modified_by = userName;
                    program.modified_date = timeStamp;

                    context.SaveChanges();
                });
        }

        public void UpdateStationProgram(int programId, DateTime timeStamp, string userName, decimal? fixedPrice)
        {
            _InReadUncommitedTransaction(
                context =>
                {
                    var program = context.station_programs.Single(sp => sp.id == programId, string.Format("Cannot find program {0}.", programId));
                    program.fixed_price = fixedPrice;
                    program.modified_by = userName;
                    program.modified_date = timeStamp;

                    context.SaveChanges();
                });
        }

        public int GetStationProgramIdByStationProgram(StationProgram program, int spotLengthId)
        {
            return _InReadUncommitedTransaction(
                context =>
                {
                    var programId = (from sp in context.station_programs
                                     where sp.station.station_code == program.StationCode &&
                                           sp.rate_source == (byte)program.RateSource &&
                                           string.Compare(sp.program_name, program.ProgramName, StringComparison.OrdinalIgnoreCase) == 0 &&
                                           sp.daypart_id == program.Daypart.Id &&
                                           sp.spot_length_id == spotLengthId &&
                                           sp.start_date == program.StartDate && sp.end_date == program.EndDate
                                     select sp.id).SingleOrDefault();

                    return programId;
                });
        }

        public List<ProposalProgramDto> GetStationProgramsForProposalDetail(DateTime flightStart, DateTime flightEnd, int spotLength, int rateSource, List<int> proposalMarketIds, int proposalDetailId)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                return _InReadUncommitedTransaction(
                    context =>
                    {
                        var programs = _ApplySearchCriteria(context, flightStart, flightEnd, rateSource, proposalMarketIds);

                        // build up the list of stationprograms based on the filters above
                        var query = programs.Select(
                            sp => new ProposalProgramDto
                            {
                                ProgramId = sp.id,
                                DayPartId = sp.daypart_id,
                                ProgramName = sp.program_name,
                                StartDate = sp.start_date,
                                EndDate = sp.end_date,

                                Station = new DisplayScheduleStation
                                {
                                    StationCode = sp.station_code,
                                    LegacyCallLetters = sp.station.legacy_call_letters,
                                    Affiliation = sp.station.affiliation,
                                    CallLetters = sp.station.station_call_letters
                                },
                                Market = new LookupDto
                                {
                                    Id = sp.station.market_code,
                                    Display = sp.station.market.geography_name
                                },
                                Genres = sp.genres.Select(
                                    genre => new LookupDto
                                    {
                                        Id = genre.id,
                                        Display = genre.name
                                    }).ToList(),
                                FlightWeeks = sp.station_program_flights.Select(fw => new ProposalProgramFlightWeek()
                                {
                                    MediaWeekId = fw.media_week_id,
                                    IsHiatus = !fw.active,
                                    Rate = (double)(spotLength == 15 ? fw.C15s_rate ?? 0
                                            : spotLength == 30 ? fw.C30s_rate ?? 0
                                            : spotLength == 60 ? fw.C60s_rate ?? 0
                                            : spotLength == 90 ? fw.C90s_rate ?? 0
                                            : spotLength == 120 ? fw.C120s_rate ?? 0
                                            : 0),
                                    Allocations = fw.station_program_flight_proposal.Where(fp => fp.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters.proposal_version_detail_id == proposalDetailId)
                                                                                              .Select(fp => new OpenMarketAllocationDto { MediaWeekId = fp.proposal_version_detail_quarter_weeks.media_week_id, Spots = fp.spots }).ToList()
                                }).ToList()
                            });

                        return query.ToList();
                    });
            }
        }

        private IQueryable<station_programs> _ApplySearchCriteria(QueryHintBroadcastContext context, DateTime startDate, DateTime endDate, int rateSource, List<int> proposalMarketIds)
        {
            var programs = context.station_programs
                                  .Include(p => p.genres)
                                  .Include(p => p.station)
                                  .Include(p => p.station_program_flights.Select(f => f.station_program_flight_proposal.Select(fp => fp.proposal_version_detail_quarter_weeks.proposal_version_detail_quarters)))
                                  .AsQueryable();

            programs = programs.Where(p => p.rate_source == rateSource);

            // filter the flight: any programs that have a day falling within the proposal flight
            programs = programs.Where(a => (a.start_date >= startDate.Date && a.start_date <= endDate.Date)
                                        || (a.end_date >= startDate.Date && a.end_date <= endDate.Date)
                                        || (a.start_date < startDate.Date && a.end_date > endDate.Date)
                                      );

            if (proposalMarketIds != null & proposalMarketIds.Count > 0)
                programs =
                    programs.Where(p => proposalMarketIds.Contains(p.station.market_code));

            return programs;
        }

    }
}
