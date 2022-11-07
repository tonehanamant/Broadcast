using OfficeOpenXml;
using Services.Broadcast.Cache;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Extensions;
using Services.Broadcast.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Services.Broadcast.Converters
{
    public interface IMasterProgramListImporter
    {
        List<ProgramMappingsDto> ImportMasterProgramList();
        List<ProgramMappingsDto> UploadMasterProgramList(Stream stream);
        /// <summary>
        /// Read the program list file and return the program list
        /// </summary>
        List<ProgramMappingsDto> ParseProgramGenresExcelFile(Stream stream);
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
        private const string ProgramShowType = "Miscellaneous";

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

        public MasterProgramListImporter(IGenreCache genreCache, IShowTypeCache showTypeCache, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper) : base(featureToggleHelper, configurationSettingsHelper)
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
                int LineIndex = 1;
                while (csvFileReader.IsEOF() == false)
                {
                    csvFileReader.NextRow();
                    try
                    {
                        var genre = csvFileReader.GetCellValue(Genre);
                        if (genre == "NULL")
                        { 
                            _LogWarning($"Line {LineIndex} has an invalid Genre of 'NULL'");
                        }
                        else
                        {
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
                        LineIndex++;
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

        public List<ProgramMappingsDto> UploadMasterProgramList(Stream stream)
        {
            var csvFileReader = new CsvFileReader(FileHeaders, Delimiters, throwExceptions: false);
            var masterList = new List<ProgramMappingsDto>();
            using (csvFileReader.Initialize(stream))
            {
                int LineIndex = 1;
                while (csvFileReader.IsEOF() == false)
                {
                    csvFileReader.NextRow();
                    try
                    {
                        var genre = csvFileReader.GetCellValue(Genre);
                        if (genre == "NULL")
                        {
                            _LogWarning($"Line {LineIndex} has an invalid Genre of 'NULL'");
                        }
                        else
                        {
                            //** This is going to remove in BP-5532 story **//
                            //** Start **//                            
                            var officialGenre = _GenreCache.GetMaestroGenreByName(genre);
                            //** End **//
                            var sourceShowType = _ShowTypeCache.GetMasterShowTypeByName(csvFileReader.GetCellValue(ShowType));
                            var maestroShowType = _ShowTypeCache.GetMaestroShowTypeByMasterShowType(sourceShowType);
                            var programMapping = new ProgramMappingsDto
                            {
                                Id = Convert.ToInt32(csvFileReader.GetCellValue(ProgramId)),
                                OfficialProgramName = csvFileReader.GetCellValue(Title),
                                //** This is going to remove in BP-5532 story **//
                                OfficialGenre = officialGenre,
                                OfficialShowType = maestroShowType
                            };
                            if (!string.IsNullOrWhiteSpace(programMapping.OfficialProgramName))
                                masterList.Add(programMapping);
                        }
                        LineIndex++;
                    }
                    catch (Exception exception)
                    {
                        _LogWarning($"Error parsing program mapping in master list with exception: {exception.Message}");
                    }
                }
            }
            return masterList;
        }
        /// <summary>
        /// Read the program list file and return the program list
        /// </summary>
        /// <param name="stream">Filestream</param>
        /// <returns>Return the program list</returns>
        public List<ProgramMappingsDto> ParseProgramGenresExcelFile(Stream stream)
        {
            var givenPrograms = _ReadProgramFile(stream);
            var programsList = new List<ProgramMappingsDto>();

            // for now we use the same show type for everything
            var sourceShowType = _ShowTypeCache.GetMaestroShowTypeByName(ProgramShowType);

            foreach (var givenProgram in givenPrograms)
            {
                Genre officialGenre = null;

                try
                {
                    officialGenre = _GenreCache.GetMaestroGenreByName(givenProgram.OfficialGenre);
                }
                catch (Exception ex)
                {
                    _LogError($"Error resolving given genre '{givenProgram.OfficialGenre}'.", ex);
                }

                if (officialGenre != null)
                {
                    var program = new ProgramMappingsDto
                    {
                        OfficialProgramName = givenProgram.OfficialProgramName,
                        OfficialGenre = officialGenre,
                        OfficialShowType = sourceShowType
                    };

                    programsList.Add(program);
                }
                else
                {
                    _LogInfo(String.Format("Program name {0} could not be proceed as An unknown" +
                        " {1} genre {2} was discovered: ", givenProgram.OfficialProgramName, "Maestro", givenProgram.OfficialGenre));
                }
            }
            return programsList;
        }

        private List<ProgramListFileRequestDto> _ReadProgramFile(Stream stream)
        {
            var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[1];
            var rawPrograms = worksheet.ConvertSheetToObjects<ProgramListFileRequestDto>();
            // filter out the empty rows the above can pick up
            var filteredPrograms= rawPrograms.Skip(3).Where(x=> x.OfficialGenre != null && x.OfficialProgramName != null).ToList();
            return filteredPrograms;
        }

    }
}
