using Services.Broadcast.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.Entities
{
    /// <summary>
    ///This class is used to read the data from program excel file
    /// </summary>
    public class ProgramListFileRequestDto
    {
        /// <summary>
        ///Get or set the Program name 
        /// </summary>
        [ExcelColumn(2)]
        public string OfficialProgramName { get; set; }
        /// <summary>
        ///Get or set the genre name 
        /// </summary>
        [ExcelColumn(3)]
        public string OfficialGenre { get; set; }
    }
}
