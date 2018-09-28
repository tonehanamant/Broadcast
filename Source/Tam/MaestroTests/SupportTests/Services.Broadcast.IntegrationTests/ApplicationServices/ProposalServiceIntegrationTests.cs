using ApprovalTests;
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
using System.IO;
using Services.Broadcast.Entities.DTO;

namespace Services.Broadcast.IntegrationTests.ApplicationServices
{
    [TestFixture]
    public class ProposalServiceIntegrationTests
    {
        private readonly IProposalService _ProposalService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalService>();
        private readonly DateTime _CurrentDateTime = new DateTime(2016, 02, 15);
        private readonly IProposalProprietaryInventoryService _ProposalProprietaryInventoryService = IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalProprietaryInventoryService>();

        public static ProposalDto SetupProposalDto()
        {
            return new ProposalDto()
            {
                ForceSave = true,
                AdvertiserId = 37444,
                ProposalName = "Proposal Test",
                GuaranteedDemoId = 31,
                MarketGroupId = ProposalEnums.ProposalMarketGroups.All,
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

        public static ProposalDetailDto SetupProposalDetailDto()
        {
            return new ProposalDetailDto
            {
                FlightStartDate = new DateTime(2016, 05, 30),
                FlightEndDate = new DateTime(2016, 06, 05),
                Daypart = new DaypartDto() { mon = true, tue = true },
                SpotLengthId = 1,
                DaypartCode = "NAV",
                ShareProjectionBookId = 413,
                HutProjectionBookId = 410,
                ProjectionPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus3,
                AdjustmentRate = 1,
                AdjustmentMargin = 3,
                AdjustmentInflation = 2,
                GoalImpression = 10000,
                GoalBudget = 100,
                GenreCriteria = new List<GenreCriteria>()
                {
                    new GenreCriteria
                    {
                        Contain = ContainTypeEnum.Include,Genre = new LookupDto {Id=10}
                    },
                    new GenreCriteria
                    {
                        Contain = ContainTypeEnum.Include,Genre = new LookupDto {Id=12}
                    }
                },
                ShowTypeCriteria = new List<ShowTypeCriteria> {
                    new ShowTypeCriteria{
                        Contain = ContainTypeEnum.Include,
                        ShowType = new LookupDto(){ Id=1}
                    },
                    new ShowTypeCriteria{
                        Contain = ContainTypeEnum.Include,
                        ShowType = new LookupDto(){ Id=3}
                    }
                },
                ProgramCriteria = new List<ProgramCriteria>()
                {
                    new ProgramCriteria()
                    {
                        Contain = ContainTypeEnum.Include,
                        Program = new LookupDto{ Display = "News" }
                    },
                    new ProgramCriteria()
                    {
                        Contain = ContainTypeEnum.Include,
                        Program = new LookupDto{Display = "Tonight" }
                    }
                },
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
                                EndDate = new DateTime(2016, 06, 05),
                                Iscis = new List<ProposalWeekIsciDto>
                                {
                                    new ProposalWeekIsciDto
                                    {
                                        ClientIsci = "AAAAAA",
                                        HouseIsci = "AAAAAA",
                                        Brand = "Testing",
                                        MarriedHouseIsci = true,
                                        Days = "-F"
                                    },
                                    new ProposalWeekIsciDto
                                    {
                                        ClientIsci = "BBBBBBB",
                                        HouseIsci = "BBBBBBB",
                                        Brand = "Testing 2",
                                        MarriedHouseIsci = false,
                                        Days = "M-T-W-TH-"
                                    }
                                }
                            }
                        }
                    }
                },
                Sequence = 0
            };
        }

        private static void _SetupProposalWithAllocations()
        {
            var inventoryService =
                IntegrationTestApplicationServiceFactory.GetApplicationService<IProposalProprietaryInventoryService>();
            var request = new ProprietaryInventoryAllocationRequest { ProposalDetailId = 7, UserName = "test-user" };

            request.SlotAllocations.Add(new ProprietaryInventorySlotAllocations
            {
                InventoryDetailSlotId = 10206,
                Deletes =
                    new List<ProprietaryInventorySlotProposal>
                    {
                        new ProprietaryInventorySlotProposal
                        {
                            QuarterWeekId = 7,
                            Order = 1,
                            SpotLength = 30,
                            Impressions = 30203.123d
                        }
                    },
                Adds =
                    new List<ProprietaryInventorySlotProposal>
                    {
                        new ProprietaryInventorySlotProposal
                        {
                            QuarterWeekId = 7,
                            Order = 1,
                            SpotLength = 30,
                            Impressions = 2000.12d
                        }
                    },
            });

            inventoryService.SaveInventoryAllocations(request);
        }

        private static JsonSerializerSettings _SetupJsonSerializerSettingsIgnoreIds()
        {
            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(LookupDto), "Id");
            jsonResolver.Ignore(typeof(ProposalProgramDto), "Id");
            jsonResolver.Ignore(typeof(ProposalDto), "Id");
            jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
            jsonResolver.Ignore(typeof(ProposalDto), "CacheGuid");
            jsonResolver.Ignore(typeof(ProposalDto), "VersionId");
            jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
            jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
            jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
            jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
            jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
            jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
            jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");
            jsonResolver.Ignore(typeof(GenreCriteria), "Id");
            jsonResolver.Ignore(typeof(ProgramCriteria), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            return jsonSettings;
        }

        [Test]
        public void CanAddProposal()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = SetupProposalDto();
                
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
                var proposalDto = SetupProposalDto();
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
                var proposalDto = SetupProposalDto();
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
                var proposalDto = SetupProposalDto();
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
        [Ignore]
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
                proposalDto.Details[0].GenreCriteria.RemoveAll(x=>x.Contain == ContainTypeEnum.Include); //proposal cannot have include and exclude genres at the same time

                var newProposal = _ProposalService.SaveProposal(proposalDto, "Integration Test", _CurrentDateTime);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDto), "VersionId");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(GenreCriteria), "Id");
                jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");
                jsonResolver.Ignore(typeof(LookupDto), "Id");


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
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void EditProposal()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = _ProposalService.GetProposalById(249);
                proposalDto.ProposalName = "Edited Proposal Test";
                proposalDto.MarketGroupId = ProposalEnums.ProposalMarketGroups.All;
                proposalDto.Equivalized = true;
                proposalDto.PostType = SchedulePostType.NTI;

                var proposalDetailDto = SetupProposalDetailDto();

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
                    ProposalName = "Proposal Test",
                    GuaranteedDemoId = 31,
                    PostType = SchedulePostType.NSI
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
                jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
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
            var proposalId = 250;
            var proposalDto = _ProposalService.GetProposalById(proposalId);

            proposalDto.Details[0].GenreCriteria = new List<GenreCriteria>()
            {
                new GenreCriteria
                    {
                        Contain = ContainTypeEnum.Exclude,Genre = new LookupDto {Id=5}
                    },
                    new GenreCriteria
                    {
                        Contain = ContainTypeEnum.Exclude,Genre = new LookupDto {Id=10}
                    }
            };
            proposalDto.Details[0].ShowTypeCriteria = new List<ShowTypeCriteria>()
            {
                new ShowTypeCriteria
                    {
                        Contain = ContainTypeEnum.Exclude,ShowType = new LookupDto {Id=1}
                    },
                    new ShowTypeCriteria
                    {
                        Contain = ContainTypeEnum.Exclude,ShowType = new LookupDto {Id=3}
                    }
            };

            proposalDto.Details[0].ProgramCriteria = new List<ProgramCriteria>()
            {
                new ProgramCriteria()
                {
                    Contain = ContainTypeEnum.Exclude,
                    Program = new LookupDto{ Display = "Shopping" }
                },
                new ProgramCriteria()
                {
                    Contain = ContainTypeEnum.Exclude,
                    Program = new LookupDto{ Display = "Paid" }
                }
            };

            var changeRequest = new ProposalChangeRequest
            {
                Id = proposalId,
                Details = proposalDto.Details
            };

            var updatedProposalDto = _ProposalService.CalculateProposalChanges(changeRequest);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
            jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
            jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
            jsonResolver.Ignore(typeof(ProposalDto), "Markets");
            jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
            jsonResolver.Ignore(typeof(GenreCriteria), "Id");
            jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");
            jsonResolver.Ignore(typeof(ProgramCriteria), "Id");
            jsonResolver.Ignore(typeof(LookupDto), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(updatedProposalDto, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateProposal_ChangeISCIFridayDay()
        {
            var proposalId = 253;
            var proposalDto = _ProposalService.GetProposalById(proposalId);

            proposalDto.Details[0].Quarters[0].Weeks[0].Iscis[0].Days = "M-W-F";

            var changeRequest = new ProposalChangeRequest
            {
                Id = proposalId,
                Details = proposalDto.Details
            };

            var updatedProposalDto = _ProposalService.CalculateProposalChanges(changeRequest);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
            jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
            jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
            jsonResolver.Ignore(typeof(ProposalDto), "Markets");
            jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
            jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
            jsonResolver.Ignore(typeof(GenreCriteria), "Id");
            jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");
            jsonResolver.Ignore(typeof(ProgramCriteria), "Id");
            jsonResolver.Ignore(typeof(LookupDto), "Id");

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = jsonResolver
            };

            Approvals.Verify(IntegrationTestHelper.ConvertToJson(updatedProposalDto, jsonSettings));
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UpdateProposal_UseDefaultNTIConversionFactor()
        {
            var proposalDto = _ProposalService.GetProposalById(253);

            proposalDto.Details[0].NtiConversionFactor = null;
            var updatedProposalDto = _ProposalService.SaveProposal(proposalDto, "Integration Test User", DateTime.Now);

            var jsonResolver = new IgnorableSerializerContractResolver();
            jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
            jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
            jsonResolver.Ignore(typeof(ProposalDto), "Id");
            jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
            jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
            jsonResolver.Ignore(typeof(GenreCriteria), "Id");
            jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");
            jsonResolver.Ignore(typeof(ProgramCriteria), "Id");
            jsonResolver.Ignore(typeof(LookupDto), "Id");

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
                var proposalDto = SetupProposalDto();
                var proposalDetailDto = SetupProposalDetailDto();

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
                var proposalDto = SetupProposalDto();
                var proposalDetailDto = SetupProposalDetailDto();

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
                var proposalDto = SetupProposalDto();
                var proposalDetailDto = SetupProposalDetailDto();

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
                var proposalDto = SetupProposalDto();
                var proposalDetailDto = SetupProposalDetailDto();

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
                var proposalDto = SetupProposalDto();

                var proposalDetailDto = SetupProposalDetailDto();

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
                jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(GenreCriteria), "Id");
                jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");

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
        public void UpdateSequenceWhenUpdatingProposalDetails()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = SetupProposalDto();

                var proposalDetailDto1 = SetupProposalDetailDto();
                proposalDetailDto1.DaypartCode = "A1";

                proposalDto.Details.Add(proposalDetailDto1);

                var proposalDetailDto2 = SetupProposalDetailDto();
                proposalDetailDto2.DaypartCode = "A2";
                proposalDto.Details.Add(proposalDetailDto2);

                var proposalDetailDto3 = SetupProposalDetailDto();
                proposalDetailDto3.DaypartCode = "A3";
                proposalDto.Details.Add(proposalDetailDto3);

                proposalDto = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                proposalDto.Details.Remove(proposalDto.Details[1]);

                var proposalDetailDto4 = SetupProposalDetailDto();
                proposalDetailDto4.DaypartCode = "A4";
                proposalDto.Details.Add(proposalDetailDto4);

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
                jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(GenreCriteria), "Id");
                jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");

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
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot save proposal detail that contains both genre inclusion and genre exclusion criteria", MatchType = MessageMatch.Contains)]
        public void CannotAddProposalWithDetailWithInvalidGenreCriteria()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = SetupProposalDto();

                var proposalDetailDto = SetupProposalDetailDto();

                proposalDetailDto.GenreCriteria.AddRange(new List<GenreCriteria>()
                {
                    new GenreCriteria
                    {
                        Contain = ContainTypeEnum.Include,Genre = new LookupDto {Id=10}
                    },
                    new GenreCriteria
                    {
                        Contain = ContainTypeEnum.Exclude,Genre = new LookupDto {Id=11}
                    }
                });

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
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(GenreCriteria), "Id");
                jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");

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
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot save proposal detail that contains both show type inclusion and show type exclusion criteria.", MatchType = MessageMatch.Contains)]
        public void CannotAddProposalWithDetailWithInvalidShowTypeCriteria()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = SetupProposalDto();

                var proposalDetailDto = SetupProposalDetailDto();

                proposalDetailDto.ShowTypeCriteria.AddRange(new List<ShowTypeCriteria>()
                {
                    new ShowTypeCriteria
                    {
                        Contain = ContainTypeEnum.Include,ShowType = new LookupDto {Id=1}
                    },
                    new ShowTypeCriteria
                    {
                        Contain = ContainTypeEnum.Exclude,ShowType = new LookupDto {Id=3}
                    }
                });

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
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(GenreCriteria), "Id");
                jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");

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
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot save proposal detail that contains both program name inclusion and program name exclusion criteria", MatchType = MessageMatch.Contains)]
        public void CannotAddProposalWithDetailWithInvalidProgramCriteria()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = SetupProposalDto();

                var proposalDetailDto = SetupProposalDetailDto();

                proposalDetailDto.ProgramCriteria.AddRange(new List<ProgramCriteria>()
                {
                    new ProgramCriteria()
                    {
                        Program = new LookupDto{Display = "Sun" },
                        Contain = ContainTypeEnum.Include
                    },
                    new ProgramCriteria()
                    {
                        Program = new LookupDto{Display = "Moon" },
                        Contain = ContainTypeEnum.Exclude
                    }
                });

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
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(GenreCriteria), "Id");
                jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");

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
                jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProgramCriteria), "Id");
                jsonResolver.Ignore(typeof(GenreCriteria), "Id");

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
                var proposalId = 251;
                var proposalDto = _ProposalService.GetProposalById(proposalId);

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

                var changeRequest = new ProposalChangeRequest
                {
                    Id = proposalId,
                    Details = proposalDto.Details
                };

                var result = _ProposalService.CalculateProposalChanges(changeRequest);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Daypart");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
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
        public void CanEditProposalWeekIscis()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposalId = 253;
                var proposalDto = _ProposalService.GetProposalById(proposalId);

                var firstWeek = proposalDto.Details.First().Quarters.First().Weeks.First();

                firstWeek.Iscis.Add(new ProposalWeekIsciDto
                {
                    Brand = "Testing 45",
                    ClientIsci = "ZZZZZZ",
                    HouseIsci = "ZZZZZZ",
                    MarriedHouseIsci = true,
                    Days = "-W-Th-F-Sa-"
                });

                var changeRequest = new ProposalChangeRequest
                {
                    Id = proposalId,
                    Details = proposalDto.Details
                };

                var result = _ProposalService.CalculateProposalChanges(changeRequest);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Daypart");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Markets");
                jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanEditProposalWeekIscisWhenFlightIsChanged()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposalId = 253;
                var proposalDto = _ProposalService.GetProposalById(proposalId);

                var firstWeek = proposalDto.Details.First().Quarters.First().Weeks.First();

                var firstDetail = proposalDto.Details.First();

                firstDetail.FlightEdited = true;

                firstWeek.Iscis.Add(new ProposalWeekIsciDto
                {
                    Brand = "Testing 45",
                    ClientIsci = "ZZZZZZ",
                    HouseIsci = "ZZZZZZ",
                    MarriedHouseIsci = true,
                    Days = "-W-Th-F-Sa-"
                });

                var changeRequest = new ProposalChangeRequest
                {
                    Id = proposalId,
                    Details = proposalDto.Details
                };

                var updatedProposalDto = _ProposalService.CalculateProposalChanges(changeRequest);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ShowTypeCriteria), "Id");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Daypart");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
                jsonResolver.Ignore(typeof(ProposalQuarterDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "ForceSave");
                jsonResolver.Ignore(typeof(ProposalWeekDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Markets");
                jsonResolver.Ignore(typeof(ProposalWeekIsciDto), "Id");

                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    ContractResolver = jsonResolver
                };

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(updatedProposalDto, jsonSettings));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanEditProposalDetailFlightAndSetWeekToHiatus()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposalId = 251;
                var proposalDto = _ProposalService.GetProposalById(proposalId);
                // editing the first proposal detail
                var editDetail = proposalDto.Details.First();
                // flight will be the same, just changing the first week to hiatus
                editDetail.FlightEdited = true;
                var week = editDetail.FlightWeeks.First();
                week.IsHiatus = true;

                var changeRequest = new ProposalChangeRequest
                {
                    Id = proposalId,
                    Details = proposalDto.Details
                };

                var result = _ProposalService.CalculateProposalChanges(changeRequest);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Daypart");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
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

                Assert.AreEqual(ProposalEnums.ProposalMarketGroups.All, proposal.MarketGroupId);
                Assert.AreEqual(ProposalEnums.ProposalMarketGroups.All, proposal.BlackoutMarketGroupId);
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

                var proposal = SetupProposalDto();
                var detail = SetupProposalDetailDto();

                proposal.Details.Add(detail);

                detail.HutProjectionBookId = null;
                detail.ShareProjectionBookId = shareBookMonthId;

                var resultProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);
                var resultProposalDetail = resultProposal.Details.First();

                Assert.AreEqual(shareBookMonthId, resultProposalDetail.SingleProjectionBookId);
                Assert.AreEqual(shareBookMonthId, resultProposalDetail.ShareProjectionBookId);
                Assert.Null(resultProposalDetail.HutProjectionBookId);
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

                Assert.AreEqual(413, firstDetail.ShareProjectionBookId);
                Assert.Null(firstDetail.HutProjectionBookId);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanSaveProposalWithProposedStatus()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = SetupProposalDto();

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
                var proposal = SetupProposalDto();

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
                var proposal = SetupProposalDto();

                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;

                var savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                Assert.AreEqual(ProposalEnums.ProposalStatusType.Contracted, savedProposal.Status);
            }
        }


        [Test]
        [Ignore]
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

        [Ignore]
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

        [Ignore]
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

        [Ignore]
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

        [Ignore]
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
                throw new Exception("Bad bad bad");
                //Assert.IsEmpty(repo.GetOpenMarketInventoryAllocations(248));
            }
        }

        [Ignore]
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

        [Ignore]
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

                newProposedDto = _ProposalService.GetProposalByIdWithVersion(249, versionNumber);
                newProposedDto.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;
                Assert.That(() => _ProposalService.SaveProposal(newProposedDto, "IntegrationTestUser", _CurrentDateTime), Throws.Exception.With.Message.EqualTo("Cannot set status to Agency On Hold, another proposal version is set to \"Agency On Hold\" or \"Contracted\"."));
            }
        }

        [Ignore]
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

        [Ignore]
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
        [Ignore]
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
        [Ignore]
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
        [Ignore]
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

        [Ignore]
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

        [Ignore]
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

        [Ignore]
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

        [Ignore]
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
        [Ignore]
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

        [Ignore]
        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot make new versions of the proposal", MatchType = MessageMatch.Contains)]
        public void CannotSaveProposalsWhenOneVersionIsContracted()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = SetupProposalDto();
                proposal.Status = ProposalEnums.ProposalStatusType.Contracted;
                var savedProposal = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);
                savedProposal.Version = null;
                _ProposalService.SaveProposal(savedProposal, "IntegrationTestUser", _CurrentDateTime);
            }
        }


        [Ignore]
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

        [Ignore]
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

        [Ignore]
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

        [Ignore]
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

        [Ignore]
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

        [Ignore]
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
        [Ignore]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot save proposal without specifying a valid username", MatchType = MessageMatch.Contains)]
        public void CannotSaveProposalWithInvalidUsername()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = SetupProposalDto();
                _ProposalService.SaveProposal(proposal, "", _CurrentDateTime);
            }
        }

        [Test]
        [Ignore]
        [UseReporter(typeof(DiffReporter))]
        public void RecalculateTotalsAfterDeletingAllocations()
        {
            using (new TransactionScopeWrapper())
            {
                _SetupProposalWithAllocations();

                var proposal = _ProposalService.GetProposalById(248);

                proposal.Details.RemoveAll(x => true);

                _ProposalService.SaveProposal(proposal, "test-user", new DateTime(2017, 09, 08));

                var newProposal = _ProposalService.GetProposalById(248);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(newProposal));
            }
        }

        [Test]
        [Ignore]
        [ExpectedException(typeof(Exception), ExpectedMessage = "The Proposal information you have entered [", MatchType = MessageMatch.Contains)]
        public void CanDeleteProposal()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                _ProposalService.DeleteProposal(248);
                // will throw an exception because it does not exist
                var proposal = _ProposalService.GetProposalById(248);
            }
        }

        [Test]
        public void CannotDeleteProposalWithProposedStatus()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var res = _ProposalService.DeleteProposal(253);

                Assert.IsTrue(res.HasWarning);
                Assert.IsTrue(res.Message.Contains("Can only delete proposals with status 'Proposed' or 'Agency on Hold'."));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void FindMatchingGenres()
        {
            using (new TransactionScopeWrapper())
            {

                var result = _ProposalService.FindGenres("ENT");

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");

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
        public void FindMatchingShowTypes()
        {
            using (new TransactionScopeWrapper())
            {

                var result = _ProposalService.FindShowType("MOV");

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");

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
        public void CanEditProposalDetailCriteria()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposalId = 251;
                var proposalDto = _ProposalService.GetProposalById(proposalId);

                // editing the first proposal detail
                var editDetail = proposalDto.Details.First();
                editDetail.ProgramCriteria = new List<ProgramCriteria>
                {
                    new ProgramCriteria
                    {
                         Contain = ContainTypeEnum.Include,
                         Program = new LookupDto
                         {
                             Id = 1,
                             Display = "Test Program 123"
                         }
                    }
                };

                editDetail.GenreCriteria = new List<GenreCriteria>
                {
                    new GenreCriteria
                    {
                         Contain = ContainTypeEnum.Include,
                         Genre = new LookupDto
                         {
                             Id = 1,
                             Display = "Action"
                         }
                    }
                };

                var changeRequest = new ProposalChangeRequest
                {
                    Id = proposalId,
                    Details = proposalDto.Details

                };

                var result = _ProposalService.CalculateProposalChanges(changeRequest);

                var jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(LookupDto), "Id");
                jsonResolver.Ignore(typeof(ProposalProgramDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "Id");
                jsonResolver.Ignore(typeof(ProposalDto), "PrimaryVersionId");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "Daypart");
                jsonResolver.Ignore(typeof(ProposalDetailDto), "EstimateId");
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
        public void CanGetShareBookFromProposal()
        {
            var proposal = _ProposalService.GetProposalById(248);
            var proposalDetail = proposal.Details.First();

            var ratingBook = ProposalServiceHelper.GetBookId(proposalDetail);

            Assert.AreEqual(413, ratingBook);
        }

        [Test]
        public void CanGetSinglePostingBookFromProposal()
        {
            var proposal = _ProposalService.GetProposalById(253);
            var proposalDetail = proposal.Details.First();

            var ratingBook = ProposalServiceHelper.GetBookId(proposalDetail);

            Assert.AreEqual(410, ratingBook);
        }


        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanUpdatePostingDataForProposal()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(26006);
                var firstDetail = proposal.Details.First();

                firstDetail.PostingBookId = 430;
                firstDetail.PostingPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus7;

                var result = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _SetupJsonSerializerSettingsIgnoreIds()));
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot set posting data before uploading affadavit file", MatchType = MessageMatch.Contains)]
        public void CannotUpdatePostingDataForProposalBeforeAffidavitDataIsLoaded()
        {
            using (new TransactionScopeWrapper())
            {
                var proposal = _ProposalService.GetProposalById(250);
                var firstDetail = proposal.Details.First();

                firstDetail.PostingBookId = 430;
                firstDetail.PostingPlaybackType = ProposalEnums.ProposalPlaybackType.LivePlus7;

                var result = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanRecalculateImpressionsWhenPostingBookIsChanged()
        {
            using (new TransactionScopeWrapper())
            {
                var affidavitRepository = IntegrationTestApplicationServiceFactory.BroadcastDataRepositoryFactory
                .GetDataRepository<IAffidavitRepository>();
                var proposal = _ProposalService.GetProposalById(26006);
                var firstDetail = proposal.Details.First();

                firstDetail.PostingBookId = 430;

                _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                var affidavitFile = affidavitRepository.GetAffidavit(167, true);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(affidavitFile));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanSaveMyEventsReportName()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(251);
                var firstDetail = proposal.Details.First();
                var firstWeek = firstDetail.Quarters.First().Weeks.First();

                firstWeek.MyEventsReportName = "Testing 2";

                var result = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _SetupJsonSerializerSettingsIgnoreIds()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanSaveMyEventsReportDefaultValue()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(251);
                var firstDetail = proposal.Details.First();
                var firstWeek = firstDetail.Quarters.First().Weeks.First();

                firstWeek.MyEventsReportName = null;

                var result = _ProposalService.SaveProposal(proposal, "IntegrationTestUser", _CurrentDateTime);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _SetupJsonSerializerSettingsIgnoreIds()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanUpdateProposalDetailAndKeepMyEventsReportName()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalService.GetProposalById(251);
                var firstDetail = proposal.Details.First();
                var firstWeek = firstDetail.Quarters.First().Weeks.First();
                firstWeek.MyEventsReportName = "Test AAA 2018-07-11";
                var proposalChangeRequest = new ProposalChangeRequest
                {
                    Id = 251,
                    Details = new List<ProposalDetailDto>()
                };
                proposalChangeRequest.Details.Add(firstDetail);

                var result = _ProposalService.CalculateProposalChanges(proposalChangeRequest);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _SetupJsonSerializerSettingsIgnoreIds()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanSaveMarketCoverage()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposalDto = new ProposalDto()
                {
                    AdvertiserId = 37444,
                    ProposalName = "Proposal Test",
                    GuaranteedDemoId = 31,
                    PostType = SchedulePostType.NSI,
                    MarketCoverage = 0.5555
                };

                var result = _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _SetupJsonSerializerSettingsIgnoreIds()));
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Could not find Audience with id", MatchType = MessageMatch.Contains)]
        public void CannotSaveProposalWithoutValidGuaranteedDemo()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposalDto = new ProposalDto()
                {
                    AdvertiserId = 37444,
                    ProposalName = "Proposal Test",
                    GuaranteedDemoId = 0,
                    PostType = SchedulePostType.NSI
                };

                var result = _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanSaveNewProposalWithProprietaryPricingData()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposalDto = new ProposalDto
                {
                    AdvertiserId = 37444,
                    ProposalName = "Proposal Test",
                    GuaranteedDemoId = 31,
                    PostType = SchedulePostType.NSI,
                    Details = new List<ProposalDetailDto>()
                };

                var proposalDetailDto = SetupProposalDetailDto();

                proposalDetailDto.ProprietaryPricing.Add(new ProprietaryPricingDto()
                {
                    InventorySource = InventorySourceEnum.CNN,
                    ImpressionsBalance = 0.3,
                    Cpm = 99.99m
                });

                proposalDetailDto.ProprietaryPricing.Add(new ProprietaryPricingDto()
                {
                    InventorySource = InventorySourceEnum.TTNW,
                    ImpressionsBalance = 0.25,
                    Cpm = 12.42m
                });

                proposalDetailDto.ProprietaryPricing.Add(new ProprietaryPricingDto()
                {
                    InventorySource = InventorySourceEnum.TVB,
                    ImpressionsBalance = 0.25,
                    Cpm = 45
                });

                proposalDetailDto.ProprietaryPricing.Add(new ProprietaryPricingDto()
                {
                    InventorySource = InventorySourceEnum.Sinclair,
                    ImpressionsBalance = 0.2,
                    Cpm = 2000m
                });

                proposalDto.Details.Add(proposalDetailDto);

                var result = _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _SetupJsonSerializerSettingsIgnoreIds()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanEditProposalWithProprietaryPricingData()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposalDto = _ProposalService.GetProposalById(249);

                proposalDto.ProposalName = "Edited Proposal Test";

                var proposalDetailDto = SetupProposalDetailDto();

                proposalDetailDto.ProprietaryPricing.Add(new ProprietaryPricingDto()
                {
                    InventorySource = InventorySourceEnum.CNN,
                    ImpressionsBalance = 0.33,
                    Cpm = 99.99m
                });

                proposalDetailDto.ProprietaryPricing.Add(new ProprietaryPricingDto()
                {
                    InventorySource = InventorySourceEnum.TTNW,
                    ImpressionsBalance = 0.33,
                    Cpm = 12.42m
                });

                proposalDetailDto.ProprietaryPricing.Add(new ProprietaryPricingDto()
                {
                    InventorySource = InventorySourceEnum.TVB,
                    ImpressionsBalance = 0.33,
                    Cpm = 45
                });

                proposalDetailDto.ProprietaryPricing.Add(new ProprietaryPricingDto()
                {
                    InventorySource = InventorySourceEnum.Sinclair,
                    ImpressionsBalance = 0.33,
                    Cpm = 2000m
                });

                proposalDto.Details.Add(proposalDetailDto);

                var result = _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _SetupJsonSerializerSettingsIgnoreIds()));
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot save proposal detail that contains invalid inventory source for proprietary pricing", MatchType = MessageMatch.Contains)]
        public void CannotSaveProposalDetailWithInvalidProprietaryPricingData()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposalDto = new ProposalDto
                {
                    AdvertiserId = 37444,
                    ProposalName = "Proposal Test",
                    GuaranteedDemoId = 31,
                    PostType = SchedulePostType.NSI,
                    Details = new List<ProposalDetailDto>()
                };

                var proposalDetailDto = SetupProposalDetailDto();

                proposalDetailDto.ProprietaryPricing.Add(new ProprietaryPricingDto()
                {
                    InventorySource = InventorySourceEnum.OpenMarket,
                    ImpressionsBalance = 1,
                    Cpm = 99.99m
                });               

                proposalDto.Details.Add(proposalDetailDto);

                var result = _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);
            }
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot save proposal detail that contains duplicated inventory sources in proprietary pricing data", MatchType = MessageMatch.Contains)]
        public void CannotSaveProposalDetailWithDuplicatdProprietaryPricingData()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposalDto = new ProposalDto
                {
                    AdvertiserId = 37444,
                    ProposalName = "Proposal Test",
                    GuaranteedDemoId = 31,
                    PostType = SchedulePostType.NSI,
                    Details = new List<ProposalDetailDto>()
                };

                var proposalDetailDto = SetupProposalDetailDto();

                proposalDetailDto.ProprietaryPricing.Add(new ProprietaryPricingDto()
                {
                    InventorySource = InventorySourceEnum.CNN,
                    ImpressionsBalance = 1,
                    Cpm = 99.99m
                });

                proposalDetailDto.ProprietaryPricing.Add(new ProprietaryPricingDto()
                {
                    InventorySource = InventorySourceEnum.CNN,
                    ImpressionsBalance = 1,
                    Cpm = 99.99m
                });

                proposalDto.Details.Add(proposalDetailDto);

                _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanSaveNewProposalWithOpenMarketPricingData()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposalDto = new ProposalDto
                {
                    AdvertiserId = 37444,
                    ProposalName = "Proposal Test",
                    GuaranteedDemoId = 31,
                    PostType = SchedulePostType.NSI,
                    Details = new List<ProposalDetailDto>()
                };

                var proposalDetailDto = SetupProposalDetailDto();

                proposalDetailDto.OpenMarketPricing.CpmMin = 9.99m;
                proposalDetailDto.OpenMarketPricing.CpmMax = 55.99m;
                proposalDetailDto.OpenMarketPricing.UnitCapPerStation = 100;
                proposalDetailDto.OpenMarketPricing.CpmTarget = OpenMarketCpmTarget.Max;

                proposalDto.Details.Add(proposalDetailDto);

                var result = _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _SetupJsonSerializerSettingsIgnoreIds()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void CanEditProposalWithOpenMarketPricingData()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposalDto = _ProposalService.GetProposalById(251);

                proposalDto.ProposalName = "Edited Proposal Test";

                var proposalDetailDto = proposalDto.Details.First();

                proposalDetailDto.OpenMarketPricing.CpmMin = 123.99m;
                proposalDetailDto.OpenMarketPricing.CpmMax = 200m;
                proposalDetailDto.OpenMarketPricing.UnitCapPerStation = 10;
                proposalDetailDto.OpenMarketPricing.CpmTarget = OpenMarketCpmTarget.Avg;

                var result = _ProposalService.SaveProposal(proposalDto, "Integration User", _CurrentDateTime);

                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result, _SetupJsonSerializerSettingsIgnoreIds()));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UploadProposalBuyScxFileSuccessfully()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var request = new ProposalBuySaveRequestDto
                {
                    EstimateId = 3909,
                    FileName = "Checkers 2Q16 SYN - ProposalBuy.scx",
                    Username = "test-user",
                    ProposalVersionDetailId =  10,
                    FileStream = new FileStream(@".\Files\Checkers 2Q16 SYN - ProposalBuy.scx",
                        FileMode.Open,
                        FileAccess.Read)
                };

                var result = _ProposalService.SaveProposalBuy(request);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        [UseReporter(typeof(DiffReporter))]
        public void UploadProposalBuyScxWithUnknownStationAndSpotLength()
        {
            using (new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var request = new ProposalBuySaveRequestDto
                {
                    EstimateId = 3909,
                    FileName = "Checkers 2Q16 SYN - ProposalBuyWithErrors.scx",
                    Username = "test-user",
                    ProposalVersionDetailId = 10,
                    FileStream = new FileStream(@".\Files\Checkers 2Q16 SYN - ProposalBuyWithErrors.scx",
                        FileMode.Open,
                        FileAccess.Read)
                };

                var result = _ProposalService.SaveProposalBuy(request);
                Approvals.Verify(IntegrationTestHelper.ConvertToJson(result));
            }
        }

        [Test]
        public void ProposalService_GetsProposalById_WithDetail_WithoutEstimateId()
        {
            using (new TransactionScopeWrapper())
            {
                var proposalDto = SetupProposalDto();
                var proposalDetailDto = SetupProposalDetailDto();
                proposalDto.Details.Add(proposalDetailDto);
                var savedProposal = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);

                var result = _ProposalService.GetProposalById(savedProposal.Id.Value);

                var retrievedProposalHasDetailWithoutEstimateId = result.Details.Single().EstimateId == null;
                Assert.True(retrievedProposalHasDetailWithoutEstimateId);
            }
        }

        [Test]
        public void ProposalService_GetsProposalById_WithDetail_WithEstimateId()
        {
            using (new TransactionScopeWrapper())
            {
                var estimateId = 3909;
                var proposalDto = SetupProposalDto();
                var proposalDetailDto = SetupProposalDetailDto();
                proposalDto.Details.Add(proposalDetailDto);
                var savedProposal = _ProposalService.SaveProposal(proposalDto, "IntegrationTestUser", _CurrentDateTime);
                var detailId = savedProposal.Details.Single().Id.Value;
                var proposalBuySaveRequest = _GetProposalBuySaveRequestDtoForSuccessfullResult(detailId, estimateId);
                _ProposalService.SaveProposalBuy(proposalBuySaveRequest);

                var result = _ProposalService.GetProposalById(savedProposal.Id.Value);

                var retrievedProposalHasDetailWithEstimateId = result.Details.Single().EstimateId == estimateId;
                Assert.True(retrievedProposalHasDetailWithEstimateId);
            }
        }

        private ProposalBuySaveRequestDto _GetProposalBuySaveRequestDtoForSuccessfullResult(int detailId, int estimateId)
        {
            return new ProposalBuySaveRequestDto
            {
                EstimateId = estimateId,
                FileName = "Checkers 2Q16 SYN - ProposalBuy.scx",
                Username = "test-user",
                ProposalVersionDetailId = detailId,
                FileStream = new FileStream(@".\Files\Checkers 2Q16 SYN - ProposalBuy.scx",
                        FileMode.Open,
                        FileAccess.Read)
            };
        }
    }
}