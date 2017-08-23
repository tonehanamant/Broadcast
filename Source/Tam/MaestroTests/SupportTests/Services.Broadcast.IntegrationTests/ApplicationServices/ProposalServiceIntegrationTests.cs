﻿using ApprovalTests;
using ApprovalTests.Reporters;
using IntegrationTests.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using Services.Broadcast.ApplicationServices;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.OpenMarketInventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Services.Broadcast.Repositories;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities.DataTransferObjects;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProposalServiceIntegrationTests
    {
        private readonly IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
        private readonly DateTime _CurrentDateTime = new DateTime(2016, 02, 15);
        private readonly IProposalProprietaryInventoryService _ProposalProprietaryInventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalProprietaryInventoryService>();

        private static ProposalDto _setupProposalDto()
        {
            return new ProposalDto()
            {
                ForceSave = true,
                AdvertiserId = 37444,
                ProposalName = "Proposal Test",
                GuaranteedDemoId = 31,
                MarketGroupId = ProposalEnums.ProposalMarketGroups.Top100,
                BlackoutMarketGroupId = ProposalEnums.ProposalMarketGroups.All,
                Markets = new List<ProposalMarketDto>()
                {
                    new ProposalMarketDto
                    {
                        Id = 100,
                        IsBlackout = false
                    },
                    new ProposalMarketDto
                    {
                        Id = 196,
                        IsBlackout = true
                    }
                },
                TargetUnits = 1,
                Equivalized = false,
                PostType = SchedulePostType.NSI
            };
        }

        private static ProposalDetailDto _setupProposalDetailDto()
        {
            return new ProposalDetailDto
            {
                FlightStartDate = new DateTime(2016, 05, 30),
                FlightEndDate = new DateTime(2016, 06, 05),
                Daypart = new DaypartDto() { mon = true, tue = true },
                SpotLengthId = 1,
                DaypartCode = "NAV",
                SharePostingBookId = 413,
                HutPostingBookId = 410,
                PlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3,
                Quarters = new List<ProposalQuarterDto>
                {
                    new ProposalQuarterDto
                    {
                        Year = 2016,
                        Quarter = 1,
                        Cpm = 21.13m,
                        ImpressionGoal = 1000.123,
                        Weeks = new List<ProposalWeekDto>
                        {
                            new ProposalWeekDto
                            {
                                Cost = 21.89m,
                                Impressions = 1000.123,
                                Units = 3,
                                MediaWeekId = 649,
                                StartDate = new DateTime(2016, 05, 30),
                                EndDate = new DateTime(2016, 06, 05)
                            }
                        }
                    }
                }
            };
        }

        [Test]
        public void CanAddProposal()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _setupProposalDto();

                var result = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);
                Assert.IsTrue(result.Id.Value > 0);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanAddProposalWithSecondaryDemos()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _setupProposalDto();
                proposalDto.SecondaryDemos.Add(3);
                proposalDto.SecondaryDemos.Add(4);

                var proposal = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "CacheGuid");
                jsonResolver.Ignore(typeof(ProposalDto), "HutBookMonthId");
                jsonResolver.Ignore(typeof(ProposalDto), "VersionId");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDto), "ShareBookMonthId");
                jsonResolver.Ignore(typeof(ProposalDto), "SweepMonthId");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposal, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadProposalByIdWithSecondaryDemos()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(248);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "CacheGuid");
                jsonResolver.Ignore(typeof(ProposalDto), "HutBookMonthId");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDto), "ShareBookMonthId");
                jsonResolver.Ignore(typeof(ProposalDto), "SweepMonthId");
                jsonResolver.Ignore(typeof(ProposalDto), "FlightWeeks");
                jsonResolver.Ignore(typeof(ProposalDto), "Details");
                jsonResolver.Ignore(typeof(ProposalDto), "SpotLengths");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposal, jsonSettings));
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot save proposal without specifying name value", MatchType = MessageMatch.Contains)]
        public void ThrowExceptionWhenProposalNameIsInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _setupProposalDto();
                proposalDto.ProposalName = string.Empty;
                var res = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);
                Assert.IsTrue(res.Id > 0);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot find advertiser", MatchType = MessageMatch.Contains)]
        public void ThrowExceptionWhenAdvertiserIsInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _setupProposalDto();
                proposalDto.AdvertiserId = 0;
                var res = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);
                Assert.IsTrue(res.Id > 0);
            }
        }

        [Test]
        public void CanFindProposalById()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(248);
                Assert.IsNotNull(proposal);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "The Proposal information you have entered", MatchType = MessageMatch.Contains)]
        public void ThrowExceptionWhenProposalIdIsInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var res = _ProposalService.GetProposalById(0);
            }
        }

        [Test]
        public void CanLoadAllProposals()
        {
            using (new TransactionScopeWrapper())
            {
                var res = _ProposalService.GetAllProposals();
                Assert.IsNotEmpty(res);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadInitialProposalData()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var result = _ProposalService.GetInitialProposalData(_CurrentDateTime);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetAllProposalVersionsForAProposal()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalId = 249;
                var result = _ProposalService.GetProposalVersionsByProposalId(proposalId);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetSpecificProposalVersion()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalId = 249;
                var proposalVersion = 3;
                var result = _ProposalService.GetProposalByIdWithVersion(proposalId, proposalVersion);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDto), "ProposalProgramFilter");
                jsonResolver.Ignore(typeof(ProposalDto), "ProposalPrograms");
                jsonResolver.Ignore(typeof(ProposalDto), "CacheGuid");
                jsonResolver.Ignore(typeof(ProposalDto), "ProposalProgramsDto");
                jsonResolver.Ignore(typeof(ProposalDto), "ProgramDisplayFilter");
                jsonResolver.Ignore(typeof(ProposalDto), "ProgramFilter");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "FlightEndDate");
                jsonResolver.Ignore(typeof(ProposalDto), "FlightStartDate");


                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanCreateNewProposalVersion()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(248);
                //setting to null should create proposal version 2
                proposalDto.Version = null;

                var newProposal = _ProposalService.SaveProposal(proposalDto, "Integration Test", _CurrentDateTime);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDto), "VersionId");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");


                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(newProposal, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadLatestProposalVersion()
        {
            using (new TransactionScopeWrapper())
            {
                // should load proposal 249 version 3
                var proposalDto = _ProposalService.GetProposalById(249);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Details");
                jsonResolver.Ignore(typeof(ProposalDto), "FlightWeeks");
                jsonResolver.Ignore(typeof(ProposalDto), "SecondaryDemos");
                jsonResolver.Ignore(typeof(ProposalDto), "SpotLengths");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "FlightEndDate");
                jsonResolver.Ignore(typeof(ProposalDto), "FlightStartDate");


                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void EditProposal()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(249);
                proposalDto.ProposalName = "Edited Proposal Test";
                proposalDto.MarketGroupId = ProposalEnums.ProposalMarketGroups.Top50;
                proposalDto.Equivalized = true;
                proposalDto.PostType = SchedulePostType.NTI;

                var proposalDetailDto = _setupProposalDetailDto();

                proposalDetailDto.FlightStartDate = new DateTime(2016, 05, 30);
                proposalDetailDto.FlightEndDate = new DateTime(2016, 06, 05);
                proposalDetailDto.Daypart = new DaypartDto() { mon = true, tue = true };
                proposalDetailDto.SpotLengthId = 1;
                proposalDetailDto.DaypartCode = "NAV";

                proposalDto.Details.Add(proposalDetailDto);

                var res = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDto), "CacheGuid");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(res, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanSaveProposalWithOnlyDefaultValues()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposalDto = new ProposalDto()
                {
                    AdvertiserId = 37444,
                    ProposalName = "Proposal Test"
                };

                var proposal = _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDto), "VersionId");
                jsonResolver.Ignore(typeof(ProposalDto), "ProgramDisplayFilter");
                jsonResolver.Ignore(typeof(ProposalDto), "ProgramFilter");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "CacheGuid");
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposal, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void GetProposalDetails()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposalDetailDto = new ProposalDetailRequestDto()
                {
                    StartDate = new DateTime(2016, 12, 5),
                    EndDate = new DateTime(2016, 12, 31),
                    FlightWeeks = new List<FlightWeekDto>
                    {
                        new FlightWeekDto
                        {
                            StartDate = new DateTime(2016, 12, 5),
                            EndDate = new DateTime(2016, 12, 11),
                            IsHiatus = true
                        }
                    }
                };

                var proposalDetail = _ProposalService.GetProposalDetail(proposalDetailDto);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");


                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalDetail, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateProposal()
        {
            var proposalDto = _ProposalService.GetProposalById(250);

            var updatedProposalDto = _ProposalService.UpdateProposal(proposalDto.Details);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
            jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
            jsonResolver.Ignore(typeof(ProposalDto), "Markets");
            jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(updatedProposalDto, jsonSettings));
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot save proposal detail without specifying flight start/end date", MatchType = MessageMatch.Contains)]
        public void ThrowExceptionWhenProposalDetailFlightIsInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _setupProposalDto();
                var proposalDetailDto = _setupProposalDetailDto();

                proposalDetailDto.FlightEndDate = default(DateTime);

                proposalDto.Details.Add(proposalDetailDto);

                _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot save proposal detail without specifying daypart", MatchType = MessageMatch.Contains)]
        public void ThrowExceptionWhenProposalDetailDaypartIsInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _setupProposalDto();
                var proposalDetailDto = _setupProposalDetailDto();

                proposalDetailDto.FlightStartDate = new DateTime(2016, 12, 5);
                proposalDetailDto.FlightEndDate = new DateTime(2016, 12, 11);
                proposalDetailDto.Daypart = new DaypartDto();

                proposalDto.Details.Add(proposalDetailDto);

                _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Invalid spot length for detail with flight", MatchType = MessageMatch.Contains)]
        public void ThrowExceptionWhenProposalDetailSpotLengthIsInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _setupProposalDto();
                var proposalDetailDto = _setupProposalDetailDto();

                proposalDetailDto.SpotLengthId = 99;

                proposalDto.Details.Add(proposalDetailDto);

                _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Invalid daypart code for proposal detail with flight", MatchType = MessageMatch.Contains)]
        public void ThrowExceptionWhenProposalDetailDaypartCodeIsInvalid()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _setupProposalDto();
                var proposalDetailDto = _setupProposalDetailDto();

                proposalDetailDto.DaypartCode = string.Empty;

                proposalDto.Details.Add(proposalDetailDto);

                _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanAddProposalWithDetail()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _setupProposalDto();

                var proposalDetailDto = _setupProposalDetailDto();

                proposalDto.Details.Add(proposalDetailDto);

                var result = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDto), "ProgramDisplayFilter");
                jsonResolver.Ignore(typeof(ProposalDto), "ProgramFilter");
                jsonResolver.Ignore(typeof(ProposalDto), "CacheGuid");
                jsonResolver.Ignore(typeof(ProposalDto), "VersionId");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                
                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));

            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadProposalWithDetails()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(248);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDto), "FlightWeeks");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");


                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposal, jsonSettings));
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadProposalByIdWithNoDetail()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(249);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDto), "VersionId");
                jsonResolver.Ignore(typeof(ProposalDto), "ProgramDisplayFilter");
                jsonResolver.Ignore(typeof(ProposalDto), "ProgramFilter");
                jsonResolver.Ignore(typeof(ProposalDto), "CacheGuid");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "FlightEndDate");
                jsonResolver.Ignore(typeof(ProposalDto), "FlightStartDate");


                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposal, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanEditProposalDetailFlight()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposalDto = _ProposalService.GetProposalById(251);

                // editing the first proposal detail
                var editDetail = proposalDto.Details.First();
                editDetail.FlightEdited = true;

                // increasing the flight 
                editDetail.FlightStartDate = new DateTime(2017, 05, 15);
                editDetail.FlightEndDate = new DateTime(2017, 06, 18);

                var FlightWeeks = new List<ProposalFlightWeek>
                {
                    new ProposalFlightWeek
                    {
                        StartDate = new DateTime(2017, 06, 05),
                        EndDate = new DateTime(2017, 06, 11),
                        IsHiatus = false
                    },
                    new ProposalFlightWeek
                    {
                        StartDate = new DateTime(2017, 06, 12),
                        EndDate = new DateTime(2017, 06, 18),
                        IsHiatus = false
                    }
                };

                editDetail.FlightWeeks.AddRange(FlightWeeks);

                var result = _ProposalService.UpdateProposal(proposalDto.Details);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Daypart");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Markets");


                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanEditProposalDetailFlightAndSetWeekToHiatus()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposalDto = _ProposalService.GetProposalById(251);
                // editing the first proposal detail
                var editDetail = proposalDto.Details.First();
                // flight will be the same, just changing the first week to hiatus
                editDetail.FlightEdited = true;
                var week = editDetail.FlightWeeks.First();
                week.IsHiatus = true;

                var result = _ProposalService.UpdateProposal(proposalDto.Details);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Daypart");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Markets");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadMarketGroups()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(250);

                Assert.AreEqual(ProposalEnums.ProposalMarketGroups.Top100, proposal.MarketGroupId);
                Assert.AreEqual(ProposalEnums.ProposalMarketGroups.Top50, proposal.BlackoutMarketGroupId);
                Assert.AreEqual(2, proposal.Markets.Count);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanSaveProposalWithUseShareBookOnly()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                const short shareBookMonthId = 413;

                var proposal = _setupProposalDto();
                var detail = _setupProposalDetailDto();

                proposal.Details.Add(detail);

                detail.HutPostingBookId = ProposalConstants.UseShareBookOnlyId;
                detail.SharePostingBookId = shareBookMonthId;

                var resultProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);
                var resultProposalDetail = resultProposal.Details.First();

                Assert.AreEqual(shareBookMonthId, resultProposalDetail.SinglePostingBookId);
                Assert.AreEqual(shareBookMonthId, resultProposalDetail.SharePostingBookId);
                Assert.AreEqual(-1, resultProposalDetail.HutPostingBookId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadProposalWithUseShareBookOnly()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(251);
                var firstDetail = proposal.Details.First();

                Assert.AreEqual(413, firstDetail.SharePostingBookId);
                Assert.AreEqual(-1, firstDetail.HutPostingBookId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanSaveProposalWithProposedStatus()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _setupProposalDto();

                proposal.Status = ProposalEnums.ProposalStatusType.Proposed;

                var savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                Assert.AreEqual(ProposalEnums.ProposalStatusType.Proposed, savedProposal.Status);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanSaveProposalWithAgencyOnHoldStatus()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _setupProposalDto();

                proposal.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;

                var savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                Assert.AreEqual(ProposalEnums.ProposalStatusType.AgencyOnHold, savedProposal.Status);
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanSaveProposalWithContractedStatus()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _setupProposalDto();

                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;

                var savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                Assert.AreEqual(ProposalEnums.ProposalStatusType.Contracted, savedProposal.Status);
            }
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanLoadLastEditedProposalAsPrimary()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposalDto = _ProposalService.GetProposalByIdWithVersion(249, 2);

                // change something
                proposalDto.Equivalized = false;

                var updatedProposalDto = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser",
                    _CurrentDateTime);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                // should return id 249, version 3 and primary 43 instead of 44
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(updatedProposalDto, jsonSettings));
            }
        }

        [Test]
        public void ProposalWithInventorySelectedIsNotSavedWithoutForceSave()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(248);
                var originalAdvertiserId = proposalDto.AdvertiserId;
                proposalDto.AdvertiserId = 37373;
                proposalDto.PostType = SchedulePostType.NTI;
                var proposal = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);
                Assert.IsTrue(proposal.ValidationWarning.HasWarning);
                var originalProposal = _ProposalService.GetProposalById(248);
                Assert.AreEqual(originalAdvertiserId, originalProposal.AdvertiserId);
            }
        }

        [Test]
        public void CanSaveUpdatedProposalWithInventoryAgainstIt()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(250);
                proposalDto.PostType = SchedulePostType.NTI;

                proposalDto.ForceSave = true;

                var res = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);
                Assert.IsTrue(res.Id > 0);
            }
        }

        [Test]
        public void UpdateProposalStatus_FromProposedToContracted_Throws_Exception()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(250);

                Assert.That(proposalDto.Status, Is.EqualTo(ProposalEnums.ProposalStatusType.Proposed));

                proposalDto.Status = ProposalEnums.ProposalStatusType.Contracted;

                Assert.That(() => _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime), Throws.Exception.With.Message.EqualTo("Cannot change proposal status from Proposed to Contracted"));
            }
        }

        [Test]
        public void UpdateProposalStatus_FromAgencyOnHoldToProposed_Has_Validation_Warning()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(250);
                proposalDto.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
                var dto = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);
                dto.Status = ProposalEnums.ProposalStatusType.Proposed;
                dto.ForceSave = false;

                var result = _ProposalService.SaveProposal(dto, "IntegrationTestUser", _CurrentDateTime);

                Assert.IsTrue(result.ValidationWarning.HasWarning);
                Assert.AreEqual(ProposalConstants.ChangeProposalStatusReleaseInventoryMessage, result.ValidationWarning.Message);
            }
        }

        [Test]
        public void UpdateProposalStatus_FromAgencyOnHoldToProposed_Clears_InventoryAllocations_When_ForceSave()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(248);
                proposalDto.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;

                var dto = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);
                dto.Status = ProposalEnums.ProposalStatusType.Proposed;
                dto.ForceSave = true;

                _ProposalService.SaveProposal(dto, "IntegrationTestUser", _CurrentDateTime);

                var repo = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory.GetDataRepository<IProposalInventoryRepository>();
                Assert.IsEmpty(repo.GetProprietaryInventoryAllocations(248));
                Assert.IsEmpty(repo.GetOpenMarketInventoryAllocations(248));
            }
        }

        [Test]
        public void SaveProposal_CannotSetStatusTo_AgencyOnHold_IfAnotherVersionisAgencyOnHold()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(249);
                proposalDto.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;

                var dto = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                proposalDto = _ProposalService.GetProposalById(249);
                proposalDto.Status = ProposalEnums.ProposalStatusType.Contracted;

                proposalDto.Version = null;
                dto = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                dto.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
                dto.ForceSave = true;
                proposalDto.Version = null;

                Assert.That(() => _ProposalService.SaveProposal(dto, "IntegrationTestUser", _CurrentDateTime), Throws.Exception.With.Message.EqualTo("Cannot set status to Agency On Hold, another proposal version is set to \"Agency On Hold\" or \"Contracted\"."));
            }
        }

        [Test]
        public void SaveProposal_CannotSetStatusTo_AgencyOnHold_IfAnotherVersionisAgencyOnHold_OtherThanPrimaryPrimary()
        {   // inspired by BCOP-1618
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(249);
                proposalDto.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
                var dto = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                dto.Status = ProposalEnums.ProposalStatusType.Proposed;
                dto.Version = null;
                var newProposedDto = _ProposalService.SaveProposal(dto, "IntegrationTestUser", _CurrentDateTime);

                var versionNumber = _ProposalService.GetProposalVersionsByProposalId(newProposedDto.Id.Value).Max(v => v.Version);

                newProposedDto = _ProposalService.GetProposalByIdWithVersion(249,versionNumber);
                newProposedDto.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
                Assert.That(() => _ProposalService.SaveProposal(newProposedDto, "IntegrationTestUser", _CurrentDateTime), Throws.Exception.With.Message.EqualTo("Cannot set status to Agency On Hold, another proposal version is set to \"Agency On Hold\" or \"Contracted\"."));
            }
        }

        [Test]
        public void SaveProposal_CannotSetStatusTo_AgencyOnHold_IfAnotherVersionisPrimary()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(249);
                proposalDto.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;

                var dto = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                dto.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
                dto.ForceSave = true;
                dto.Version = null;

                Assert.That(() => _ProposalService.SaveProposal(dto, "IntegrationTestUser", _CurrentDateTime), Throws.Exception.With.Message.EqualTo("Cannot set status to Agency On Hold, another proposal version is set to \"Agency On Hold\" or \"Contracted\"."));
            }
        }

        [Test]
        public void SaveProposal_CannotSetStatusTo_AgencyOnHold_IfAnotherVersionisContracted()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(249);
                proposalDto.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;

                var dto = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);
                dto.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
                dto.ForceSave = true;
                dto.Version = null;

                Assert.That(() => _ProposalService.SaveProposal(dto, "IntegrationTestUser", _CurrentDateTime), Throws.Exception.With.Message.EqualTo("Cannot set status to Agency On Hold, another proposal version is set to \"Agency On Hold\" or \"Contracted\"."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanDeleteInventoryAllocationsForDetailQuarterWeekWhenUpdatingProposalDetail()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(248);
                proposalDto.ForceSave = true;

                // removing a detail quarter that has inventory against it.
                proposalDto.Details.RemoveAt(0);

                // it should save just fine
                var res = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");

                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };
                //It should remove the week
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(res, jsonSettings));

            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void HasInventorySelectedWhenChangingWeekToHiatus()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(248);
                var firstWeek = proposal.Details.First().Quarters.First().Weeks.First();

                firstWeek.IsHiatus = true;

                var resultProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(resultProposal));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanDeleteInventoryAllocationsForDetailQuarterWeekWhenUpdatingWeekToHiatus()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(248);
                var firstWeek = proposal.Details.First().Quarters.First().Weeks.First();

                proposal.ForceSave = true;
                firstWeek.IsHiatus = true;

                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                var proposalInventory = _ProposalProprietaryInventoryService.GetInventory(7);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalInventory));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanDeleteInventoryAllocationWhenProposalDetailSpotLengthIsChanged()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(248);
                proposalDto.ForceSave = true;
                proposalDto.Details.First().SpotLengthId = 4;

                _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                var inventory = _ProposalProprietaryInventoryService.GetInventory(proposalDto.Details.First().Id.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailBudgetPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailCpmPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailImpressionsPercent");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "ImpressionsPercent");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "BudgetPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalBudget");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalCpm");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalImpressions");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "BudgetTotal");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "ImpressionsTotal");


                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventory, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanDeleteInventoryAllocationWhenProposalDetailDaypartIsChanged()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(248);
                proposalDto.ForceSave = true;
                proposalDto.Details.First().Daypart = new DaypartDto() { thu = true };

                _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                var inventory = _ProposalProprietaryInventoryService.GetInventory(proposalDto.Details.First().Id.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailBudgetPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailCpmPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailImpressionsPercent");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "ImpressionsPercent");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "BudgetPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalBudget");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalCpm");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalImpressions");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "BudgetTotal");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "ImpressionsTotal");



                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventory, jsonSettings));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanDeleteInventoryAllocationWhenProposalMarketsInChanged()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(248);
                proposalDto.ForceSave = true;
                proposalDto.BlackoutMarketGroupId = ProposalEnums.ProposalMarketGroups.All;

                _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                var inventory = _ProposalProprietaryInventoryService.GetInventory(proposalDto.Details.First().Id.Value);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailBudgetPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailCpmPercent");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailImpressionsPercent");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "ImpressionsPercent");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "BudgetPercent");

                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalBudget");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalCpm");
                jsonResolver.Ignore(typeof(ProposalDetailInventoryBase), "DetailTotalImpressions");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "BudgetTotal");
                jsonResolver.Ignore(typeof(BaseProposalInventoryWeekDto), "ImpressionsTotal");


                var jsonSettings = new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(inventory, jsonSettings));
            }
        }

        [Test]
        public void CanReturnHasInventorySelectedWhenProposalDetailDaypartIsChanged()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalByIdWithVersion(248, 1);
                var proposalDetail = proposal.Details.First();
                proposalDetail.DaypartId = 0;
                proposalDetail.Daypart = new DaypartDto() { mon = true, fri = true, sun = false, endTime = 36000, startTime = 32400 };

                proposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                Assert.IsTrue(proposal.ValidationWarning.HasWarning);
                Assert.AreEqual(ProposalConstants.HasInventorySelectedMessage, proposal.ValidationWarning.Message);
            }
        }

        [Test]
        public void CanReturnHasInventorySelectedWhenProposalDetailSpotLengthIsChanged()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalByIdWithVersion(248, 1);
                var proposalDetail = proposal.Details.First();
                proposalDetail.SpotLengthId = 4;

                proposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                Assert.IsTrue(proposal.ValidationWarning.HasWarning);
                Assert.AreEqual(ProposalConstants.HasInventorySelectedMessage, proposal.ValidationWarning.Message);
            }
        }

        [Test]
        public void CanReturnHasInventorySelectedWhenProposalDetailQuarterCPMIsChanged()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalByIdWithVersion(248, 1);
                var proposalDetailQuarter = proposal.Details.First().Quarters.First();
                proposalDetailQuarter.Cpm = 999;

                proposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                Assert.IsTrue(proposal.ValidationWarning.HasWarning);
                Assert.AreEqual(ProposalConstants.HasInventorySelectedMessage, proposal.ValidationWarning.Message);
            }
        }

        [Test]
        public void CanReturnHasInventorySelectedWhenProposalDetailQuarterImpressionGoalIsChanged()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalByIdWithVersion(248, 1);
                var proposalDetailQuarter = proposal.Details.First().Quarters.First();
                proposalDetailQuarter.ImpressionGoal = 25999;

                proposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                Assert.IsTrue(proposal.ValidationWarning.HasWarning);
                Assert.AreEqual(ProposalConstants.HasInventorySelectedMessage, proposal.ValidationWarning.Message);
            }
        }
        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Only one proposal version can be set to contracted per proposal", MatchType = MessageMatch.Contains)]
        public void CanOnlySaveOneContractedProposalVersion()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalByIdWithVersion(249, 2);
                proposal.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                proposal = _ProposalService.GetProposalByIdWithVersion(249, 2);
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);
                // new version contracted
                var anotherProposal = _ProposalService.GetProposalByIdWithVersion(249, 3);
                anotherProposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                _ProposalService.SaveProposal(anotherProposal, "IntegrationTestUser", _CurrentDateTime);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot make new versions of the proposal", MatchType = MessageMatch.Contains)]
        public void CannotSaveProposalsWhenOneVersionIsContracted()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _setupProposalDto();
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                var savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);
                savedProposal.Version = null;
                _ProposalService.SaveProposal(savedProposal, "IntegrationTestUser", _CurrentDateTime);
            }
        }


        [Test]
        public void CanUnorderProposal()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(249);
                proposal.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
                var savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                proposal = _ProposalService.GetProposalById(249);
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                var proposalId = savedProposal.Id.Value;

                _ProposalService.UnorderProposal(proposalId, "IntegrationTestUser");

                var resultProposal = _ProposalService.GetProposalById(proposalId);
                var proposalVersions = _ProposalService.GetProposalVersionsByProposalId(proposalId);

                Assert.AreEqual(ProposalEnums.ProposalStatusType.AgencyOnHold, resultProposal.Status);
                Assert.AreEqual(4, proposalVersions.Count);
                Assert.AreEqual(1, proposalVersions.Count(pv => pv.Status == ProposalEnums.ProposalStatusType.AgencyOnHold));
                Assert.AreEqual(1, proposalVersions.Count(pv => pv.Status == ProposalEnums.ProposalStatusType.PreviouslyContracted));
            }
        }

        [Ignore]
        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanUnorderProposalAndAllocationsAreNotChanged()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(248);
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                var savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);
                var proposalId = savedProposal.Id.Value;
                var proposalDetailId = savedProposal.Details.First().Id.Value;

                _ProposalService.UnorderProposal(proposalId, "IntegrationTestUser");

                var proposalInventory = _ProposalProprietaryInventoryService.GetInventory(proposalDetailId);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(proposalInventory));
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Only proposals in Contracted status can be unordered.", MatchType = MessageMatch.Contains)]
        public void CannotUnorderProposalInProposedStatus()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(248);
                proposal.Status = ProposalEnums.ProposalStatusType.Proposed;
                var savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);
                var proposalId = savedProposal.Id.Value;

                _ProposalService.UnorderProposal(proposalId, "IntegrationTestUser");
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Only proposals in Contracted status can be unordered.", MatchType = MessageMatch.Contains)]
        public void CannotUnorderProposalInAgencyOnHoldStatus()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(248);
                proposal.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
                var savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);
                var proposalId = savedProposal.Id.Value;

                _ProposalService.UnorderProposal(proposalId, "IntegrationTestUser");
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot change the status of a Previously Contracted proposal.", MatchType = MessageMatch.Contains)]
        public void CannotChangeStatusOfProposalWithPreviouslyContractedStatus()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(248);
                proposal.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
                var savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                proposal = _ProposalService.GetProposalById(248);
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                var proposalId = savedProposal.Id.Value;
                _ProposalService.UnorderProposal(proposalId, "IntegrationTestUser");
                var proposalVersions = _ProposalService.GetProposalVersionsByProposalId(proposalId);
                var lastProposalVersion = proposalVersions.Last();
                var previouslyContractedProposal = _ProposalService.GetProposalByIdWithVersion(proposalId,
                    lastProposalVersion.Version);

                previouslyContractedProposal.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;

                _ProposalService.SaveProposal(previouslyContractedProposal, "IntegrationTestUser", _CurrentDateTime);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot edit a proposal in Previously Contracted status.", MatchType = MessageMatch.Contains)]
        public void CannotSetProposalStatusToPreviouslyContracted()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(248);
                proposal.Status = ProposalEnums.ProposalStatusType.PreviouslyContracted;
                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot edit a proposal in Previously Contracted status.", MatchType = MessageMatch.Contains)]
        public void CannotEditProposalWithPreviouslyContractedStatus()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(248);
                proposal.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
                var savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                proposal = _ProposalService.GetProposalById(248);
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                var proposalId = savedProposal.Id.Value;
                _ProposalService.UnorderProposal(proposalId, "IntegrationTestUser");
                var proposalVersions = _ProposalService.GetProposalVersionsByProposalId(proposalId);
                var lastProposalVersion = proposalVersions.Last();
                var previouslyContractedProposal = _ProposalService.GetProposalByIdWithVersion(proposalId,
                    lastProposalVersion.Version);

                previouslyContractedProposal.PostType = SchedulePostType.NSI;
                previouslyContractedProposal.ProposalName = "Previously Contracted Test";

                _ProposalService.SaveProposal(previouslyContractedProposal, "IntegrationTestUser", _CurrentDateTime);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot save proposal without specifying a valid username", MatchType = MessageMatch.Contains)]
        public void CannotSaveProposalWithInvalidUsername()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _setupProposalDto();
                _ProposalService.SaveProposal(proposal, "", _CurrentDateTime);
            }
        }
    }
}
