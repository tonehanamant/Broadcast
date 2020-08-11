using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Inventory.ProgramMapping;

namespace Services.Broadcast.IntegrationTests.UnitTests.ApplicationServices.Inventory.ProgramMapping.Engines
{
    public class ProgramMappingCleanupEngineUnitTests
    {
        private ProgramMappingCleanupEngine _ProgramMappingCleanupEngine;

        [SetUp]
        public void SetUp()
        {
            _ProgramMappingCleanupEngine = new ProgramMappingCleanupEngine();
        }

        [Test]
        [TestCase("********* Simpsons", "Simpsons")]
        [TestCase("#Simpsons", "Simpsons")]
        [TestCase("x2 Simpsons 2X", "Simpsons")]
        [TestCase("#Simpsons (TENTATIVE) (R) [E] (O) (EM)", "Simpsons")]
        [TestCase("Simpsons at 12:12AM", "Simpsons")]
        [TestCase("Simpsons Su", "Simpsons")]
        [TestCase("Simpsons Sun", "Simpsons")]
        [TestCase("Simpsons Sa", "Simpsons")]
        [TestCase("Simpsons Sat", "Simpsons")]
        [TestCase("Simpsons RPT", "Simpsons")]
        [TestCase("Simpsons ENCORE", "Simpsons")]
        [TestCase("Simpsons FOLLOWING NBA FINALS", "Simpsons")]
        [TestCase("Simpsons (2 HOURS)", "Simpsons")]
        [TestCase("Simpsons (REBROADCAST) (ENCORE) ENCORE (RERUN) (TENT)", "Simpsons")]
        [TestCase("Simpsons (M-F)", "Simpsons")]
        [TestCase("Simpsons SIMULCAST", "Simpsons")]
        public void GetCleanProgram_ApplyRemoval(string programName, string expected)
        {
            var result = _ProgramMappingCleanupEngine.GetCleanProgram(programName);

            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase("Simpsons NWS", "Simpsons NEWS")]
        [TestCase("Simpsons /W Futurama", "Simpsons WITH Futurama")]
        [TestCase("Simpsons W/ Futurama", "Simpsons WITH Futurama")]
        [TestCase("Simpsons FT.", "Simpsons FORT")]
        [TestCase("MORN Simpsons", "MORNING Simpsons")]
        [TestCase("Simpsons WKND", "Simpsons WEEKEND")]
        [TestCase("Simpsons MINUT(12)", "Simpsons MINUTE")]
        [TestCase("Simpsons MRN", "Simpsons MORNING")]
        [TestCase("GD Simpsons", "GOOD Simpsons")]
        [TestCase("GM Simpsons", "GOOD MORNING Simpsons")]
        [TestCase("Simpsons INTL", "Simpsons INTERNATIONAL")]
        [TestCase("Simpsons WMN", "Simpsons WOMEN")]
        public void GetCleanProgram_ApplyReplacement(string programName, string expected)
        {
            var result = _ProgramMappingCleanupEngine.GetCleanProgram(programName);

            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase("Simpsons, The","The Simpsons")]
        [TestCase("SIMPSONS, THE", "THE SIMPSONS")]
        [TestCase("sImPsOnS, tHe", "tHe sImPsOnS")]
        [TestCase("Simpsons, A", "A Simpsons")]
        [TestCase("Asimpsons, An", "An Asimpsons")]
        public void GetCleanProgram__InvertPrepositions(string programName, string expected)
        {
            var result = _ProgramMappingCleanupEngine.GetCleanProgram(programName);

            Assert.AreEqual(expected, result);
        }
    }
}
