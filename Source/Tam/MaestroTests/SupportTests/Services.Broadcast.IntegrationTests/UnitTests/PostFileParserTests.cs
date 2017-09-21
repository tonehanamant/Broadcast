using Moq;
using NUnit.Framework;
using OfficeOpenXml;
using Services.Broadcast.Converters;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests
{
    public class PostFileRow
    {
        public string Rank { get; set; }
        public string Market { get; set; }
        public string Station { get; set; }
        public string Affiliate { get; set; }
        public string Weekstart { get; set; }
        public string Day { get; set; }
        public string Date { get; set; }

        [DisplayName("Time Aired")]
        public string TimeAired { get; set; }
        [DisplayName("Program Name")]
        public string ProgramName { get; set; }
        public string Length { get; set; }
        [DisplayName("House ISCI")]
        public string HouseISCI { get; set; }
        [DisplayName("Client ISCI")]
        public string ClientISCI { get; set; }
        public string Advertiser { get; set; }
        [DisplayName("Inventory Source")]
        public string InventorySource { get; set; }
        [DisplayName("Inventory Source Daypart")]
        public string InventorySourceDaypart { get; set; }
        [DisplayName("Inventory Out of Spec Reason")]
        public string InventoryOutofSpecReason { get; set; }
        public string Estimate { get; set; }
        [DisplayName("Detected Via")]
        public string DetectedVia { get; set; }
        public string Spot { get; set; }

        public PostFileRow(string rank, string market, string station, string affiliate, string weekstart, string day, string date, string timeAired, string programName, string length, string houseISCI, string clientISCI, string advertiser, string inventorySource, string inventorySourceDaypart, string inventoryOutofSpecReason, string estimate, string detectedVia, string spot)
        {
            Rank = rank;
            Market = market;
            Station = station;
            Affiliate = affiliate;
            Weekstart = weekstart;
            Day = day;
            Date = date;
            TimeAired = timeAired;
            ProgramName = programName;
            Length = length;
            HouseISCI = houseISCI;
            ClientISCI = clientISCI;
            Advertiser = advertiser;
            InventorySource = inventorySource;
            InventorySourceDaypart = inventorySourceDaypart;
            InventoryOutofSpecReason = inventoryOutofSpecReason;
            Estimate = estimate;
            DetectedVia = detectedVia;
            Spot = spot;
        }
    }

    [TestFixture]
    public class PostFileParserTests
    {
        private PostFileRow _ValidRow;
        private readonly IPostFileParser _PostFileParser = IntegrationTestApplicationServiceFactory.GetApplicationService<IPostFileParser>();
        private ExcelWorksheet _Worksheet;
        private ExcelPackage _Package;

        [SetUp]
        public void Setup()
        {
            _ValidRow = new PostFileRow("93", "BATON ROUGE", "WAFB", "CBS", "2/20/2017", "THU", "2/23/2017", "2/23/2017 4:56:08 AM", "WAFB 9 NEWS THIS MORNING: EARLY EDIT", "15", "", "NNVA0045000", "BEIERSDORF", "ASSEMBLY", "EMN", "", "7196", "BVS Cadent", "1");
            _Package = new ExcelPackage();
            _Package.Workbook.Worksheets.Add("Default");
            _Worksheet = _Package.Workbook.Worksheets.First();
        }

        [Test]
        public void Parse_Throws_When_Missing_RequiredColumns()
        {
            const bool printHeaders = false;
            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, printHeaders);

            try
            {
                _PostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                foreach (var header in PostFileParser.ExcelFileHeaders)
                {
                    Assert.True(e.ParsingErrors.Exists(x => x.Contains(header)));
                }

                Assert.True(e.ParsingErrors.All(x => x.Contains("Could not find header for column")));
            }
        }

        [TestCase("")]
        [TestCase("abc")]
        [TestCase(null)]
        public void Parse_Throws_When_Rank_Invalid(string value)
        {
            _ValidRow.Rank = value;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.InvalidNumberErrorMessage, PostFileParser.RANK, value))));
            }
        }

        [Test]
        public void Parse_Throws_When_Station_Does_Not_Exist()
        {
            _ValidRow.Station = "afasdf";

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            var repository = new Mock<IStationRepository>();
            repository.Setup(r => r.GetStationCode(_ValidRow.Station)).Returns((short?)null);

            try
            {
                using (new RepositoryMock<IStationRepository>(repository))
                    _PostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format("\t'{0}' {1} does not exist\n", PostFileParser.STATION, _ValidRow.Station))));
            }
        }

       [Test]
        public void Parse_Throws_When_Weekstart_Not_Monday()
        {
            //Wednesday
            _ValidRow.Weekstart = "5/17/2017";

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format("\t'{0}' {1} is {2}, not Monday\n", PostFileParser.WEEKSTART, DateTime.Parse(_ValidRow.Weekstart).Date, "Wednesday"))));
            }
        }

        [Test]
        public void Parse_Throws_When_Date_NotSameDateAsTimeAired()
        {
            _ValidRow.Date = DateTime.Today.ToString();
            _ValidRow.TimeAired = DateTime.Today.AddDays(1).ToString();

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format("\t'{0}' {1:d} is not the same day as the Date in '{2}', {3:d}\n", PostFileParser.DATE, DateTime.Parse(_ValidRow.Date).Date, PostFileParser.TIMEAIRED, DateTime.Parse(_ValidRow.TimeAired).Date))));
            }
        }

        [Test]
        public void Parse_Does_Not_Check_SameDay_When_Date_Is_MinValue()
        {
            _ValidRow.Date = DateTime.MinValue.ToString();
            _ValidRow.TimeAired = DateTime.Today.AddDays(1).ToString();

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => !x.Contains(string.Format("\t'{0}' {1:d} is not the same day as the Date in '{2}', {3:d}\n", PostFileParser.DATE, DateTime.Parse(_ValidRow.Date).Date, PostFileParser.TIMEAIRED, DateTime.Parse(_ValidRow.TimeAired).Date))));
            }
        }

        [TestCase("abc")]
        public void Parse_Throws_When_SpotLength_Not_Valid(string value)
        {
            _ValidRow.Length = value;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.ErrorInColumn, PostFileParser.SPOTLENGTH))));
            }
        }

        [Test]
        public void Parse_Throws_When_SpotLength_Not_Found()
        {
            _ValidRow.Length = "92";

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            var mock = new Mock<ISpotLengthRepository>();
            mock.Setup(r => r.GetSpotLengthAndIds()).Returns(new Dictionary<int, int>());

            try
            {
                using (new RepositoryMock<ISpotLengthRepository>(mock))
                    _PostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.ErrorInColumn, PostFileParser.SPOTLENGTH))));
            }
        }

        [TestCase("abc")]
        public void Parse_Throws_When_Estimate_Not_Valid(string value)
        {
            _ValidRow.Estimate = value;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.InvalidNumberErrorMessage, PostFileParser.ESTIMATE, _ValidRow.Estimate))));
            }
        }

        [TestCase("")]
        [TestCase("12")]
        [TestCase(null)]
        [TestCase("asdaf")]
        public void Parse_Throws_When_Spot_Not_Valid(string value)
        {
            _ValidRow.Spot = value;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format("\t'{0}' field has invalid value '{1}', must be 1\n", PostFileParser.SPOT, _ValidRow.Spot))));
            }
        }

        [Test]
        public void Parse_DoesNotReturn_WhenAnyErrors()
        {
            var invalidRow = new PostFileRow("93NOTVALIDVALUE", "BATON ROUGE", "WAFB", "CBS", "2/20/2017", "THU", "2/23/2017", "2/23/2017 4:56:08 AM", "WAFB 9 NEWS THIS MORNING: EARLY EDIT", "15", "", "NNVA0045000", "BEIERSDORF", "ASSEMBLY", "EMN", "", "7196", "BVS Cadent", "1");

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow, invalidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.Any());
            }
        }

        [Test]
        public void Parse_Throws_When_Station_Empty()
        {
            _ValidRow.Station = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.ColumnRequiredErrorMessage, PostFileParser.STATION))));
            }
        }

        [Test]
        public void Parse_Throws_When_Affiliate_Empty()
        {
            _ValidRow.Affiliate = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.ColumnRequiredErrorMessage, PostFileParser.AFFILIATE))));
            }
        }

        [TestCase("")]
        [TestCase("12/30/1899 2:15:00 PM")]
        public void Parse_Throws_When_Weekstart_Invalid(string date)
        {
            _ValidRow.Weekstart = date;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.InvalidDateErrorMessage, PostFileParser.WEEKSTART))));
            }
        }

        [Test]
        public void Parse_Throws_When_Day_Not_Valid()
        {
            _ValidRow.Day = "aFGafas";

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format("\t'{0}' {1} is not a valid day\n", PostFileParser.DAY, _ValidRow.Day))));
            }
        }

        [Test]
        public void Parse_Throws_When_Day_Empty()
        {
            var sut = _PostFileParser;
            var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("Default");
            var worksheet = package.Workbook.Worksheets.First();

            _ValidRow.Day = string.Empty;

            worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                sut.Parse(package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.ColumnRequiredErrorMessage, PostFileParser.DAY))));
            }
        }

        [TestCase("")]
        [TestCase("12/30/1899 2:15:00 PM")]
        public void Parse_Throws_When_Date_Invalid(string date)
        {
            _ValidRow.Date = date;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.InvalidDateErrorMessage, PostFileParser.DATE))));
            }
        }

        [TestCase("")]
        [TestCase("12/30/1899 2:15:00 PM")]
        public void Parse_Throws_When_TimeAired_Invalid(string date)
        {
            _ValidRow.TimeAired = date;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.InvalidDateErrorMessage, PostFileParser.TIMEAIRED))));
            }
        }

        [Test]
        public void Parse_Throws_When_ProgramName_Empty()
        {
            _ValidRow.ProgramName = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.ColumnRequiredErrorMessage, PostFileParser.PROGRAMNAME))));
            }
        }

        [Test]
        public void Parse_Throws_When_SpotLength_Empty()
        {
            _ValidRow.Length = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.ColumnRequiredErrorMessage, PostFileParser.SPOTLENGTH))));
            }
        }

        [Test]
        public void Parse_Throws_When_ClientISCI_Empty()
        {
            _ValidRow.ClientISCI = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.ColumnRequiredErrorMessage, PostFileParser.CLIENTISCI))));
            }
        }

        [Test]
        public void Parse_Throws_When_Advertiser_Empty()
        {
            _ValidRow.Advertiser = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.ColumnRequiredErrorMessage, PostFileParser.ADVERTISER))));
            }
        }

        [Test]
        public void Parse_Throws_When_Estimate_Empty()
        {
            _ValidRow.Estimate = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.ColumnRequiredErrorMessage, PostFileParser.ESTIMATE))));
            }
        }

        [Test]
        public void Parse_Throws_When_DetectedVia_Empty()
        {
            _ValidRow.DetectedVia = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<PostFileRow> { _ValidRow }, true);

            try
            {
                _PostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(PostFileParser.ColumnRequiredErrorMessage, PostFileParser.DETECTEDVIA))));
            }
        }
    }
}
