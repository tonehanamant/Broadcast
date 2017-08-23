using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using NUnit.Framework;
using System.IO;

namespace BroadcastTest.Hooks
{
    [Binding]
    public sealed class NUnit3Hooks
    {
        // For additional details on SpecFlow hooks see http://go.specflow.org/doc-hooks

        [BeforeFeature("workingdirectory_feature")]
        public static void ChangeWorkingDirectory()
        {
            FeatureContext.Current.Add("NUnit3Hooks.OldWorkingDirectory", Directory.GetCurrentDirectory());
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
        }

        [AfterFeature("workingdirectory_feature")]
        public static void RestoreWorkingDirectory()
        {
            //Directory.SetCurrentDirectory(FeatureContext.Current.Get("NUnit3Hooks.OldWorkingDirectory"));
        }


    }
}
