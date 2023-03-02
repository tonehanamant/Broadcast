using Services.Broadcast.Extensions;
using System.Collections.Generic;

namespace Services.Broadcast.Entities.SpotExceptions.OutOfSpecs
{
    public class OutOfSpecExportReportData
    {       
        private const string FILENAME_FORMAT = "Template - Out of Spec Report Buying Team.xlsx";

        /// <summary>
        /// Gets or sets the name of the out of spec export file.
        /// </summary>
        /// <value>The name of the out of spec export export file.</value>
        public string OutOfSpecExportFileName { get; set; }

        /// <summary>
        /// Gets or sets the out of spec export.
        /// </summary>
        /// <value>The out of spec export.</value>
        public List<OutOfSpecExportReportDto> OutOfSpecs { get; set; } = new List<OutOfSpecExportReportDto>();

        /// <summary>
        /// Gets the name of the out of spec export file.
        /// </summary>
        /// <returns></returns>
        internal string _GetMarketAffiliatesExportFileName()
        {
            var rawFileName = FILENAME_FORMAT;
            var fileName = rawFileName.PrepareForUsingInFileName();
            return fileName;
        }
    }
}
