using Moq;
using NUnit.Framework;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Services.Broadcast.Converters.Post;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests.Post
{
    [TestFixture]
    public class BvsPostFileParserTests
    {
        private readonly IPostFileParser _BvsPostFileParser =
            new BvsPostFileParser(IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory);
        private BvsPostFileRow _ValidRow;
        private ExcelWorksheet _Worksheet;
        private ExcelPackage _Package;

        [SetUp]
        public void Setup()
        {
            _ValidRow = new BvsPostFileRow("9", "ATLANTA", "WGCL", "CBS", "9/30/2017", "9/30/2017 6:12:51", "CBS46 NEWS AT 6AM", "30", "NNVA0045000", "BEIERSDORF", "TACO BELL (EMN)", "3763/CNN AM", "In Spec", "Incorrect Day");
            _Package = new ExcelPackage();
            _Package.Workbook.Worksheets.Add("Default");
            _Worksheet = _Package.Workbook.Worksheets.First();
        }

        [Test]
        public void Weekstart_Is_Monday()
        {
            // Sunday.
            var date = new DateTime(2017, 10, 4, 12, 0, 0);

            // Previous Monday.
            var weekstart = new DateTime(2017, 10, 2, 0, 0, 0);

            _ValidRow.Date = date.ToString("d");
            _ValidRow.TimeAired = date.ToString("G");

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            var result = _BvsPostFileParser.Parse(_Package);

            var firstRow = result.First();

            Assert.AreEqual(weekstart, firstRow.weekstart);
            Assert.AreEqual(DayOfWeek.Monday, firstRow.weekstart.DayOfWeek);
        }

        [Test]
        public void Weekstart_Is_Correct_When_Date_Is_Sunday()
        {
            // Sunday.
            var date = new DateTime(2017, 10, 1, 12, 0 , 0);
            
            // Previous Monday.
            var weekstart = new DateTime(2017, 09, 25, 0, 0, 0);

            _ValidRow.Date = date.ToString("d");
            _ValidRow.TimeAired = date.ToString("G");

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            var result = _BvsPostFileParser.Parse(_Package);

            var firstRow = result.First();

            Assert.AreEqual(weekstart, firstRow.weekstart);
        }

        [Test]
        public void Parse_Throws_When_Missing_RequiredColumns()
        {
            const bool printHeaders = false;

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, printHeaders);

            try
            {
                _BvsPostFileParser.Parse(_Package);
                
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                foreach (var header in BvsPostFileParser.ExcelFileHeaders)
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

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BasePostFileParser.InvalidNumberErrorMessage, BvsPostFileParser.RANK, value))));
            }
        }

        [Test]
        public void Parse_Throws_When_Station_Does_Not_Exist()
        {
            _ValidRow.Station = "afasdf";

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            var repository = new Mock<IStationRepository>();
            repository.Setup(r => r.GetStationCode(_ValidRow.Station)).Returns((short?)null);

            try
            {
                using (new RepositoryMock<IStationRepository>(repository))
                    _BvsPostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format("\t'{0}' {1} does not exist\n", BvsPostFileParser.STATION, _ValidRow.Station))));
            }
        }

        [Test]
        public void Parse_Throws_When_Date_NotSameDateAsTimeAired()
        {
            _ValidRow.Date = DateTime.Today.ToString();
            _ValidRow.TimeAired = DateTime.Today.AddDays(1).ToString();

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format("\t'{0}' {1:d} is not the same day as the Date in '{2}', {3:d}\n", BvsPostFileParser.DATE, DateTime.Parse(_ValidRow.Date).Date, BvsPostFileParser.TIMEAIRED, DateTime.Parse(_ValidRow.TimeAired).Date))));
            }
        }

        [Test]
        public void Parse_Does_Not_Check_SameDay_When_Date_Is_MinValue()
        {
            _ValidRow.Date = DateTime.MinValue.ToString();
            _ValidRow.TimeAired = DateTime.Today.AddDays(1).ToString();

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => !x.Contains(string.Format("\t'{0}' {1:d} is not the same day as the Date in '{2}', {3:d}\n", BvsPostFileParser.DATE, DateTime.Parse(_ValidRow.Date).Date, BvsPostFileParser.TIMEAIRED, DateTime.Parse(_ValidRow.TimeAired).Date))));
            }
        }

        [TestCase("abc")]
        public void Parse_Throws_When_SpotLength_Not_Valid(string value)
        {
            _ValidRow.Length = value;

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BasePostFileParser.ErrorInColumn, BvsPostFileParser.SPOTLENGTH))));
            }
        }

        [Test]
        public void Parse_Throws_When_SpotLength_Not_Found()
        {
            _ValidRow.Length = "92";

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            var mock = new Mock<ISpotLengthRepository>();
            mock.Setup(r => r.GetSpotLengthAndIds()).Returns(new Dictionary<int, int>());

            try
            {
                using (new RepositoryMock<ISpotLengthRepository>(mock))
                    _BvsPostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BasePostFileParser.ErrorInColumn, BvsPostFileParser.SPOTLENGTH))));
            }
        }

        [TestCase("/CNN AM")]
        public void Parse_Throws_When_Estimate_Not_Valid(string value)
        {
            _ValidRow.EstimateAndInventorySource = value;

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
                Assert.Fail();
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BasePostFileParser.InvalidNumberErrorMessage, BvsPostFileParser.ESTIMATE_AND_INVENTORY_SOURCE, _ValidRow.EstimateAndInventorySource))));
            }
        }

        [Test]
        public void Parse_DoesNotReturn_WhenAnyErrors()
        {
            var invalidRow = new BvsPostFileRow("AAA", "ATLANTA", "WGCL", "CBS", "9/30/2017", "9/30/2017 6:12:51", "CBS46 NEWS AT 6AM", "30", "NNVA0045000", "BEIERSDORF", "TACO BELL (EMN)", "3763/CNN AM", "In Spec", "Incorrect Day");

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow, invalidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
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

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BasePostFileParser.ColumnRequiredErrorMessage, BvsPostFileParser.STATION))));
            }
        }

        [Test]
        public void Parse_Throws_When_Affiliate_Empty()
        {
            _ValidRow.Affiliate = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BasePostFileParser.ColumnRequiredErrorMessage, BvsPostFileParser.AFFILIATE))));
            }
        }

        [TestCase("")]
        [TestCase("12/30/1899 2:15:00 PM")]
        public void Parse_Throws_When_Date_Invalid(string date)
        {
            _ValidRow.Date = date;

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BasePostFileParser.InvalidDateErrorMessage, BvsPostFileParser.DATE))));
            }
        }

        [TestCase("")]
        [TestCase("12/30/1899 2:15:00 PM")]
        public void Parse_Throws_When_TimeAired_Invalid(string date)
        {
            _ValidRow.TimeAired = date;

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BasePostFileParser.InvalidDateErrorMessage, BvsPostFileParser.TIMEAIRED))));
            }
        }

        [Test]
        public void Parse_Throws_When_ProgramName_Empty()
        {
            _ValidRow.ProgramName = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BasePostFileParser.ColumnRequiredErrorMessage, BvsPostFileParser.PROGRAMNAME))));
            }
        }

        [Test]
        public void Parse_Throws_When_SpotLength_Empty()
        {
            _ValidRow.Length = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BasePostFileParser.ColumnRequiredErrorMessage, BvsPostFileParser.SPOTLENGTH))));
            }
        }

        [Test]
        public void Parse_Throws_When_ClientIsci_Empty()
        {
            _ValidRow.ClientIsci = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BasePostFileParser.ColumnRequiredErrorMessage, BvsPostFileParser.CLIENTISCI))));
            }
        }

        [Test]
        public void Parse_Throws_When_Advertiser_Empty()
        {
            _ValidRow.AdvertiserAndDaypart = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BvsPostFileParser.UnableToParseValueErrorMessage, BvsPostFileParser.ADVERTISER_AND_DAYPART))));
            }
        }

        [Test]
        public void Parse_Throws_When_Advertiser_Incorrect_Format()
        {
            _ValidRow.AdvertiserAndDaypart = "(EMN)";

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BasePostFileParser.ColumnRequiredErrorMessage, BvsPostFileParser.ADVERTISER_AND_DAYPART))));
            }
        }

        [Test]
        public void Parse_Throws_When_Estimate_And_Inventory_Source_Are_Empty()
        {
            _ValidRow.EstimateAndInventorySource = string.Empty;

            _Worksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { _ValidRow }, true);

            try
            {
                _BvsPostFileParser.Parse(_Package);
            }
            catch (PostParsingException e)
            {
                Assert.True(e.ParsingErrors.All(x => x.Contains(string.Format(BvsPostFileParser.UnableToParseValueErrorMessage, BvsPostFileParser.ESTIMATE_AND_INVENTORY_SOURCE))));
            }
        }        
    }
}
