using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using EntityFrameworkMapping.Broadcast;
using Microsoft.VisualBasic.FileIO;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.Converters
{
    public class AudienceInfo
    {
        public int FieldIndex { get; set; }
        public int AudienceId { get; set; }
    }


    public interface IAssemblyScheduleConverter : IApplicationService, IScheduleConverter
    {
    }


    public class AssemblyScheduleConverter : ScheduleConverterBase,IAssemblyScheduleConverter
    {
        private static readonly List<string> CsvFileHeaders = new List<string>()
        {
            "MARKET",
            "STA",
            "WEEK OF",
            "LEN",
            "DAYS",
            "TIME",
            "PROGRAM NAME",
            "PROGRAM AIRED",
            "Bkd $ (G)",
            "Buy Spots"
        };

        public AssemblyScheduleConverter(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IDaypartCache daypartCache,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache, IBroadcastAudiencesCache audienceCache)
            : base(broadcastDataRepositoryFactory, daypartCache, mediaMonthAndWeekAggregateCache,audienceCache)
        {
        }

        private Dictionary<int, AudienceInfo> _demoInfo = null;

        public override schedule Convert(ScheduleDTO scheduleDto)
        {
            var rawSchedule = _GetScheduleData(scheduleDto);
            // the schedule details are stored from the file temporarily so they can be easily aggregated.
            rawSchedule.schedule_details = _AggregateDetails(rawSchedule);

            return rawSchedule;
        }

        private schedule _GetScheduleData(ScheduleDTO scheduleDto)
        {
            schedule convertedSchedule = null;
            var row = 1;
            using (var parser = _SetUsUpTheCSVParser(scheduleDto.FileStream))
            {
                var headers = _ValidateAndSetupHeadersAndDemos(parser);

                convertedSchedule = _MapSchedule(scheduleDto);
                convertedSchedule.schedule_audiences = _MapAudiences();

                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields().ToList();
                    var newDetail = _MapDetailsFromFile(fields, headers);
                    if (newDetail != null)
                    {
                        newDetail.schedule_id = convertedSchedule.id;
                        convertedSchedule.schedule_details.Add(newDetail);
                    }
                    row++;
                }
            }

            convertedSchedule.start_date =
                convertedSchedule.schedule_details.SelectMany(s => s.schedule_detail_weeks).Min(w => w.start_date);
            convertedSchedule.end_date =
                convertedSchedule.schedule_details.SelectMany(s => s.schedule_detail_weeks).Max(w => w.end_date);

            return convertedSchedule;
        }

        public List<schedule_details> _AggregateDetails(schedule rawSchedule)
        {
            var detailsGroups = rawSchedule
                .schedule_details
                .GroupBy(sd => new
                {
                    sd.market,
                    sd.network,
                    sd.program,
                    sd.spot_length_id,
                    sd.daypart_id
                });

            List<schedule_details> details = new List<schedule_details>();
            foreach (var detailsGroup in detailsGroups)
            {
                var detail = new schedule_details();
                detail.market = detailsGroup.Key.market;
                detail.network = detailsGroup.Key.network;
                detail.program = detailsGroup.Key.program;
                detail.spot_length_id = detailsGroup.Key.spot_length_id;
                detail.daypart_id = detailsGroup.Key.daypart_id;

                detail.spot_length = detailsGroup.First().spot_length;

                int weeks = 0;
                var detailsWeekGroup = detailsGroup
                    .SelectMany(dg => dg.schedule_detail_weeks)
                    .GroupBy(dg => dg.media_week_id);
                foreach (var group in detailsWeekGroup)
                {
                    var mediaWeek = new schedule_detail_weeks();
                    mediaWeek.media_week_id = group.Key;
                    mediaWeek.spots = group.Sum(dg => dg.spots);
                    mediaWeek.start_date = group.First().start_date;
                    mediaWeek.end_date = group.First().end_date;
                    detail.schedule_detail_weeks.Add(mediaWeek);
                    weeks++;
                }
                var audiences = new List<schedule_detail_audiences>();
                foreach (var demo in _demoInfo)
                {
                    var demoRank = demo.Key;
                    audiences.Add(new schedule_detail_audiences()
                    {
                        demo_rank = demoRank,
                        impressions = detailsGroup
                            .SelectMany(dg => dg.schedule_detail_audiences)
                            .Where(sda => sda.audience_id == demo.Value.AudienceId)
                            .Sum(sa => sa.impressions),
                        audience_id = demo.Value.AudienceId
                    });
                }
                detail.schedule_detail_audiences = audiences;

                // calc costs
                detail.total_spots = detailsGroup.Sum(dg => dg.total_spots);
                detail.total_cost = detailsGroup.Sum(dg => dg.total_cost);
                if (detail.total_spots != 0)
                    detail.spot_cost = detail.total_cost / detail.total_spots;

                details.Add(detail);
            }

            return details;
        }

        private List<schedule_audiences> _MapAudiences()
        {
            var audiences = new List<schedule_audiences>();

            foreach (var demo in _demoInfo)
            {
                var audience = new schedule_audiences();
                audience.audience_id = demo.Value.AudienceId;
                audience.rank = demo.Key;
                audiences.Add(audience);
            }
            return audiences;
        }

        private schedule_details _MapDetailsFromFile(List<string> fields, Dictionary<string, int> headers)
        {
            var rawDetail = new schedule_details();
            rawDetail.market = fields[headers["MARKET"]];
            rawDetail.network = fields[headers["STA"]];
            rawDetail.program = fields[headers["PROGRAM NAME"]];
            if (string.IsNullOrEmpty(rawDetail.network)
                || string.IsNullOrEmpty(rawDetail.market)
                || string.IsNullOrEmpty(rawDetail.program))
                return null;

            DateTime startWeek;
            if (!DateTime.TryParse(fields[headers["WEEK OF"]], out startWeek))
                throw new Exception(string.Format("Invalid start week \"{0}\"", fields[headers["WEEK OF"]]));

            var daypartId = _GetDaypartId(fields, headers);
            if (daypartId == null)
                return null;
            rawDetail.daypart_id = daypartId.Value;

            int quantity;
            if (int.TryParse(fields[headers["Buy Spots"]], out quantity))
                rawDetail.total_spots = quantity;

            var rawRate = fields[headers["Bkd $ (G)"]].Replace("$", string.Empty);
            var rate = System.Convert.ToDecimal(rawRate);
            rawDetail.total_cost = rate;
            rawDetail.spot_cost = 0;

            rawDetail.spot_length = fields[headers["LEN"]];
            rawDetail.spot_length_id = _LookupSpotLengthId(rawDetail.spot_length);

            var mediaWeekId = _MediaMonthAndWeekAggregateCache.GetMediaWeekContainingDate(startWeek).Id;
            var detailWeek = new schedule_detail_weeks();
            detailWeek.start_date = startWeek;
            detailWeek.end_date = detailWeek.start_date.AddDays(6);
            detailWeek.media_week_id = mediaWeekId;
            detailWeek.spots = rawDetail.total_spots;
            rawDetail.schedule_detail_weeks.Add(detailWeek);

            var audiences = new List<schedule_detail_audiences>();
            foreach (var demo in _demoInfo)
            {
                var demoRank = demo.Key;
                audiences.Add(new schedule_detail_audiences()
                {
                    demo_rank = demoRank,
                    impressions = System.Convert.ToInt32(decimal.Parse(fields[demo.Value.FieldIndex])*1000),
                    audience_id = demo.Value.AudienceId
                });
                rawDetail.schedule_detail_audiences = audiences;
            }
            rawDetail.schedule_detail_audiences = audiences;

            return rawDetail;
        }

        private int _LookupSpotLengthId(string spot_length_string)
        {
            int spotLength = -1;
            if (!Int32.TryParse(spot_length_string,out spotLength))
                throw new Exception(string.Format("Cannot convert spot length, {0}, to number", spot_length_string));

            var spotLengths = _DataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthsByLength(new List<int>() {spotLength});

            return spotLengths.First().Id;
        }
        private int? _GetDaypartId(List<string> fields, Dictionary<string, int> headers)
        {
            var time = fields[headers["TIME"]];
            var days = fields[headers["DAYS"]];
            if (string.IsNullOrEmpty(time) || string.IsNullOrEmpty(days))
                return null;

            var displayDaypart = AssemblyImportHelper.LookupDaypartByTimeAndRotation(time, days, _DaypartCache);

            return displayDaypart.Id;
        }

        private Dictionary<int, AudienceInfo> _FetchMatchingAudiences(List<string> fields, int audienceStartingIndex)
        {
            var matchedDemos = new Dictionary<int, AudienceInfo>();
            var demoRank = 1;
            string errorMessages = string.Empty;
            for (var rover = audienceStartingIndex; rover < fields.Count; rover++)
            {
                var audienceField = fields[rover];
                if (audienceField[0] == 'R')
                    continue; // skip ratings (note demo rank remains unchanged)

                int startAge;
                int endAge;

                var subcategoryCode = AssemblyImportHelper.ExtractAudienceInfo(audienceField.Substring(1), out startAge, out endAge);

                var possibleAudienceMatches = _AudienceCache.FindByAgeRange(startAge, endAge).Select(a => new LookupDto(a.Id, a.SubCategoryCode)).ToList();

                var matchingAudience = possibleAudienceMatches.FirstOrDefault(a => a.Display == subcategoryCode);


                if (matchingAudience == null && subcategoryCode == AssemblyImportHelper.WomanSubcategoryCode)
                {
                    matchingAudience = possibleAudienceMatches.FirstOrDefault(a => a.Display == AssemblyImportHelper.FemaleSubcategoryCode);
                }
                if (matchingAudience == null)
                {
                    errorMessages += string.Format("Could not find a matching audience for \"{0}\" <br />", audienceField);
                    continue;
                }

                if (matchedDemos.Any(d => d.Value.AudienceId == matchingAudience.Id))
                    continue;   // dup audience? skip

                var audienceInfo = new AudienceInfo()
                {
                    AudienceId = matchingAudience.Id,
                    FieldIndex = rover
                };
                matchedDemos.Add(demoRank, audienceInfo);
                demoRank++;
            }
            if (!string.IsNullOrEmpty(errorMessages))
                throw new Exception(errorMessages);

            return matchedDemos;
        }

        private Dictionary<string, int> _ValidateAndSetupHeadersAndDemos(TextFieldParser parser)
        {
            var fields = parser.ReadFields().ToList();
            var validationErrors = new List<string>();
            var headers = new Dictionary<string, int>();

            try
            {
                foreach (var header in CsvFileHeaders)
                {
                    var headerItemIndex = fields.IndexOf(header);
                    if (headerItemIndex >= 0)
                    {
                        headers.Add(header, headerItemIndex);
                        if (header == "Buy Spots")
                        {
                            _demoInfo = _FetchMatchingAudiences(fields, headerItemIndex + 1);
                            break;  // last one
                        }
                        continue;
                    }
                    validationErrors.Add(string.Format("Could not find required column {0}.<br />", header));
                }
            }
            catch (Exception e)
            {
                validationErrors.Add(e.Message);
                // message will be picked up below
            }

            if (validationErrors.Any())
            {
                var message = "";
                validationErrors.ForEach(err => message += err + "<br />");
                throw new Exception(message);
            }
            return headers;
        }

        private TextFieldParser _SetUsUpTheCSVParser(Stream rawStream)
        {
            var parser = new TextFieldParser(rawStream);
            if (parser.EndOfData)
                throw new ExtractBvsExceptionEmptyFiles();

            parser.SetDelimiters(new string[] { "," });

            return parser;
        }

    }
}
