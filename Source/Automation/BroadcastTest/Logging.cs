using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using NUnit.Framework;
using System.IO;

namespace BroadcastTest
{

    //Move into framework package
    //[TestFixture]
    public class Logging
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Logging()
        {
            FileInfo fileInfo = new FileInfo(@"C:\Users\bbernstein\Documents\Visual Studio 2013\Projects\BroadcastAutoTest\BroadcastTest\Testing.config");
            log4net.Config.XmlConfigurator.Configure(fileInfo);
        }

        //[Test]
        public void BasicLogTest()
        {
            log.Error("Testing error log.");
        }

        //[Test]
        public void DebugLogTest()
        {
            log.Debug("Testing debug log.");
        }

        //[Test]
        public void DatabaseLogTest()
        {
        }



    }
}
