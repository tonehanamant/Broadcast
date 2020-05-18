using ApprovalTests;
using ApprovalTests.Reporters;
using Moq;
using NUnit.Framework;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using System;
using System.Collections.Generic;

namespace Services.Broadcast.IntegrationTests.UnitTests.BusinessEngines
{
    [Category("short_running")]
    public class CreativeLengthEngineUnitTests
    {
        private ICreativeLengthEngine _CreativeLengthEngine;
        private Mock<ISpotLengthEngine> _SpotLengthEngineMock;

        [SetUp]
        public void SetUp()
        {
            _SpotLengthEngineMock = new Mock<ISpotLengthEngine>();
           
            _CreativeLengthEngine = new CreativeLengthEngine(_SpotLengthEngineMock.Object);
        }

        [Test]
        public void CreativeLength_NoWeightValueSet()
        {
            _SpotLengthEngineMock.Setup(s => s.SpotLengthIdExists(It.IsAny<int>())).Returns(true);

            var creativeLenghts = _GetCreativeLengths();
            //remove the first creative length because it has a value selected
            creativeLenghts[0].Weight = null;

            Assert.DoesNotThrow(() => _CreativeLengthEngine.ValidateCreativeLengths(creativeLenghts));
        }
        
        [Test]
        public void CreativeLength_None()
        {
            var exception = Assert.Throws<ApplicationException>(() => _CreativeLengthEngine.ValidateCreativeLengths(new List<CreativeLength>()));

            Assert.AreEqual("There should be at least 1 creative length selected on the plan", exception.Message);
        }

        [Test]
        [TestCase(100)]
        [TestCase(0)]
        [TestCase(-1)]
        public void CreativeLength_InvalidSpotLengthId(int spotLengthId)
        {
            _SpotLengthEngineMock.Setup(s => s.SpotLengthIdExists(spotLengthId)).Returns(false);

            var creativeLengths = _GetCreativeLengths();
            creativeLengths[0].SpotLengthId = spotLengthId;

            var exception = Assert.Throws<ApplicationException>(() => _CreativeLengthEngine.ValidateCreativeLengths(creativeLengths));

            Assert.AreEqual($"Invalid spot length id {spotLengthId}", exception.Message);
        }

        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(101)]
        public void CreativeLength_InvalidWeight(int weight)
        {
            _SpotLengthEngineMock.Setup(s => s.SpotLengthIdExists(It.IsAny<int>())).Returns(true);

            var creativeLengths = _GetCreativeLengths();
            creativeLengths[0].Weight = weight;

            var exception = Assert.Throws<ApplicationException>(() => _CreativeLengthEngine.ValidateCreativeLengths(creativeLengths));

            Assert.AreEqual("Creative length weight must be between 1 and 100", exception.Message);
        }

        [Test]
        [TestCase(20)]
        [TestCase(60)]
        public void CreativeLength_InvalidSum(int weight)
        {
            _SpotLengthEngineMock.Setup(s => s.SpotLengthIdExists(It.IsAny<int>())).Returns(true);

            var creativeLengths = _GetCreativeLengths();
            creativeLengths[0].Weight = weight;

            var exception = Assert.Throws<ApplicationException>(() => _CreativeLengthEngine.ValidateCreativeLengthsForPlanSave(creativeLengths));

            Assert.AreEqual("Sum Weight of all Creative Lengths must equal 100%", exception.Message);
        }

        [Test]
        public void CreativeLength_InvalidPartialSum()
        {
            _SpotLengthEngineMock.Setup(s => s.SpotLengthIdExists(It.IsAny<int>())).Returns(true);

            var creativeLengths = _GetCreativeLengths();
            creativeLengths.Add(new CreativeLength { SpotLengthId = 4, Weight = null });
            creativeLengths[0].Weight = 60;

            var exception = Assert.Throws<ApplicationException>(() => _CreativeLengthEngine.ValidateCreativeLengthsForPlanSave(creativeLengths));

            Assert.AreEqual("Creative length weight must be between 1 and 100", exception.Message);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CreativeLength_UnevenDistribution()
        {
            _SpotLengthEngineMock.Setup(s => s.SpotLengthIdExists(It.IsAny<int>())).Returns(true);

            var creativeLengths = new List<CreativeLength>
            {
                new CreativeLength{ SpotLengthId = 1, Weight = 2},
                new CreativeLength { SpotLengthId = 2},
                new CreativeLength { SpotLengthId = 3},
                new CreativeLength { SpotLengthId = 4},
            };

            var result = _CreativeLengthEngine.DistributeWeight(creativeLengths);
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        private List<CreativeLength> _GetCreativeLengths()
        {
            return new List<CreativeLength> {
                    new CreativeLength { SpotLengthId = 1, Weight = 50 },
                    new CreativeLength { SpotLengthId = 2, Weight = 50 }
                };
        }
    }
}
