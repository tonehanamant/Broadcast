using ApprovalTests.Reporters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Broadcast.IntegrationTests
{
    public class BroadcastDiffReporter : DiffReporter
    {
        public override void Report(string approved, string received)
        {
            base.Report(received, approved);
        }
    }
}
