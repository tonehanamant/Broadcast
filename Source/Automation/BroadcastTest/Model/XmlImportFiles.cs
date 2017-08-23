using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BroadcastTest.Model
{
    class XmlImportFiles
    {

        public XmlImportFiles() 
        {
            XmlFiles = new List<XmlImportFiles>();
        }

        public string FilePath { get; set; }
        public string FileName { get; set; }

        public List<XmlImportFiles> XmlFiles { get; set; }

    }
}
