using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using System;
using System.Collections.Generic;
using System.IO;

namespace Services.Broadcast.Converters
{
    public interface IMasterProgramListImporter
    {
        List<ProgramMappingsDto> ImportMasterProgramList();
    }

    public class MasterProgramListImporter : BroadcastBaseClass, IMasterProgramListImporter
    {
        private const string ProgramId = "PROGRAM_ID";
        private const string ParentId = "PARENT_ID";
        private const string Title = "title";
        private const string EpisodeTitle = "episode_title";
        private const string EpisodeNumber = "episode_number";
        private const string SeasonEpisodeNumber = "season_ep_number";
        private const string OriginalAirDate = "orig_airdate";
        private const string Description = "description";
        private const string SynType = "syn_type";
        private const string ShowType = "show_type";
        private const string Categories = "categories";
        private const string Genre = "genre";
        private const string MpaaRating = "mpaa_rating";
        private const string StationOfOrigination = "station_of_origination";
        private const string HomeTeamId = "HOME_TEAM_ID";
        private const string AwayTeamId = "AWAY_TEAM_ID";
        private const string EventLocation = "event_location";
        private const string WwtvTitle = "WWTV_Title";

        private readonly List<string> FileHeaders = new List<string>
        {
            ProgramId,
            ParentId,
            Title,
            EpisodeTitle,
            EpisodeNumber,
            SeasonEpisodeNumber,
            OriginalAirDate,
            Description,
            SynType,
            ShowType,
            Categories,
            Genre,
            MpaaRating,
            StationOfOrigination,
            HomeTeamId,
            AwayTeamId,
            EventLocation,
            WwtvTitle,
        };

        private readonly string[] Delimiters = new string[] { "|" };

        private readonly IGenreCache _GenreCache;
        private readonly IShowTypeCache _ShowTypeCache;

        public MasterProgramListImporter(IGenreCache genreCache, IShowTypeCache showTypeCache)
        {
            _GenreCache = genreCache;
            _ShowTypeCache = showTypeCache;
        }

        public List<ProgramMappingsDto> ImportMasterProgramList()
        {
            var stream = _GetMasterProgramList();

            var csvFileReader = new CsvFileReader(FileHeaders, Delimiters, throwExceptions: false);
            var masterList = new List<ProgramMappingsDto>();

            using (csvFileReader.Initialize(stream))
            {
                while (csvFileReader.IsEOF() == false)
                {
                    csvFileReader.NextRow();

                    try
                    {
                        var genre = csvFileReader.GetCellValue(Genre);
                        var sourceGenre = _GenreCache.GetSourceGenreLookupDtoByName(genre, ProgramSourceEnum.Master);
                        var maestroGenre = _GenreCache.GetMaestroGenreLookupDtoBySourceGenre(sourceGenre, ProgramSourceEnum.Master);
                        var officialGenre = _GenreCache.GetMaestroGenreByName(maestroGenre.Display);

                        var sourceShowType = _ShowTypeCache.GetMasterShowTypeByName(csvFileReader.GetCellValue(ShowType));
                        var maestroShowType = _ShowTypeCache.GetMaestroShowTypeByMasterShowType(sourceShowType);

                        var programMapping = new ProgramMappingsDto
                        {
                            Id = Convert.ToInt32(csvFileReader.GetCellValue(ProgramId)),
                            OfficialProgramName = csvFileReader.GetCellValue(Title),
                            OfficialGenre = officialGenre,
                            OfficialShowType = maestroShowType
                        };

                        if (!string.IsNullOrWhiteSpace(programMapping.OfficialProgramName))
                            masterList.Add(programMapping);
                    }
                    catch (Exception exception)
                    {
                        _LogWarning($"Error parsing program mapping in master list with exception: {exception.Message}");
                    }
                }
            }

            return masterList;
        }

        private Stream _GetMasterProgramList()
        {
            var appFolder = _GetBroadcastAppFolder();
            const string masterListFolder = "ProgramMappingMasterList";
            const string masterListFile = "MasterListWithWwtvTitles.txt";

            var masterListPath = Path.Combine(
                appFolder,
                masterListFolder,
                masterListFile);

            var fileStream = File.OpenText(masterListPath);

            return fileStream.BaseStream;
        }
    }
}
