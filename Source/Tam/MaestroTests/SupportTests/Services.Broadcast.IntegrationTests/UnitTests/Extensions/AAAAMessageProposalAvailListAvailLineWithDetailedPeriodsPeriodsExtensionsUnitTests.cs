using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using Services.Broadcast.Extensions;
using System;

namespace Services.Broadcast.IntegrationTests.UnitTests.Extensions
{
    public class AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsExtensionsUnitTests
    {
        [Test]
        public void GetAllPeriods_Null()
        {
            AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriods request = null;

            var result = request.GetAllPeriods();

            Assert.IsEmpty(result);
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllPeriods()
        {
            var request = new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriods
            {
                DayDetailedPeriod = new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriod[3]
                 {
                     new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriod() {
                         DemoValues = new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriodDemoValue[2] {
                             new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriodDemoValue { demoRef = "demo7", Value = 18} ,
                             new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriodDemoValue { demoRef = "demo8", Value = 17.5m} ,
                         },
                        startDate = new DateTime(2020,6,1),
                        endDate = new DateTime(2020,6,10),
                        Rate = "E"
                     },
                     new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriod() {
                         DemoValues = new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriodDemoValue[2] {
                             new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriodDemoValue { demoRef = "demo9", Value = 25.01m} ,
                             new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriodDemoValue { demoRef = "demo10", Value = 9.81m} ,
                         },
                        startDate = new DateTime(2020,6,11),
                        endDate = new DateTime(2020,6,15),
                        Rate = "E"
                     },
                     new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriod() {
                         DemoValues = new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriodDemoValue[2] {
                             new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriodDemoValue { demoRef = "demo11", Value = 5} ,
                             new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDayDetailedPeriodDemoValue { demoRef = "demo12", Value = 8.9m} ,
                         },
                        startDate = new DateTime(2020,6,17),
                        endDate = new DateTime(2020,6,30),
                        Rate = "E"
                     },
                 },
                DetailedPeriod = new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriod[3]
                 {
                     new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriod(){
                         Rate = "E",
                         endDate = new DateTime(2020,7,13),
                         startDate = new DateTime(2020,7,10),
                         DemoValues = new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue[2] {
                             new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue { demoRef = "demo1", Value = 10} ,
                             new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue { demoRef = "demo2", Value = 11} ,
                         }
                     },
                     new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriod(){
                         Rate = "E",
                         endDate = new DateTime(2020,7,20),
                         startDate = new DateTime(2020,7,10),
                         DemoValues = new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue[2] {
                             new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue { demoRef = "demo3", Value = 1.99m} ,
                             new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue { demoRef = "demo4", Value = 20} ,
                         }
                     },
                     new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriod(){
                         Rate = "E",
                         endDate = new DateTime(2020,7,9),
                         startDate = new DateTime(2020,7,1),
                         DemoValues = new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue[2] {
                             new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue { demoRef = "demo6", Value = 8} ,
                             new AAAAMessageProposalAvailListAvailLineWithDetailedPeriodsPeriodsDetailedPeriodDemoValue { demoRef = "demo5", Value = 7.5m} ,
                         }
                     },
                 }
            };

            var result = request.GetAllPeriods();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

    }
}
