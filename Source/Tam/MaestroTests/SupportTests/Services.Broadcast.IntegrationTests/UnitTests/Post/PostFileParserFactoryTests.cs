using System.Collections.Generic;
using NUnit.Framework;
using OfficeOpenXml;
using Services.Broadcast.Converters.Post;
using System.Linq;

namespace Services.Broadcast.IntegrationTests.UnitTests.Post
{
    [TestFixture]
    public class PostFileParserFactoryTests
    {
        private readonly IPostFileParserFactory _PostFileParserFactory =
            IntegrationTestApplicationServiceFactory.GetApplicationService<IPostFileParserFactory>();

        [Test]
        public void CreateBvsPostFileFactoryTest()
        {
            var bvsPackage = new ExcelPackage();
            var bvsValidRow = new BvsPostFileRow("9", "ATLANTA", "WGCL", "CBS", "9/30/2017", "9/30/2017 6:12:51", "CBS46 NEWS AT 6AM", "30", "NNVA0045000", "BEIERSDORF", "TACO BELL (EMN)", "3763/CNN AM", "In Spec", "Incorrect Day");           
            
            bvsPackage.Workbook.Worksheets.Add("Default");
            
            var bvsWorksheet = bvsPackage.Workbook.Worksheets.First();

            bvsWorksheet.Cells.LoadFromCollection(new List<BvsPostFileRow> { bvsValidRow }, true);

            var bvsParser = _PostFileParserFactory.CreateParser(bvsPackage);

            Assert.IsInstanceOf(typeof(BvsPostFileParser), bvsParser);
        }

        [Test]
        public void CreatePostFileFactoryTest()
        {
            var postValidRow = new PostFileRow("93", "BATON ROUGE", "WAFB", "CBS", "2/20/2017", "THU", "2/23/2017", "2/23/2017 4:56:08 AM", "WAFB 9 NEWS THIS MORNING: EARLY EDIT", "15", "", "NNVA0045000", "BEIERSDORF", "ASSEMBLY", "EMN", "", "7196", "BVS Cadent", "1");
            var postPackage = new ExcelPackage();
            
            postPackage.Workbook.Worksheets.Add("Default");

            var postWorksheet = postPackage.Workbook.Worksheets.First();

            postWorksheet.Cells.LoadFromCollection(new List<PostFileRow> { postValidRow }, true);

            var postParser = _PostFileParserFactory.CreateParser(postPackage);

            Assert.IsInstanceOf(typeof(PostFileParser), postParser);
        }
    }
}
