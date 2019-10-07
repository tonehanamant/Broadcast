using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices.Plan;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Plan;
using System;
using System.Collections.Generic;
using Tam.Maestro.Common.DataLayer;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class PlanServiceIntegrationTests
    {
        private readonly IPlanService _PlanService = IntegrationTestApplicationServiceFactory.GetApplicationService<IPlanService>();

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanStatuses()
        {
            var statuses = _PlanService.GetPlanStatuses();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(statuses));
        }

        [Test]
        public void CreateNewPlan()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                Assert.IsTrue(newPlanId > 0);
            }
        }

        [Test]
        public void CreatePlan_InvalidSpotLengthId()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.SpotLengthId = 100;

                var exception = Assert.Throws<Exception>(() => _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01)));

                Assert.That(exception.Message, Is.EqualTo("Invalid spot length"));
            }
        }

        [Test]
        public void CreatePlan_NotExistingProduct()
        {
            const int notExistingProductId = 666;

            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.ProductId = notExistingProductId;

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 01)),
                    Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid product"));
            }
        }

        [Test]
        public void CreatePlan_InvalidName()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Name = null;

                var exception = Assert.Throws<Exception>(() => _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01)));

                Assert.That(exception.Message, Is.EqualTo("Invalid plan name"));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlan()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));

                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(testPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlan()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                // modify the plan
                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.Name = "Renamed Plan";
                testPlan.ProductId = 2;
                // modify the flight.
                testPlan.FlightNotes = "Changed the flight notes";
                testPlan.FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019, 1, 28),
                    new DateTime(2019, 1, 4)
                };

                var modifedPlanId = _PlanService.SavePlan(testPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifedPlanId);

                Assert.IsTrue(modifedPlanId > 0);
                Assert.AreEqual(newPlanId, modifedPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanAndRemoveHiatusDays()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                // modify the plan
                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.Name = "Renamed Plan";
                testPlan.ProductId = 2;
                // modify the flight.
                testPlan.FlightNotes = "Changed the flight notes";
                testPlan.FlightHiatusDays = new List<DateTime>();

                var modifedPlanId = _PlanService.SavePlan(testPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifedPlanId);

                Assert.IsTrue(modifedPlanId > 0);
                Assert.AreEqual(newPlanId, modifedPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        public void SavePlan_WithoutFlightInfo()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto testPlan = _GetNewPlan();
                testPlan.FlightStartDate = null;
                testPlan.FlightEndDate = null;
                testPlan.FlightNotes = null;
                testPlan.FlightHiatusDays.Clear();

                Assert.That(() => _PlanService.SavePlan(testPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight start/end date."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidFlightDays()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.FlightStartDate = new DateTime(2019, 10, 1);
                newPlan.FlightEndDate = new DateTime(2018, 01, 01);

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight dates.  The end date cannot be before the start date."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidHiatusDays()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(1968, 1, 28),
                    new DateTime(1976, 6, 4)
                };

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight hiatus day.  All days must be within the flight date range."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanWithDayparts()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();

                var planId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(planId);

                Assert.AreEqual(3, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanAddDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                var planId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto modifiedPlan = _PlanService.GetPlan(planId);
                modifiedPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 3, DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews, StartTimeSeconds = 1200, EndTimeSeconds = 1900, WeightingGoalPercent = 13.8 });

                var modifiedPlanId = _PlanService.SavePlan(modifiedPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifiedPlanId);

                Assert.AreEqual(4, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlanRemoveDaypart()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                var planId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto modifiedPlan = _PlanService.GetPlan(planId);
                modifiedPlan.Dayparts.RemoveAt(modifiedPlan.Dayparts.Count - 1);


                var modifiedPlanId = _PlanService.SavePlan(modifiedPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifiedPlanId);

                Assert.AreEqual(2, finalPlan.Dayparts.Count);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        public void SavePlan_WithInvalidWeightingGoalTooLittle()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1, StartTimeSeconds = 8900, EndTimeSeconds = 4600, WeightingGoalPercent = 0.0 });

                Assert.Throws<Exception>(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), "Invalid daypart weighting goal.");
            }
        }

        [Test]
        public void SavePlan_WithInvalidWeightingGoalTooHigh()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1, StartTimeSeconds = 4600, EndTimeSeconds = 8900, WeightingGoalPercent = 111.0 });

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart weighting goal."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlan_WithSecondaryAudiences()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.SecondaryAudiences = new List<PlanAudienceDto>()
                {
                    new PlanAudienceDto {AudienceId = 7, Type = Entities.Enums.AudienceTypeEnum.Nielsen, Vpvh = 0.435},
                    new PlanAudienceDto {AudienceId = 6, Type = Entities.Enums.AudienceTypeEnum.Nielsen, Vpvh = 0.635}
                };

                var planId = _PlanService.SavePlan(newPlan, "integration_test", new DateTime(2019, 01, 01));
                Assert.IsTrue(planId > 0);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(_PlanService.GetPlan(planId), _GetJsonSettings()));
            }
        }

        [Test]
        public void SavePlan_WithInvalidSecondaryAudience()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.SecondaryAudiences = new List<PlanAudienceDto>()
                {
                    new PlanAudienceDto {AudienceId = 0, Type = Entities.Enums.AudienceTypeEnum.Nielsen},
                };

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid audience"));
            }
        }

        [Test]
        public void SavePlan_WithInvalidSecondaryAudienceDuplicate()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.SecondaryAudiences = new List<PlanAudienceDto>()
                {
                    new PlanAudienceDto {AudienceId = 31, Type = Entities.Enums.AudienceTypeEnum.Nielsen},
                };

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("An audience cannot appear multiple times"));
            }
        }

        [Test]
        public void SavePlan_WitInvalidFlightNotes()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto testPlan = _GetNewPlan();
                testPlan.FlightNotes = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque porttitor tellus at ante tempus vehicula ac at sapien. Pellentesque lorem velit, sodales in ex quis, laoreet dictum risus. Quisque odio sapien, dignissim a lacus et, dignissim auctor urna. Vestibulum tempus dui tortor, nec fermentum massa pharetra sit amet. Morbi fermentum ornare scelerisque. Proin ut lectus in nisl vulputate mattis in in ex. Nam erat sem, convallis condimentum velit blandit, scelerisque condimentum dolor. Maecenas fermentum feugiat lectus. Phasellus et sem in velit hendrerit sodales. Suspendisse porta nec felis ac blandit. In eu nisi ut dui tristique mattis. Vivamus vulputate, elit sit amet porta molestie, justo mauris cursus ipsum, et rhoncus arcu odio id enim. Pellentesque elementum posuere nibh ac rutrum. Donec eget erat nec lorem feugiat ornare vel congue nibh. Nulla cursus bibendum sollicitudin. Quisque viverra ante massa, sed molestie augue rutrum sed. Aenean tempus vitae purus sed lobortis. Sed cursus tempor erat ac pulvinar.";

                Assert.That(() => _PlanService.SavePlan(testPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Flight notes cannot be longer than 1024 characters."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanCurrencies()
        {
            var currencies = _PlanService.GetPlanCurrencies();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(currencies));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetPlanDeliveryTypes()
        {
            var deliveryTypes = _PlanService.PlanGoalBreakdownTypes();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(deliveryTypes));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase1()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                Budget = 100m,
                DeliveryImpressions = 3000d,
                AudienceId = 31,
                MediaMonthId = 437
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase2()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPM = 100m,
                DeliveryImpressions = 3000d,
                AudienceId = 31,
                MediaMonthId = 437
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase3()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPM = 100m,
                Budget = 3000m,
                AudienceId = 31,
                MediaMonthId = 437
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase4()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPP = 100m,
                Budget = 3000m,
                AudienceId = 31,
                MediaMonthId = 437
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase5()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPP = 100m,
                DeliveryRatingPoints = 3000d,
                AudienceId = 31,
                MediaMonthId = 437
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase6()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                Budget = 100m,
                DeliveryRatingPoints = 3000d,
                AudienceId = 31,
                MediaMonthId = 437
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase7()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                Budget = 1000,
                DeliveryRatingPoints = 0,
                DeliveryImpressions = 25000,
                AudienceId = 34,
                MediaMonthId = 437
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase8()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPM = 13.45m,
                DeliveryImpressions = 13.456d,
                AudienceId = 31,
                MediaMonthId = 437
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase9()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                Budget = 3000,
                DeliveryImpressions = 3000d,
                AudienceId = 31,
                MediaMonthId = 437
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase10()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPM = 1,
                DeliveryImpressions = 3000d,
                AudienceId = 31,
                MediaMonthId = 437
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Calculator_TestCase11()
        {
            var result = _PlanService.Calculate(new PlanDeliveryBudget
            {
                CPM = 1,
                Budget = 3000,
                AudienceId = 31,
                MediaMonthId = 437
            });
            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        public void Calculator_WithoutMediaMonthId()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.Calculate(new PlanDeliveryBudget
                {
                    Budget = null,
                    CPM = null,
                    DeliveryImpressions = null,
                    AudienceId = 5
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Cannot calculate goal without media month and audience"));
            }
        }

        [Test]
        public void Calculator_WithoutAudienceId()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.Calculate(new PlanDeliveryBudget
                {
                    Budget = null,
                    CPM = null,
                    DeliveryImpressions = null,
                    MediaMonthId = 437,
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Cannot calculate goal without media month and audience"));
            }
        }

        [Test]
        public void Calculator_InvalidObject()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.Calculate(new PlanDeliveryBudget
                {
                    Budget = null,
                    CPM = null,
                    DeliveryImpressions = null,
                    MediaMonthId = 437,
                    AudienceId = 5
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("At least 2 values needed to calculate goal amount"));
            }
        }

        [Test]
        public void Calculator_InvalidObject2()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.Calculate(new PlanDeliveryBudget
                {
                    Budget = 1,
                    CPM = 1,
                    DeliveryImpressions = 1,
                    AudienceId = 0
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Cannot calculate goal without media month and audience"));
            }
        }

        [Test]
        public void Calculator_InvalidObject_NegativeValues()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.Calculate(new PlanDeliveryBudget
                {
                    Budget = -1,
                    CPM = null,
                    DeliveryImpressions = null
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid budget values passed"));
            }
        }

        [Test]
        public void SavePlan_WithInvalidStartTime_TooLittle()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1, StartTimeSeconds = -2, EndTimeSeconds = 4600, WeightingGoalPercent = 111.0 });

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidStartTime_TooMuch()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1, StartTimeSeconds = 999999999, EndTimeSeconds = 4600, WeightingGoalPercent = 111.0 });

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidEndTime_TooLittle()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1, StartTimeSeconds = 8900, EndTimeSeconds = -2, WeightingGoalPercent = 111.0 });

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlan_WithEmptyCoverageGoal()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                newPlan.CoverageGoalPercent = null;

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid coverage goal value."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidCoverageGoalTooSmall()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.CoverageGoalPercent = -1;

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid coverage goal value."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidCoverageGoalTooBig()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.CoverageGoalPercent = 120;

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid coverage goal value."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void SavePlan_WithEmptyBlackoutMarkets()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                newPlan.BlackoutMarkets.Clear();
                var newPlanId = _PlanService.SavePlan(newPlan, "integration_test", new System.DateTime(2019, 01, 01));
                // modify the plan
                PlanDto testPlan = _PlanService.GetPlan(newPlanId);

                testPlan.Name = "Renamed Plan";
                testPlan.ProductId = 2;
                // modify the flight.
                testPlan.FlightNotes = "Changed the flight notes";
                testPlan.FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019, 1, 28),
                    new DateTime(2019, 1, 4)
                };

                var modifedPlanId = _PlanService.SavePlan(testPlan, "integration_test", new System.DateTime(2019, 01, 15));
                PlanDto finalPlan = _PlanService.GetPlan(modifedPlanId);

                Assert.IsTrue(modifedPlanId > 0);
                Assert.AreEqual(newPlanId, modifedPlanId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(finalPlan, _GetJsonSettings()));
            }
        }

        [Test]
        public void SavePlan_WithEmptyAvailableMarkets()
        {
            using (new TransactionScopeWrapper())
            {
                // generate a plan for test
                PlanDto newPlan = _GetNewPlan();
                newPlan.AvailableMarkets.Clear();

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid total market coverage."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidAvailableMarketShareOfVoiceTooSmall()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.AvailableMarkets[0].ShareOfVoicePercent = -1;

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid share of voice for market."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidAvailableMarketShareOfVoiceTooBig()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.AvailableMarkets[0].ShareOfVoicePercent = 120;

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid share of voice for market."));
            }
        }

        [Test]
        public void SavePlan_WithInvalidEndTime_TooMuch()
        {
            using (new TransactionScopeWrapper())
            {
                PlanDto newPlan = _GetNewPlan();
                newPlan.Dayparts.Add(new PlanDaypartDto { DaypartCodeId = 1, StartTimeSeconds = 8900, EndTimeSeconds = 999999999, WeightingGoalPercent = 111.0 });

                Assert.That(() => _PlanService.SavePlan(newPlan, "integration_test",
                    new DateTime(2019, 01, 01)), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid daypart times."));
            }
        }

        [Test]
        public void Plan_WeeklyBreakdown_InvalidRequest()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculatePlanWeeklyGoalBreakdown(null), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid request"));
            }
        }

        [Test]
        public void Plan_WeeklyBreakdown_InvalidFlight()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
                {
                    FlightEndDate = default,
                    FlightStartDate = default
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight start/end date."));
            }
        }

        [Test]
        public void Plan_WeeklyBreakdown_InvalidFlightStartDate()
        {
            using (new TransactionScopeWrapper())
            {
                Assert.That(() => _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
                {
                    FlightEndDate = default,
                    FlightStartDate = new DateTime(2019, 01, 01)
                }), Throws.TypeOf<Exception>().With.Message.EqualTo("Invalid flight start/end date."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_CustomDelivery_InitialRequest()
        {
            using (new TransactionScopeWrapper())
            {
                var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
                {
                    FlightEndDate = new DateTime(2019, 03, 05),
                    FlightStartDate = new DateTime(2019, 02, 01),
                    DeliveryType = PlanGoalBreakdownTypeEnum.Custom,
                    FlightHiatusDays = new List<DateTime>(),
                    TotalImpressions = 10000
                });
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_OneHiatusDay()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 08, 03),
                FlightEndDate = new DateTime(2019, 08, 27),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 15) },
                TotalImpressions = 1000
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_RoundShareOfVoiceTotal()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 08, 05),
                FlightEndDate = new DateTime(2019, 09, 19),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 15) },
                TotalImpressions = 1000
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_PRI15094()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = Entities.Enums.PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 09, 29),
                FlightEndDate = new DateTime(2019, 10, 13),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 10, 10), new DateTime(2019, 10, 12), new DateTime(2019, 10, 4), new DateTime(2019, 10, 2) },
                TotalImpressions = 20095158400
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_OneWeekHiatus()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = Entities.Enums.PlanGoalBreakdownTypeEnum.Even,
                FlightStartDate = new DateTime(2019, 08, 01),
                FlightEndDate = new DateTime(2019, 08, 31),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 5), new DateTime(2019, 8, 6), new DateTime(2019, 8, 7), new DateTime(2019, 8, 8),
                                                        new DateTime(2019, 8, 9), new DateTime(2019, 8, 10), new DateTime(2019, 8, 11)},
                TotalImpressions = 1000
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_CustomDelivery()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = Entities.Enums.PlanGoalBreakdownTypeEnum.Custom,
                FlightStartDate = new DateTime(2019, 08, 03),
                FlightEndDate = new DateTime(2019, 08, 20),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 8, 15) },
                TotalImpressions = 1000,
                Weeks = new List<WeeklyBreakdownWeek> { new WeeklyBreakdownWeek {
                      ActiveDays= "Sa,Su",
                      EndDate= new DateTime(2019,08,4),
                      Impressions= 200.0,
                      MediaWeekId= 814,
                      NumberOfActiveDays= 2,
                      ShareOfVoice= 20.0,
                      StartDate= new DateTime(2019,07,29),
                      WeekNumber= 1
                }}
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void Plan_WeeklyBreakdown_UpdateHiatusDaysInCustomDelivery()
        {
            var result = _PlanService.CalculatePlanWeeklyGoalBreakdown(new WeeklyBreakdownRequest
            {
                DeliveryType = PlanGoalBreakdownTypeEnum.Custom,
                FlightStartDate = new DateTime(2019, 09, 30),
                FlightEndDate = new DateTime(2019, 10, 13),
                FlightHiatusDays = new List<DateTime> { new DateTime(2019, 10, 2) },
                TotalImpressions = 1000,
                Weeks = new List<WeeklyBreakdownWeek> {
                    new WeeklyBreakdownWeek {
                      ActiveDays= "",
                      EndDate= new DateTime(2019,10,6),
                      Impressions= 500,
                      MediaWeekId= 814,
                      NumberOfActiveDays= 7,
                      ShareOfVoice= 50,
                      StartDate= new DateTime(2019,09,30),
                      WeekNumber= 1
                    },
                    new WeeklyBreakdownWeek {
                      ActiveDays= "",
                      EndDate= new DateTime(2019,10,13),
                      Impressions= 500,
                      MediaWeekId= 814,
                      NumberOfActiveDays= 7,
                      ShareOfVoice= 50,
                      StartDate= new DateTime(2019,10,7),
                      WeekNumber= 2
                    },
                }
            });

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void PlanServiceGetPlanDefaultsTest()
        {
            var defaults = _PlanService.GetPlanDefaults();

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(defaults));
        }

        private static PlanDto _GetNewPlan()
        {
            return new PlanDto
            {
                CampaignId = 1,
                Equivalized = true,
                Name = "New Plan",
                ProductId = 1,
                SpotLengthId = 1,
                Status = PlanStatusEnum.Working,
                FlightStartDate = new DateTime(2019, 1, 1),
                FlightEndDate = new DateTime(2019, 1, 31),
                FlightNotes = "Sample notes",
                FlightHiatusDays = new List<DateTime>
                {
                    new DateTime(2019,1,20),
                    new DateTime(2019,1,24)
                },
                AudienceId = 31,        //HH
                AudienceType = AudienceTypeEnum.Nielsen,
                HUTBookId = 437,
                PostingType = PostingTypeEnum.NTI,
                ShareBookId = 437,
                Budget = 100m,
                CPM = 12m,
                DeliveryImpressions = 100d,
                CPP = 12m,
                Currency = PlanCurrenciesEnum.Impressions,
                DeliveryRatingPoints = 100d,
                CoverageGoalPercent = 80.5,
                GoalBreakdownType = PlanGoalBreakdownTypeEnum.Even,
                AvailableMarkets = new List<PlanAvailableMarketDto>
                {
                    new PlanAvailableMarketDto { MarketCode = 100, MarketCoverageFileId = 1, PercentageOfUS = 48, Rank = 1, ShareOfVoicePercent = 22.2, Market = "Portland-Auburn"},
                    new PlanAvailableMarketDto { MarketCode = 101, MarketCoverageFileId = 1, PercentageOfUS = 32.5, Rank = 2, ShareOfVoicePercent = 34.5, Market = "New York"}
                },
                BlackoutMarkets = new List<PlanBlackoutMarketDto>
                {
                    new PlanBlackoutMarketDto {MarketCode = 123, MarketCoverageFileId = 1, PercentageOfUS = 5.5, Rank = 5, Market = "Burlington-Plattsburgh" },
                    new PlanBlackoutMarketDto {MarketCode = 234, MarketCoverageFileId = 1, PercentageOfUS = 2.5, Rank = 8, Market = "Amarillo" },
                },
                ModifiedBy = "Test User",
                ModifiedDate = new DateTime(2019, 01, 12, 12, 30, 29),
                Dayparts = new List<PlanDaypartDto>
                {
                    new PlanDaypartDto{ DaypartCodeId = 2, DaypartTypeId = DaypartTypeEnum.EntertainmentNonNews, StartTimeSeconds = 0, EndTimeSeconds = 2000, WeightingGoalPercent = 28.0 },
                    new PlanDaypartDto{ DaypartCodeId = 11, DaypartTypeId = DaypartTypeEnum.News, StartTimeSeconds = 1500, EndTimeSeconds = 2788, WeightingGoalPercent = 33.2 },
                    new PlanDaypartDto{ DaypartCodeId = 3, DaypartTypeId = DaypartTypeEnum.ROS, StartTimeSeconds = 1550, EndTimeSeconds = 3000, WeightingGoalPercent = 25 }
                },
                Vpvh = 0.012,
                WeeklyBreakdownWeeks = new List<WeeklyBreakdownWeek>
                {
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 1, MediaWeekId = 784,
                        StartDate = new DateTime(2018,12,31), EndDate = new DateTime(2019,01,06),
                        NumberOfActiveDays = 6, ActiveDays = "Tu-Su", Impressions = 20, ShareOfVoice = 20
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 2, MediaWeekId = 785,
                        StartDate = new DateTime(2019,01,07), EndDate = new DateTime(2019,01,13),
                        NumberOfActiveDays = 7, ActiveDays = "M-Su", Impressions = 20, ShareOfVoice = 20
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 3, MediaWeekId = 786,
                        StartDate = new DateTime(2019,01,14), EndDate = new DateTime(2019,01,20),
                        NumberOfActiveDays = 6, ActiveDays = "M-Sa", Impressions = 20, ShareOfVoice = 20
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 4, MediaWeekId = 787,
                        StartDate = new DateTime(2019,01,21), EndDate = new DateTime(2019,01,27),
                        NumberOfActiveDays = 6, ActiveDays = "M-W,F-Su", Impressions = 20, ShareOfVoice = 20
                    },
                    new WeeklyBreakdownWeek
                    {
                        WeekNumber = 5, MediaWeekId = 788,
                        StartDate = new DateTime(2019,01,28), EndDate = new DateTime(2019,02,03),
                        NumberOfActiveDays = 4, ActiveDays = "M-Th", Impressions = 20, ShareOfVoice = 20
                    }
                }
            };
        }

        private JsonSerializerSettings _GetJsonSettings()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();

            jsonResolver.Ignore(typeof(PlanDto), "Id");
            jsonResolver.Ignore(typeof(PlanDaypartDto), "Id");
            jsonResolver.Ignore(typeof(PlanMarketDto), "Id");
            jsonResolver.Ignore(typeof(PlanSummaryDto), "PlanId");

            return new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };
        }
    }
}
