using System;
using System.Linq;
using Common.Services.ApplicationServices;
using Common.Services.Repositories;
using OfficeOpenXml;

namespace Services.Broadcast.Converters.Post
{
    public interface IPostFileParserFactory : IApplicationService
    {
        BasePostFileParser CreateParser(ExcelPackage stream);
    }

    public class PostFileParserFactory : IPostFileParserFactory
    {
        private readonly IDataRepositoryFactory _DataRepositoryFactory;

        private const string PostFileHeader = "Weekstart";

        public PostFileParserFactory(IDataRepositoryFactory dataRepositoryFactory)
        {
            _DataRepositoryFactory = dataRepositoryFactory;
        }

        public BasePostFileParser CreateParser(ExcelPackage excelPackage)
        {
            var worksheet = excelPackage.Workbook.Worksheets.First();
            var postFileHeaderFound = false;

            for (var column = 1; column <= worksheet.Dimension.End.Column; column++)
            {
                var value = worksheet.Cells[1, column].Value ?? string.Empty;

                var cellValue = value.ToString().Trim();

                if (string.Equals(cellValue, PostFileHeader, StringComparison.CurrentCultureIgnoreCase))
                    postFileHeaderFound = true;
            }

            if (postFileHeaderFound)
                return new PostFileParser(_DataRepositoryFactory,null,null);

            return new BvsPostFileParser(_DataRepositoryFactory,null,null);
        }
    }
}