﻿using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Common.Systems.LockTokens;
using Newtonsoft.Json;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Cache;
using Services.Broadcast.Converters.Scx;
using Services.Broadcast.Entities;
using Services.Broadcast.Entities.DTO;
using Services.Broadcast.Entities.Enums;
using Services.Broadcast.Entities.Scx;
using Services.Broadcast.Helpers;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Transactions;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
using Tam.Maestro.Services.Cable.Entities;
using Tam.Maestro.Services.Clients;

namespace Services.Broadcast.ApplicationServices
{
    public interface IProposalService : IApplicationService
    {
        List<DisplayProposal> GetAllProposals();
        ProposalDto SaveProposal(ProposalDto proposalDto, string userName, DateTime? currentDateTime);
        ProposalDto GetProposalById(int proposalId);
        ProposalLoadDto GetInitialProposalData(DateTime? currentDateTime);
        List<DisplayProposalVersion> GetProposalVersionsByProposalId(int proposalId);
        ProposalDto GetProposalByIdWithVersion(int proposalId, int proposalVersion);
        ProposalDetailDto GetProposalDetail(ProposalDetailRequestDto proposalDetailRequestDto);
        ProposalDto CalculateProposalChanges(ProposalChangeRequest changeRequest);
        ProposalDto UnorderProposal(int proposalId, string username);        
        ValidationWarningDto DeleteProposal(int proposalId);
        Dictionary<int, ProposalDto> GetProposalsByQuarterWeeks(List<int> quarterWeekIds);
        List<LookupDto> FindGenres(string genreSearchString);
        List<LookupDto> FindPrograms(ProgramSearchRequest request, string requestUrl);
        List<LookupDto> FindProgramsExternalApi(ProgramSearchRequest request);
        /// <summary>
        /// Finds show types based on the input string
        /// </summary>
        /// <param name="showTypeSearchString">Show type to filter by</param>
        /// <returns>List of LookupDto objects</returns>
        List<LookupDto> FindShowType(string showTypeSearchString);

        List<string> SaveProposalBuy(ProposalBuySaveRequestDto proposalBuyRequest);

        //Feature not available
        //Tuple<string, Stream> GenerateScxFileDetail(int proposalDetailId);
        //Tuple<string, Stream> GenerateScxFileArchive(int proposalIds);

        string AlignProposalDaypartsToZeroSeconds();
        BroadcastLockResponse LockProposal(int proposalId);
        BroadcastReleaseLockResponse UnlockProposal(int proposalId);
    }

    public class ProposalService : BroadcastBaseClass, IProposalService
    {
        private readonly IProposalRepository _ProposalRepository;
        private readonly IDataRepositoryFactory _BroadcastDataRepositoryFactory;
        private readonly IBroadcastAudiencesCache _AudiencesCache;
        private readonly ISpotLengthRepository _SpotLengthRepository;
        private readonly IMediaMonthAndWeekAggregateCache _MediaMonthAndWeekAggregateCache;
        private readonly ISMSClient _SmsClient;
        private readonly IProposalCalculationEngine _ProposalCalculationEngine;
        private readonly IDaypartCache _DaypartCache;
        private readonly IProposalInventoryRepository _ProposalInventoryRepository;
        private readonly IStationRepository _StationRepository;
        private readonly IGenreRepository _GenreRepository;
        private readonly IShowTypeRepository _ShowTypeReporitory;
        private readonly IProgramNameRepository _ProgramNameRepository;
        private readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine;
        private readonly IProposalScxConverter _ProposalScxConverter;
        private readonly IProjectionBooksService _ProjectionBooksService;
        private readonly IRatingForecastService _RatingForecastService;
        private readonly IProposalTotalsCalculationEngine _ProposalTotalsCalculationEngine;
        private readonly IProposalProprietaryInventoryService _ProposalProprietaryInventoryService;
        private readonly IMyEventsReportNamingEngine _MyEventsReportNamingEngine;
        private readonly IImpressionsService _AffidavitImpressionsService;
        private readonly IProposalOpenMarketInventoryService _ProposalOpenMarketInventoryService;
        private readonly ILockingEngine _LockingEngine;
        const char ISCI_DAYS_DELIMITER = '-';

        public ProposalService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IBroadcastAudiencesCache audiencesCache,
            ISMSClient smsClient,
            IDaypartCache daypartCache,
            IProposalCalculationEngine proposalCalculationEngine,
            IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalScxConverter proposalScxConverter,
            IProjectionBooksService postingBooksService,
            IRatingForecastService ratingForecastService,
            IProposalTotalsCalculationEngine proposalTotalsCalculationEngine,
            IProposalProprietaryInventoryService proposalProprietaryInventoryService,
            IImpressionsService affidavitImpressionsService,
            IMyEventsReportNamingEngine myEventsReportNamingEngine,
            IProposalOpenMarketInventoryService proposalOpenMarketInventoryService, IFeatureToggleHelper featureToggleHelper, IConfigurationSettingsHelper configurationSettingsHelper, ILockingEngine lockingEngine)
        : base(featureToggleHelper, configurationSettingsHelper)
        {
            _BroadcastDataRepositoryFactory = broadcastDataRepositoryFactory;
            _AudiencesCache = audiencesCache;
            _ProposalRepository = _BroadcastDataRepositoryFactory.GetDataRepository<IProposalRepository>();
            _SpotLengthRepository = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>();
            _MediaMonthAndWeekAggregateCache = mediaMonthAndWeekAggregateCache;
            _SmsClient = smsClient;
            _DaypartCache = daypartCache;
            _ProposalCalculationEngine = proposalCalculationEngine;
            _ProposalInventoryRepository =
                _BroadcastDataRepositoryFactory.GetDataRepository<IProposalInventoryRepository>();
            _StationRepository = broadcastDataRepositoryFactory.GetDataRepository<IStationRepository>();
            _GenreRepository = broadcastDataRepositoryFactory.GetDataRepository<IGenreRepository>();
            _ProgramNameRepository = broadcastDataRepositoryFactory.GetDataRepository<IProgramNameRepository>();
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _ProposalScxConverter = proposalScxConverter;
            _ProjectionBooksService = postingBooksService;
            _RatingForecastService = ratingForecastService;
            _ProposalTotalsCalculationEngine = proposalTotalsCalculationEngine;
            _ProposalProprietaryInventoryService = proposalProprietaryInventoryService;
            _ShowTypeReporitory = broadcastDataRepositoryFactory.GetDataRepository<IShowTypeRepository>();
            _AffidavitImpressionsService = affidavitImpressionsService;
            _MyEventsReportNamingEngine = myEventsReportNamingEngine;
            _ProposalOpenMarketInventoryService = proposalOpenMarketInventoryService;
            _LockingEngine = lockingEngine;
        }

        public List<DisplayProposal> GetAllProposals()
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposals = _ProposalRepository.GetAllProposals();
                var advertisers = _SmsClient.FindAdvertisersByIds(proposals.Select(q => q.Advertiser.Id).Distinct().ToList());

                foreach (var p in proposals)
                {
                    var ad = advertisers.Find(q => q.Id == p.Advertiser.Id);
                    if (ad != null)
                        p.Advertiser.Display = ad.Display;
                }

                return proposals;
            }
        }

        public ProposalDto SaveProposal(ProposalDto saveRequest, string userName, DateTime? currentDateTime)
        {

            _LockingEngine.LockProposal(saveRequest.Id.Value);
            try
            {
                _ValidateProposalDtoBeforeSave(saveRequest, userName);

                if (saveRequest.Id.HasValue && saveRequest.Version.HasValue)
                {
                    var proposal = GetProposalByIdWithVersion(saveRequest.Id.Value, saveRequest.Version.Value);

                    if (proposal.Status == ProposalEnums.ProposalStatusType.Proposed && saveRequest.Status == ProposalEnums.ProposalStatusType.Contracted)
                        throw new Exception("Cannot change proposal status from Proposed to Contracted");

                    _EnsureContractedOnHoldStatus(saveRequest, proposal);

                    _CheckIfPostingDataHasChanged(saveRequest, proposal);

                    if (proposal.Status == ProposalEnums.ProposalStatusType.AgencyOnHold && saveRequest.Status == ProposalEnums.ProposalStatusType.Proposed)
                    {
                        if (saveRequest.ForceSave)
                        {
                            _ProposalInventoryRepository.DeleteInventoryAllocations(saveRequest.Id.Value);
                            _ProposalRepository.ResetAllTotals(saveRequest.Id.Value, saveRequest.Version.Value);
                        }
                        else
                        {
                            saveRequest.ValidationWarning = new ValidationWarningDto
                            {
                                HasWarning = true,
                                Message = ProposalConstants.ChangeProposalStatusReleaseInventoryMessage
                            };

                            return saveRequest;
                        }
                    }
                }
                else
                {
                    _EnsureContractedOnHoldStatus(saveRequest);
                }

                // set flightweeks id and dayparts
                _SetProposalFlightWeeksAndIds(saveRequest);
                _SetProposalDetailDaypartId(saveRequest);
                _SetProposalDetailsRatingBooksId(saveRequest);

                if (!saveRequest.ForceSave && _HasInventorySelected(saveRequest))
                {
                    saveRequest.ValidationWarning = new ValidationWarningDto
                    {
                        HasWarning = true,
                        Message = ProposalConstants.HasInventorySelectedMessage
                    };

                    return saveRequest;
                }

                var proposalId = _SaveProposal(saveRequest, userName);

                return saveRequest.Version.HasValue
                    ? GetProposalByIdWithVersion(proposalId, saveRequest.Version.Value)
                    : GetProposalById(proposalId);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                _LockingEngine.UnlockProposal(saveRequest.Id.Value);
            }
        }

        private void _CheckIfPostingDataHasChanged(ProposalDto saveRequest, ProposalDto proposal)
        {
            foreach (var newDetail in saveRequest.Details)
            {
                if (!newDetail.Id.HasValue)
                    continue;

                var previousDetail = proposal.Details.First(x => x.Id == newDetail.Id);
                var newPostingBook = newDetail.PostingBookId;
                var newPlaybackType = newDetail.PostingPlaybackType;
                var previousPostingBook = previousDetail.PostingBookId;
                var previousPlaybackType = previousDetail.PostingPlaybackType;

                if ((previousPostingBook == null && newPostingBook != null) ||
                    (previousPlaybackType == null && newPlaybackType != null))
                {
                    throw new Exception("Cannot set posting data before uploading affadavit file");
                }

                if (previousPostingBook != newPostingBook || previousPlaybackType != newPlaybackType)
                {
                    newDetail.HasPostingDataChanged = true;
                }
            }
        }

        public ValidationWarningDto DeleteProposal(int proposalId)
        {
            // get all proposal versions 
            var proposalVersions = GetProposalVersionsByProposalId(proposalId);

            // check if there is at least one that is contracted or previously contracted
            if (
                proposalVersions.Any(
                    a =>
                        a.Status == ProposalEnums.ProposalStatusType.Contracted ||
                        a.Status == ProposalEnums.ProposalStatusType.PreviouslyContracted))
                return new ValidationWarningDto()
                {
                    HasWarning = true,
                    Message = "Can only delete proposals with status 'Proposed' or 'Agency on Hold'."
                };

            using (var transaction = new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                // remove allocations by version
                foreach (var proposalVersion in proposalVersions)
                {
                    var proposal = GetProposalByIdWithVersion(proposalId, proposalVersion.Version);

                    _DeleteProposalDetailInventoryAllocations(proposal);

                    _DeleteAllInventoryAllocations(proposal);
                }

                _ProposalRepository.DeleteProposal(proposalId);

                transaction.Complete();
            }

            return new ValidationWarningDto() { HasWarning = false };
        }

        private void _DeleteAllInventoryAllocations(ProposalDto proposalDto)
        {
            var proposalVersionQuarterWeeksIds = _GetProposalDetailQuarterWeekIdFromProposalDto(proposalDto);

            // check if proposal has invetory allocated against those quarter week ids
            var proposalHasInventoryDetailSlots =
                _ProposalInventoryRepository.GetProposalsInventoryDetailSlotIdsByQuarterWeeksIds(
                    proposalVersionQuarterWeeksIds)
                    .Any();

            // deal with inventory allocation
            if (proposalHasInventoryDetailSlots)
                _ProposalInventoryRepository.DeleteInventoryAllocationsForDetailQuarterWeek(
                    proposalVersionQuarterWeeksIds);
        }

        private void _EnsureContractedOnHoldStatus(ProposalDto saveRequest, ProposalDto proposal = null)
        {
            if (saveRequest.Id == null)
                return;

            var proposalVersions = _ProposalRepository.GetProposalVersions(saveRequest.Id.Value);
            var query = proposalVersions.Where(pv => (pv.Status == ProposalEnums.ProposalStatusType.AgencyOnHold
                                                         || pv.Status == ProposalEnums.ProposalStatusType.Contracted));
            if (proposal != null)
            {
                query = query.Where(pv => pv.Id != proposal.VersionId);
            }
            if (saveRequest.Status == ProposalEnums.ProposalStatusType.AgencyOnHold && query.Any())
            {
                throw new Exception("Cannot set status to Agency On Hold, another proposal version is set to \"Agency On Hold\" or \"Contracted\".");
            }
        }

        public ProposalDto UnorderProposal(int proposalId, string username)
        {
            using (var transaction = new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                var proposal = GetProposalById(proposalId);

                if (proposal.Status != ProposalEnums.ProposalStatusType.Contracted)
                    throw new Exception("Only proposals in Contracted status can be unordered.");

                var currentActiveVersion = proposal.Version;

                // Create new version with the status as previously contracted.
                // This way there's no need to move the inventory allocations.
                proposal.Version = null;
                proposal.Status = ProposalEnums.ProposalStatusType.PreviouslyContracted;

                _SaveProposal(proposal, username);

                // The proposal with the inventory allocations has the status reset to agency on hold.
                // It is also set as the active version.
                proposal.Version = currentActiveVersion;
                proposal.Status = ProposalEnums.ProposalStatusType.AgencyOnHold;

                _SaveProposal(proposal, username);

                transaction.Complete();

                return proposal;
            }
        }

        private bool _HasInventorySelected(ProposalDto saveRequest)
        {
            if (!saveRequest.Id.HasValue || !saveRequest.Version.HasValue)
                return false;

            var proposal = GetProposalByIdWithVersion(saveRequest.Id.Value, saveRequest.Version.Value);

            if (proposal == null)
                return false;

            // get list of detail ids to check if any has inventory against it
            var proposalHasInventoryDetailSlots = false;

            if (saveRequest.Details != null && saveRequest.Details.Any())
            {
                var quarters =
                    saveRequest.Details.Where(a => a.Quarters != null && a.Quarters.Any())
                        .SelectMany(a => a.Quarters)
                        .ToList();
                var quarterWeeksIds =
                    quarters.Where(b => b.Weeks != null && b.Weeks.Any())
                        .SelectMany(w => w.Weeks.Select(d => d.Id ?? 0).Where(e => e > 0))
                        .ToList();

                proposalHasInventoryDetailSlots =
                    _ProposalInventoryRepository.GetProposalsInventoryDetailSlotIdsByQuarterWeeksIds(quarterWeeksIds)
                        .Any();
            }

            if (!proposalHasInventoryDetailSlots)
                return false;

            // deal with markets
            var proposalMarketsIds = proposal.Markets != null
                ? proposal.Markets.Select(x => x.Id).OrderBy(c => c).ToList()
                : new List<short>();

            var requestMarketIds = saveRequest.Markets != null
                ? saveRequest.Markets.Select(x => x.Id).OrderBy(c => c).ToList()
                : new List<short>();

            if ((proposal.Equivalized != saveRequest.Equivalized) ||
                (proposal.PostType != saveRequest.PostType) ||
                (proposal.MarketGroupId != saveRequest.MarketGroupId) ||
                (proposal.GuaranteedDemoId != saveRequest.GuaranteedDemoId) ||
                (!Enumerable.SequenceEqual(proposalMarketsIds, requestMarketIds)))
            {
                return true;
            }

            // deal with changes at detail level
            if (!Enumerable.SequenceEqual(proposal.Details.SelectMany(a => a.Quarters.SelectMany(q => q.Weeks.OrderBy(w => w.Id))).Select(b => b.IsHiatus),
                    saveRequest.Details.SelectMany(a => a.Quarters.SelectMany(q => q.Weeks.OrderBy(w => w.Id)).Select(z => z.IsHiatus)))) return true;
            // check for changes in the flight weeks
            if (!Enumerable.SequenceEqual(proposal.Details.SelectMany(a => a.Quarters.SelectMany(q => q.Weeks.Select(w => w.Id))).OrderBy(b => b),
                    saveRequest.Details.SelectMany(a => a.Quarters.SelectMany(q => q.Weeks.Select(w => w.Id))).OrderBy(z => z))) return true;
            // check for changes in the spot length
            if (!Enumerable.SequenceEqual(proposal.Details.Select(a => a.SpotLengthId).OrderBy(b => b),
                    saveRequest.Details.Select(c => c.SpotLengthId).OrderBy(z => z))) return true;
            // check for changes in the daypart ids
            if (!Enumerable.SequenceEqual(proposal.Details.Select(a => a.DaypartId).OrderBy(b => b),
                    saveRequest.Details.Select(a => a.DaypartId).OrderBy(z => z))) return true;
            // check if cpms have changed
            if (!Enumerable.SequenceEqual(proposal.Details.SelectMany(a => a.Quarters.Select(q => q.Cpm)).OrderBy(b => b),
                    saveRequest.Details.SelectMany(a => a.Quarters.Select(q => q.Cpm)).OrderBy(z => z))) return true;
            // check if impressions have changed.
            if (!Enumerable.SequenceEqual(proposal.Details.SelectMany(a => a.Quarters.Select(q => q.ImpressionGoal)).OrderBy(b => b),
                    saveRequest.Details.SelectMany(a => a.Quarters.Select(q => q.ImpressionGoal)).OrderBy(z => z))) return true;

            return false;
        }

        private void _SetProposalDetailsRatingBooksId(ProposalDto saveRequest)
        {
            foreach (var proposalDetailDto in saveRequest.Details)
            {
                if (proposalDetailDto.HutProjectionBookId == null &&
                    proposalDetailDto.ShareProjectionBookId != null)
                {
                    proposalDetailDto.SingleProjectionBookId = proposalDetailDto.ShareProjectionBookId;
                    proposalDetailDto.HutProjectionBookId = null;
                    proposalDetailDto.ShareProjectionBookId = null;
                }
                else
                {
                    proposalDetailDto.SingleProjectionBookId = null;
                }
            }
        }

        private int _SaveProposal(ProposalDto proposalDto, string userName)
        {
            using (var transaction = new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                _SetProposalDefaultValues(proposalDto);
                _SetProposalDetailsDefaultValues(proposalDto);
                _SetISCIWeekDays(proposalDto);
                _SetProposalDetailSequenceNumbers(proposalDto);

                // check if an existing proposal is being saved
                var isValidProposalIdAndVersion = proposalDto.Id.HasValue && proposalDto.Version.HasValue;

                // update existing proposal
                if (isValidProposalIdAndVersion)
                {
                    // handle proposal detail inventory allocation
                    _DeleteProposalDetailInventoryAllocations(proposalDto);

                    _DeleteAnyOpenMarketAllocations(proposalDto);

                    // handle proposal detail updates
                    _ProposalRepository.UpdateProposal(proposalDto, userName);

                    _RecalculateAffidavitImpressions(proposalDto);
                }
                else
                {
                    _ProposalRepository.CreateProposal(proposalDto, userName);
                }

                transaction.Complete();

                return proposalDto.Id.Value;
            }
        }

        private void _RecalculateAffidavitImpressions(ProposalDto proposalDto)
        {
            foreach (var detail in proposalDto.Details)
            {
                if (detail.Id.HasValue && detail.HasPostingDataChanged)
                {
                    _AffidavitImpressionsService.RecalculateImpressionsForProposalDetail(detail.Id.Value);
                }
            }
        }

        private static void _SetProposalDetailSequenceNumbers(ProposalDto proposalDto)
        {
            var sequence = 1;
            foreach (var detail in proposalDto.Details)
            {
                detail.Sequence = sequence;
                sequence++;
            }
        }

        private void _SetISCIWeekDays(ProposalDto proposalDto)
        {
            proposalDto.Details.ForEach(quarter => quarter.Quarters.ForEach(week => week.Weeks.ForEach(isci => isci.Iscis.ForEach(isciDay =>
            {
                List<string> splitDays = isciDay.Days?.Split(new char[] { ISCI_DAYS_DELIMITER }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (splitDays != null && splitDays.Any())
                {
                    isciDay.Thursday = splitDays.Any(l => l.Equals("TH", StringComparison.CurrentCultureIgnoreCase));
                    splitDays.RemoveAll(l => l.Equals("TH", StringComparison.CurrentCultureIgnoreCase));
                    isciDay.Monday = splitDays.Any(l => l.Equals("M", StringComparison.CurrentCultureIgnoreCase));
                    isciDay.Tuesday = splitDays.Any(l => l.Equals("T", StringComparison.CurrentCultureIgnoreCase));
                    isciDay.Wednesday = splitDays.Any(l => l.Equals("W", StringComparison.CurrentCultureIgnoreCase));
                    isciDay.Friday = splitDays.Any(l => l.Equals("F", StringComparison.CurrentCultureIgnoreCase));
                    isciDay.Saturday = splitDays.Any(l => l.Equals("Sa", StringComparison.CurrentCultureIgnoreCase));
                    isciDay.Sunday = splitDays.Any(l => l.Equals("Su", StringComparison.CurrentCultureIgnoreCase));
                }
            }))));
        }

        private void _SetProposalDefaultValues(ProposalDto proposalDto)
        {
            // set target and default margin that are nullable when first creating a proposal
            proposalDto.TargetCPM = proposalDto.TargetCPM ?? 0;
            proposalDto.Margin = proposalDto.Margin ?? ProposalConstants.ProposalDefaultMargin;
            if (proposalDto.MarketCoverage.HasValue)
                proposalDto.MarketCoverage = proposalDto.MarketCoverage.Value;
        }

        private void _SetProposalDetailsDefaultValues(ProposalDto proposalDto)
        {
            var advertiser = _SmsClient.FindAdvertiserById(proposalDto.AdvertiserId);

            var spotLengths = _SpotLengthRepository.GetSpotLengthIdsByDuration();
            proposalDto.Details.ForEach(detail =>
            {
                if (proposalDto.PostType.Equals(PostingTypeEnum.NTI))
                {
                    var defaultNtiConversionFactor = _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.DefaultNtiConversionFactor, 0.2);
                    //set default value for NTI Conversion factor
                    detail.NtiConversionFactor = detail.NtiConversionFactor == null
                        ? defaultNtiConversionFactor
                        : detail.NtiConversionFactor.Value;

                }
                //set default value for My Events Report Name
                detail.Quarters.ForEach(y => y.Weeks.ForEach(week =>
                {
                    if (string.IsNullOrWhiteSpace(week.MyEventsReportName))
                    {
                        week.MyEventsReportName = _MyEventsReportNamingEngine.GetDefaultMyEventsReportName(detail.DaypartCode, spotLengths.First(l => l.Value == detail.SpotLengthId).Key, week.StartDate, advertiser.Display);
                    }
                }));
            });
        }

        private void _DeleteAnyOpenMarketAllocations(ProposalDto proposalDto)
        {
            if (!proposalDto.Id.HasValue || !proposalDto.Version.HasValue)
                return;

            //var existingProposal = GetProposalByIdWithVersion(proposalDto.Id.Value, proposalDto.Version.Value);
            //_DeleteAnyOpenMarketAllocationsByDaypart(proposalDto);
        }

        /// <summary>
        /// deletes existing allocations that fall outside the new proposal's daypart.
        /// </summary>
        private void _DeleteAnyOpenMarketAllocationsByDaypart(ProposalDto proposalToUpdateDto)
        {
            //foreach (var detail in proposalToUpdateDto.Details)
            //{
            //    if (!detail.Id.HasValue)
            //        continue;

            //    int detailId = detail.Id.Value;
            //    var detailDaypart = DaypartCache.Instance.GetDisplayDaypart(detail.DaypartId);

            //    var allocations = _ProposalOpenMarketInventoryService.GetProposalInventoryAllocations(detailId);
            //    var stationPrograms = _BroadcastDataRepositoryFactory.GetDataRepository<IStationProgramRepository>()
            //                                .GetStationProgramsByIds(allocations.Select(a => a.StationProgramId).ToList())
            //                                .ToDictionary(k => k.Id,v => v);

            //    List<int> programsToDelete = new List<int>();
            //    allocations.ForEach(p =>
            //    {
            //        var stationProgram = stationPrograms[p.StationProgramId];

            //        var programDaypart = DaypartCache.Instance.GetDisplayDaypart(stationProgram.Daypart.Id);
            //        if (!programDaypart.Intersects(detailDaypart))
            //        {
            //            programsToDelete.Add(p.StationProgramId);
            //        }
            //    });

            //    if (programsToDelete.Any())
            //        _BroadcastDataRepositoryFactory.GetDataRepository<IProposalOpenMarketInventoryRepository>()
            //            .RemoveAllocations(programsToDelete, detailId);
            //}
        }
        private void _DeleteProposalDetailInventoryAllocations(ProposalDto proposalDto)
        {
            if (!proposalDto.Id.HasValue || !proposalDto.Version.HasValue)
                return;

            var countOfAllocationsBeforeDeleting = _ProposalInventoryRepository.GetCountOfAllocationsForProposal(proposalDto.Id.Value);

            _DeleteProposalDetailInventoryAllocationsForQuarterWeeks(proposalDto);

            _DeleteProposalDetailInventoryAllocationsForSpotLength(proposalDto);

            _DeleteProposalDetailInventoryAllocationsForDaypart(proposalDto);

            _DeleteProposalDetailInventoryAllocationsForMarkets(proposalDto);

            var countOfAllocationsAfterDeleting = _ProposalInventoryRepository.GetCountOfAllocationsForProposal(proposalDto.Id.Value);

            if (countOfAllocationsBeforeDeleting != countOfAllocationsAfterDeleting)
            {
                _RecalculateTotalsAfterDeletingAllocations(proposalDto);
            }
        }

        private void _RecalculateTotalsAfterDeletingAllocations(ProposalDto proposalDto)
        {
            if (!proposalDto.Id.HasValue || !proposalDto.Version.HasValue)
                return;

            if (proposalDto.Details.Count == 0)
                _ProposalRepository.ResetAllTotals(proposalDto.Id.Value, proposalDto.Version.Value);
            else
            {
                foreach (var proposalDetailDto in proposalDto.Details)
                {
                    if (!proposalDetailDto.Id.HasValue)
                        return;
                    _ProposalProprietaryInventoryService.RecalculateInventoryTotals(proposalDetailDto.Id.Value);
                }
            }
        }

        private void _DeleteProposalDetailInventoryAllocationsForSpotLength(ProposalDto proposalDto)
        {
            foreach (var proposalDetailDto in proposalDto.Details)
            {
                var previousSpotLengthId = _GetPreviousSpotLengthIdForProposalDetail(proposalDto, proposalDetailDto);
                var proposalDetailWeeks = proposalDetailDto.Quarters.SelectMany(x => x.Weeks).Select(x => x.Id.HasValue ? x.Id.Value : -1).ToList();

                if (proposalDetailDto.SpotLengthId != previousSpotLengthId)
                    _ProposalInventoryRepository.DeleteInventoryAllocationsForDetailQuarterWeek(proposalDetailWeeks);
            }
        }

        private int _GetPreviousSpotLengthIdForProposalDetail(ProposalDto proposalDto, ProposalDetailDto proposalDetailDto)
        {
            if (proposalDto.Id == null ||
                proposalDetailDto.Id == null)
                return -1;

            var previousProposal = proposalDto.Version.HasValue
                    ? GetProposalByIdWithVersion(proposalDto.Id.Value, proposalDto.Version.Value)
                    : GetProposalById(proposalDto.Id.Value);

            var previousProposalDetail = previousProposal.Details.FirstOrDefault(d => d.Id == proposalDetailDto.Id);

            if (previousProposalDetail == null)
                return -1;

            return previousProposalDetail.SpotLengthId;
        }

        private void _DeleteProposalDetailInventoryAllocationsForDaypart(ProposalDto proposalDto)
        {
            foreach (var proposalDetailDto in proposalDto.Details)
            {
                var proposalDetailWeeks = proposalDetailDto.Quarters.SelectMany(x => x.Weeks).Select(x => x.Id.HasValue ? x.Id.Value : -1).ToList();
                var proposalInventorySlots = _ProposalInventoryRepository.GetProposalInventorySlotsByQuarterWeekIds(proposalDetailWeeks);
                var proposalDetailDaypart = DaypartDto.ConvertDaypartDto(proposalDetailDto.Daypart);
                var slotAllocationsToDelete = new List<int>();

                foreach (var proposalInventorySlot in proposalInventorySlots)
                {
                    var displayDaypart = DaypartCache.Instance.GetDisplayDaypart(proposalInventorySlot.RolleupDaypartId);

                    if (!displayDaypart.Intersects(proposalDetailDaypart))
                        slotAllocationsToDelete.Add(proposalInventorySlot.Id);
                }

                _ProposalInventoryRepository.DeleteInventoryAllocations(slotAllocationsToDelete, proposalDetailWeeks);
            }
        }

        private void _DeleteProposalDetailInventoryAllocationsForMarkets(ProposalDto proposalDto)
        {
            if (!proposalDto.Id.HasValue || !proposalDto.Version.HasValue)
                return;

            foreach (var proposalDetailDto in proposalDto.Details)
            {
                if (!proposalDetailDto.Id.HasValue)
                    continue;

                var markets = _ProposalMarketsCalculationEngine.GetProposalMarketsList(proposalDto.Id.Value, proposalDto.Version.Value);
                var marketIds = markets.Select(m => m.Id).ToList();
                var proposalDetailWeeks = proposalDetailDto.Quarters.SelectMany(x => x.Weeks).Select(x => x.Id.HasValue ? x.Id.Value : -1).ToList();
                var proposalInventorySlots = _ProposalInventoryRepository.GetProposalInventorySlotsByQuarterWeekIds(proposalDetailWeeks);
                var slotAllocationsToDelete = new List<int>();

                foreach (var proposalInventorySlot in proposalInventorySlots)
                {
                    var isIncludedInMarkets = false;

                    foreach (var inventoryComponent in proposalInventorySlot.InventoryDetailSlotComponents)
                    {
                        var stationCode = inventoryComponent.StationCode;
                        var broadcastStation = _StationRepository.GetBroadcastStationByCode(stationCode);

                        if (marketIds.Contains(broadcastStation.MarketCode.Value))
                        {
                            isIncludedInMarkets = true;

                            break;
                        }
                    }

                    if (!isIncludedInMarkets)
                        slotAllocationsToDelete.Add(proposalInventorySlot.Id);
                }

                _ProposalInventoryRepository.DeleteInventoryAllocations(slotAllocationsToDelete, proposalDetailWeeks);
            }
        }

        // deal with inventory detail quarter weeks that have been modified (deleted) by user and have allocations against it
        private void _DeleteProposalDetailInventoryAllocationsForQuarterWeeks(ProposalDto proposalDto)
        {
            var proposalVersionid = _ProposalRepository.GetProposalVersionId(proposalDto.Id.Value, proposalDto.Version.Value);

            // get unchanged proposal version detail week ids
            var proposalDetailQuaterWeekIds = _ProposalRepository.GetProposalDetailQuarterWeekIdsByProposalVersionId(proposalVersionid);

            var updatedProposalQuarterWeeksIds = _GetProposalDetailQuarterWeekIdFromProposalDto(proposalDto);

            // get the details quarter week ids that have been removed, if any
            var deletedQuarterWeeksIds = proposalDetailQuaterWeekIds.Except(updatedProposalQuarterWeeksIds).ToList();
            if (!deletedQuarterWeeksIds.Any()) return;

            // check if proposal has invetory allocated against those quarter week ids
            var proposalHasInventoryDetailSlots =
                _ProposalInventoryRepository.GetProposalsInventoryDetailSlotIdsByQuarterWeeksIds(deletedQuarterWeeksIds)
                    .Any();

            // deal with inventory allocation
            if (proposalHasInventoryDetailSlots)
                _ProposalInventoryRepository.DeleteInventoryAllocationsForDetailQuarterWeek(deletedQuarterWeeksIds);
        }

        private List<int> _GetProposalDetailQuarterWeekIdFromProposalDto(ProposalDto proposalDto)
        {
            var hasDetails = proposalDto.Details != null && proposalDto.Details.Any();
            if (!hasDetails) return new List<int>();

            // retrieve quarterweek ids that have an value against it (user could have added new week ids)
            return proposalDto.Details.Where(z => z.Quarters.Any())
                .SelectMany(
                    a =>
                        a.Quarters.Where(y => y.Weeks.Any())
                            .SelectMany(b => b.Weeks.Where(m => m.Id.HasValue && !m.IsHiatus).Select(w => w.Id.Value)))
                .ToList();
        }


        private void _SetProposalFlightWeeksAndIds(ProposalDto proposalDto)
        {
            // when creating a new proposal flightweeks is null
            if (proposalDto.FlightWeeks == null)
                proposalDto.FlightWeeks = new List<ProposalFlightWeek>();
            else
                proposalDto.FlightWeeks.Clear();

            proposalDto.FlightStartDate = null;
            proposalDto.FlightEndDate = null;

            if (proposalDto.Details != null && proposalDto.Details.Any())
            {
                // build list of flight weeks
                var flights = proposalDto.Details.SelectMany(
                    d => d.Quarters.SelectMany(q => q.Weeks.Select(w => new ProposalFlightWeek()
                    {
                        StartDate = w.StartDate,
                        EndDate = w.EndDate,
                        IsHiatus = w.IsHiatus,
                        MediaWeekId = w.MediaWeekId
                    }))).OrderBy(x => x.StartDate).ToList();

                // just make sure not repeated flight weeks are added
                foreach (var f in flights)
                {
                    var flight = proposalDto.FlightWeeks.Find(a => a.MediaWeekId == f.MediaWeekId);
                    if (flight == null)
                    {
                        proposalDto.FlightWeeks.Add(f);
                    }
                    else
                    {
                        // if the flight in the list is hiatus and the one beeing validated is not hiatus, the no hiatus takes place
                        if (!f.IsHiatus && flight.IsHiatus)
                            flight.IsHiatus = false;
                    }
                }

                // set start/end date
                proposalDto.FlightStartDate = proposalDto.Details.Select(a => a.FlightStartDate).Min();
                proposalDto.FlightEndDate = proposalDto.Details.Select(a => a.FlightEndDate).Max();
            }
        }

        private void _ValidateProposalDtoBeforeSave(ProposalDto proposalDto, string userName)
        {
            if (proposalDto == null)
                throw new Exception("Cannot save proposal with invalid data.");

            if (string.IsNullOrWhiteSpace(proposalDto.ProposalName))
                throw new Exception("Cannot save proposal without specifying name value.");

            if (string.IsNullOrWhiteSpace(userName))
                throw new Exception("Cannot save proposal without specifying a valid username.");

            // Will throw an exception if demo not found.
            _AudiencesCache.FindDto(proposalDto.GuaranteedDemoId);

            _ValidatePreviouslyContractedStatus(proposalDto);

            // Will throw an exception if advertiser not found.
            _SmsClient.FindAdvertiserById(proposalDto.AdvertiserId);

            _ValidateProposalDetailBeforeSave(proposalDto);

            _ValidateProposalStatusBeforeSave(proposalDto);
        }

        private void _ValidatePreviouslyContractedStatus(ProposalDto proposalDto)
        {
            if (proposalDto.Status == ProposalEnums.ProposalStatusType.PreviouslyContracted)
                throw new Exception("Cannot edit a proposal in Previously Contracted status.");

            if (!proposalDto.Id.HasValue || !proposalDto.Version.HasValue)
                return;

            var previousProposalVersion = _ProposalRepository.GetProposalByIdAndVersion(proposalDto.Id.Value,
                proposalDto.Version.Value);

            if (previousProposalVersion.Status == ProposalEnums.ProposalStatusType.PreviouslyContracted)
                throw new Exception("Cannot change the status of a Previously Contracted proposal.");
        }

        private void _ValidateProposalStatusBeforeSave(ProposalDto proposalDto)
        {
            // new proposal
            if (!proposalDto.Id.HasValue) return;
            // get list of all proposal versions
            var proposalVersions = _ProposalRepository.GetProposalVersions(proposalDto.Id.Value);
            // check if exist one that is contracted.
            var contractedProposal = proposalVersions.Find(a => a.Status == ProposalEnums.ProposalStatusType.Contracted);

            // avoid creating new proposals when there is already one contracted proposal
            if (contractedProposal != null && !proposalDto.Version.HasValue)
            {
                throw new Exception("Cannot make new versions of the proposal.");
            }

            // if a contracted proposal already exists and user is trying to save a new version with contracted status, throw error
            if (contractedProposal != null && contractedProposal.VersionNumber != proposalDto.Version && proposalDto.Status == ProposalEnums.ProposalStatusType.Contracted)
            {
                throw new Exception("Only one proposal version can be set to contracted per proposal.");
            }

        }

        private void _ValidateProposalProjectionBooks(ProposalDetailDto proposalDetailDto)
        {
            if (proposalDetailDto.ShareProjectionBookId == null)
                throw new Exception("Cannot save proposal without specifying a Share Book");

            //ProjectionPlaybackType is not nullable
            //if (proposalDetailDto.ProjectionPlaybackType == null)
            //    throw new Exception("Cannot save proposal without specifying a Playback Type");

            if (proposalDetailDto.ShareProjectionBookId == proposalDetailDto.HutProjectionBookId ||
                proposalDetailDto.HutProjectionBookId > proposalDetailDto.ShareProjectionBookId)
                throw new Exception("Hut Media Month must be earlier (less than) the Share Media Month");
        }

        private void _ValidateProposalDetailBeforeSave(ProposalDto proposalDto)
        {
            var validSpothLengths = _SpotLengthRepository.GetSpotLengthIdsByDuration();

            const int secondsPerMinute = 60;
            const int secondsPerDay = 60 * 60 * 24;

            foreach (var detail in proposalDto.Details)
            {
                _ValidateProposalProjectionBooks(detail);

                if (detail.FlightEndDate == default(DateTime) || detail.FlightStartDate == default(DateTime))
                    throw new Exception("Cannot save proposal detail without specifying flight start/end date.");

                var displaydayPart = DaypartDto.ConvertDaypartDto(detail.Daypart);

                if (!displaydayPart.IsValid)
                    throw new Exception("Cannot save proposal detail without specifying daypart.");

                if (!validSpothLengths.ContainsValue(detail.SpotLengthId))
                    throw new Exception(string.Format("Invalid spot length for detail with flight '{0}-{1}'.", detail.FlightStartDate.Date, detail.FlightEndDate.Date));

                if (string.IsNullOrWhiteSpace(detail.DaypartCode))
                    throw new Exception(string.Format("Invalid daypart code for proposal detail with flight '{0}-{1}'.", detail.FlightStartDate.Date, detail.FlightEndDate.Date));

                if (detail.GenreCriteria.Exists(g => g.Contain == ContainTypeEnum.Include) && detail.GenreCriteria.Exists(g => g.Contain == ContainTypeEnum.Exclude))
                    throw new Exception("Cannot save proposal detail that contains both genre inclusion and genre exclusion criteria.");

                if (detail.ShowTypeCriteria.Exists(g => g.Contain == ContainTypeEnum.Include) && detail.ShowTypeCriteria.Exists(g => g.Contain == ContainTypeEnum.Exclude))
                    throw new Exception("Cannot save proposal detail that contains both show type inclusion and show type exclusion criteria.");

                if (detail.ProgramCriteria.Exists(g => g.Contain == ContainTypeEnum.Include) && detail.ProgramCriteria.Exists(g => g.Contain == ContainTypeEnum.Exclude))
                    throw new Exception("Cannot save proposal detail that contains both program name inclusion and program name exclusion criteria.");

                if ((detail.Daypart.startTime % secondsPerMinute > 0) || (detail.Daypart.endTime % secondsPerMinute > 0))
                    throw new Exception("Cannot save a daypart that has a start or an end time with non-zero seconds");

                if ((detail.Daypart.startTime > secondsPerDay) || (detail.Daypart.endTime > secondsPerDay))
                    throw new Exception("Cannot save a daypart that has a start time or end time over 24H");
            }
        }

        public ProposalDto GetProposalById(int proposalId)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalRepository.GetProposalById(proposalId);

                SetupProposalDto(proposal);
                return proposal;
            }
        }

        private void SetupProposalDto(ProposalDto proposal)
        {
            _SetProposalDetailDaypart(proposal.Details);
            _SetProposalSpotLengths(proposal);
            _SetProposalDetailFlightWeeks(proposal);
            _SetProposalFlightWeeksAndIds(proposal);
            _SetProposalMarketGroups(proposal);
            _SetProposalRatingBooks(proposal);
            _SetProposalMargins(proposal);
            _SetProposalCanBeDeleted(proposal);
            _SetProposalDetailIsciDay(proposal.Details);
        }

        private void _SetProposalDetailIsciDay(List<ProposalDetailDto> details)
        {
            details.ForEach(detail => detail.Quarters.ForEach(quarter => quarter.Weeks.ForEach(week => week.Iscis.ForEach(isci => _GenerateIsciDays(isci)))));
        }

        private void _GenerateIsciDays(ProposalWeekIsciDto isciDto)
        {
            isciDto.Days = string.Empty;
            if (isciDto.Sunday)
            {
                isciDto.Days = $"Su{ISCI_DAYS_DELIMITER}";
            }
            if (isciDto.Monday)
            {
                isciDto.Days = $"{isciDto.Days}M{ISCI_DAYS_DELIMITER}";
            }
            if (isciDto.Tuesday)
            {
                isciDto.Days = $"{isciDto.Days}T{ISCI_DAYS_DELIMITER}";
            }
            if (isciDto.Wednesday)
            {
                isciDto.Days = $"{isciDto.Days}W{ISCI_DAYS_DELIMITER}";
            }
            if (isciDto.Thursday)
            {
                isciDto.Days = $"{isciDto.Days}Th{ISCI_DAYS_DELIMITER}";
            }
            if (isciDto.Friday)
            {
                isciDto.Days = $"{isciDto.Days}F{ISCI_DAYS_DELIMITER}";
            }
            if (isciDto.Saturday)
            {
                isciDto.Days = $"{isciDto.Days}Sa";
            }

            isciDto.Days = isciDto.Days.EndsWith(ISCI_DAYS_DELIMITER.ToString()) ? isciDto.Days.Remove(isciDto.Days.Length - 1) : isciDto.Days;
            if (string.IsNullOrWhiteSpace(isciDto.Days))
                isciDto.Days = null;
        }

        public Dictionary<int, ProposalDto> GetProposalsByQuarterWeeks(List<int> quarterWeekIds)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposals = _ProposalRepository.GetProposalsByQuarterWeeks(quarterWeekIds);
                proposals.Values.ForEach(SetupProposalDto);
                return proposals;
            }

        }

        private void _SetProposalRatingBooks(ProposalDto proposal)
        {
            foreach (var proposalDetailDto in proposal.Details)
            {
                if (proposalDetailDto.SingleProjectionBookId != null)
                {
                    proposalDetailDto.ShareProjectionBookId = proposalDetailDto.SingleProjectionBookId;
                }
            }
        }

        private void _SetProposalMarketGroups(ProposalDto proposalDto)
        {
            proposalDto.MarketGroup = _GetMarketGroupDto(proposalDto.MarketGroupId);
            proposalDto.BlackoutMarketGroup = _GetMarketGroupDto(proposalDto.BlackoutMarketGroupId);
        }

        public ProposalDto GetProposalByIdWithVersion(int proposalId, int proposalVersion)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposal = _ProposalRepository.GetProposalByIdAndVersion(proposalId, proposalVersion);
                SetupProposalDto(proposal);
                return proposal;
            }
        }

        private void _SetProposalCanBeDeleted(ProposalDto proposal)
        {
            var proposalVersions = GetProposalVersionsByProposalId(proposal.Id.Value);
            proposal.CanDelete =
                !proposalVersions.Any(a => a.Status == ProposalEnums.ProposalStatusType.Contracted ||
                                           a.Status == ProposalEnums.ProposalStatusType.PreviouslyContracted);

        }

        private void _SetProposalMargins(ProposalDto proposal)
        {
            _ProposalTotalsCalculationEngine.CalculateProposalTotalsMargins(proposal);
        }

        private void _SetProposalDetailFlightWeeks(ProposalDto proposal)
        {
            if (proposal.Details == null || !proposal.Details.Any()) return;
            foreach (var detail in proposal.Details)
            {
                detail.FlightWeeks.Clear();

                var flights = detail.Quarters.SelectMany(q => q.Weeks.Select(w => new ProposalFlightWeek()
                {
                    StartDate = w.StartDate,
                    EndDate = w.EndDate,
                    IsHiatus = w.IsHiatus,
                    MediaWeekId = w.MediaWeekId
                })).ToList();

                detail.FlightWeeks.AddRange(flights);
            }

            proposal.FlightStartDate = proposal.Details.Select(a => a.FlightStartDate).Min();
            proposal.FlightEndDate = proposal.Details.Select(a => a.FlightEndDate).Max();
        }

        private void _SetProposalSpotLengths(ProposalDto proposal)
        {
            var spotsLengths = _SpotLengthRepository.GetSpotLengthIdsByDuration().OrderBy(m => m.Key);
            var detailSpots = proposal.Details.Select(a => a.SpotLengthId).Distinct().ToList();
            proposal.SpotLengths =
                spotsLengths.Where(d => detailSpots.Any(a => a == d.Value))
                    .Select(z => new LookupDto() { Id = z.Value, Display = z.Key.ToString() })
                    .ToList();
        }

        private void _SetProposalDetailDaypart(List<ProposalDetailDto> proposalDetail)
        {
            if (proposalDetail == null) return;
            foreach (var detail in proposalDetail)
            {
                detail.Daypart = DaypartDto.ConvertDisplayDaypart(_DaypartCache.GetDisplayDaypart(detail.DaypartId));
            }
        }

        private void _SetProposalDetailDaypartId(ProposalDto proposalDto)
        {
            if (proposalDto.Details == null) return;
            foreach (var detail in proposalDto.Details)
            {
                detail.DaypartId = _DaypartCache.GetIdByDaypart(DaypartDto.ConvertDaypartDto(detail.Daypart));
            }
        }

        public ProposalDetailDto GetProposalDetail(ProposalDetailRequestDto proposalDetailRequestDto)
        {
            var mediaWeeks = _MediaMonthAndWeekAggregateCache.GetMediaWeeksInRange(proposalDetailRequestDto.StartDate, proposalDetailRequestDto.EndDate);

            var proposalMediaWeeks = _GetProposalMediaWeeks(proposalDetailRequestDto, mediaWeeks);

            var proposalQuarterDto = _GetProposalQuarterDtos(proposalMediaWeeks);

            var defaultNtiConversionFactor =
                _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.DefaultNtiConversionFactor, 0.2);
            var proposalDetail = new ProposalDetailDto
            {
                FlightStartDate = proposalDetailRequestDto.StartDate,
                FlightEndDate = proposalDetailRequestDto.EndDate,
                Quarters = proposalQuarterDto.OrderBy(q => q.Year).ThenBy(q => q.Quarter).ToList(),
                DefaultProjectionBooks = _ProjectionBooksService.GetDefaultProjectionBooks(proposalDetailRequestDto.StartDate),
                NtiConversionFactor = defaultNtiConversionFactor
            };           
            _ProposalCalculationEngine.SetQuarterTotals(proposalDetail);

            return proposalDetail;
        }

        public ProposalDto CalculateProposalChanges(ProposalChangeRequest changeRequest)
        {
            var proposalDto = new ProposalDto();
            if (changeRequest.Id.HasValue)
            {
                proposalDto = _ProposalRepository.GetProposalById(changeRequest.Id.Value);
            }

            proposalDto.Details = changeRequest.Details;

            // deal with edited flights
            if (proposalDto.Details.Any(a => a.FlightEdited))
                _SetEditedProposalDetailFlights(proposalDto.Details);

            _ProposalCalculationEngine.UpdateProposal(proposalDto);

            _SetProposalSpotLengths(proposalDto);

            _SetProposalDetailFlightWeeks(proposalDto);

            _SetProposalFlightWeeksAndIds(proposalDto);

            _SetProposalDetailSequenceNumbers(proposalDto);

            return proposalDto;
        }


        // need to check for new proposal flight changes, including adding new quarter, weeks and also hiatus 
        private void _SetEditedProposalDetailFlights(List<ProposalDetailDto> proposalDetailDtos)
        {
            foreach (var editedDetail in proposalDetailDtos.Where(a => a.FlightEdited))
            {
                // generate new proposal detail quarters/weeks based on edited flight
                var newProposalDetail =
                    GetProposalDetail(new ProposalDetailRequestDto()
                    {
                        EndDate = editedDetail.FlightEndDate,
                        StartDate = editedDetail.FlightStartDate,
                        FlightWeeks =
                            editedDetail.FlightWeeks.Select(
                                a =>
                                    new FlightWeekDto()
                                    {
                                        EndDate = a.EndDate,
                                        StartDate = a.StartDate,
                                        IsHiatus = a.IsHiatus
                                    }).ToList()
                    });

                editedDetail.DefaultProjectionBooks = newProposalDetail.DefaultProjectionBooks;

                // deal with quarters - new quarters will be dealt by user.
                foreach (var newQuarterDto in newProposalDetail.Quarters)
                {
                    // need to identify if quarter already exists and update in case
                    var existingQuarter =
                        editedDetail.Quarters.SingleOrDefault(
                            q => q.Quarter == newQuarterDto.Quarter && q.Year == newQuarterDto.Year);
                    // update quarter details
                    if (existingQuarter != null)
                    {
                        newQuarterDto.Id = existingQuarter.Id; // if this match is not done, BE will not know what quarter to update when saving
                        newQuarterDto.Cpm = existingQuarter.Cpm;
                        newQuarterDto.DistributeGoals = existingQuarter.DistributeGoals;
                        newQuarterDto.ImpressionGoal = existingQuarter.ImpressionGoal;
                    }
                }

                // list of all weeks before edit
                var editedDetailWeeks = editedDetail.Quarters.SelectMany(w => w.Weeks).ToList();

                // deal with weeks - retrieve the weeks that match, to get corresponding values that will be used to calculate totals
                foreach (var weekDto in newProposalDetail.Quarters.SelectMany(w => w.Weeks))
                {
                    var week = editedDetailWeeks.SingleOrDefault(w => w.MediaWeekId == weekDto.MediaWeekId);
                    if (week != null)
                    {
                        weekDto.Id = week.Id;
                        weekDto.Impressions = week.Impressions;
                        weekDto.Cost = week.Cost;
                        weekDto.Units = week.Units;
                        weekDto.Iscis = week.Iscis;
                        weekDto.MyEventsReportName = week.MyEventsReportName;
                    }
                }

                editedDetail.FlightEdited = false;
                // clear quarters
                editedDetail.Quarters.Clear();
                // update quarters with new set of data
                editedDetail.Quarters.AddRange(newProposalDetail.Quarters);
            }
        }

        public ProposalLoadDto GetInitialProposalData(DateTime? currentDateTime)
        {
            var result = new ProposalLoadDto
            {
                Advertisers = _SmsClient.GetActiveAdvertisers(),
                Audiences = _AudiencesCache.GetAllLookups(),
                SpotLengths = _SpotLengthRepository.GetSpotLengths(),
                SchedulePostTypes = EnumExtensions.ToLookupDtoList<PostingTypeEnum>(),
                Markets = _BroadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>().GetMarketDtos()
                    .OrderBy(m => m.Display).ToList(),
                
                DefaultMarketCoverage = _ConfigurationSettingsHelper.GetConfigValueWithDefault(ConfigKeys.DefaultMarketCoverage, 0.8),
                ProprietaryPricingInventorySources = EnumHelper.GetProprietaryInventorySources().Select(p => new LookupDto
                {
                    Id = (int)p,
                    Display = p.Description()
                }).ToList(),
                ForecastDefaults = new ForecastRatingsDefaultsDto
                {
                    PlaybackTypes = EnumExtensions.ToLookupDtoList<ProposalEnums.ProposalPlaybackType>(),
                    CrunchedMonths = _RatingForecastService.GetMediaMonthCrunchStatuses()
                        .Where(a => a.Crunched == CrunchStatusEnum.Crunched)
                        .Select(
                            m => new LookupDto
                            {
                                Display = m.MediaMonth.LongMonthNameAndYear,
                                Id = m.MediaMonth.Id
                            })
                        .ToList()
                },
                Statuses = EnumExtensions.ToLookupDtoList<ProposalEnums.ProposalStatusType>(),
                MarketGroups = EnumExtensions.ToLookupDtoList<ProposalEnums.ProposalMarketGroups>()
            };
           
            return result;
        }

        private MarketGroupDto _GetMarketGroupDto(ProposalEnums.ProposalMarketGroups? marketGroup)
        {
            if (marketGroup == null)
                return null;

            return new MarketGroupDto
            {
                Id = (int)marketGroup,
                Display = marketGroup.Description()
            };
        }

        public List<DisplayProposalVersion> GetProposalVersionsByProposalId(int proposalId)
        {
            using (new TransactionScopeWrapper(TransactionScopeOption.Suppress, IsolationLevel.ReadUncommitted))
            {
                var proposalVersions = _ProposalRepository.GetProposalVersions(proposalId);
                var audiences = _AudiencesCache.GetAllEntities();
                var advertisers = _SmsClient.GetActiveAdvertisers();

                var result = proposalVersions.Select(
                    pv =>
                    {
                        var p = new DisplayProposalVersion();
                        p.Version = pv.VersionNumber;
                        var ad = advertisers.Find(a => a.Id == pv.AdvertiserId);
                        if (ad != null)
                            p.Advertiser = ad.Display;
                        p.DateModified = pv.LastModifiedDate;
                        var au =
                            audiences.Find(
                                a => pv.GuaranteedAudienceId.HasValue && a.Id == pv.GuaranteedAudienceId.Value);
                        if (au != null)
                            p.GuaranteedAudience = au.Name;
                        p.Notes = pv.Notes;
                        p.Owner = pv.Owner;
                        p.StartDate = pv.StartDate;
                        p.EndDate = pv.EndDate;
                        p.Status = pv.Status;

                        return p;
                    }).ToList();

                return result;
            }
        }

        private IEnumerable<ProposalQuarterDto> _GetProposalQuarterDtos(IEnumerable<ProposalMediaWeek> proposalMediaWeeks)
        {
            var proposalQuarterDto = new List<ProposalQuarterDto>();

            var groupedMediaWeeks =
                proposalMediaWeeks.OrderBy(week => week.Year).
                    ThenBy(week => week.Month).
                    GroupBy(week => new { week.Year, week.Quarter });

            foreach (var groupedMediaWeek in groupedMediaWeeks)
            {
                proposalQuarterDto.Add(new ProposalQuarterDto
                {
                    QuarterText = groupedMediaWeek.First().QuarterText,
                    Cpm = 0,
                    ImpressionGoal = 0,
                    Year = groupedMediaWeek.First().Year,
                    Quarter = groupedMediaWeek.First().Quarter,
                    Weeks = groupedMediaWeek.Select(week =>
                    {
                        var mediaWeek = new ProposalWeekDto();

                        var m = _MediaMonthAndWeekAggregateCache.GetMediaWeekById(week.MediaWeekId);
                        mediaWeek.StartDate = m.StartDate;
                        mediaWeek.EndDate = m.EndDate;
                        mediaWeek.MediaWeekId = week.MediaWeekId;
                        mediaWeek.Week = week.Week;
                        mediaWeek.Units = week.IsHiatus ? 0 : 1;
                        mediaWeek.Impressions = 0;
                        mediaWeek.Cost = 0;
                        mediaWeek.IsHiatus = week.IsHiatus;

                        return mediaWeek;
                    }).ToList()
                });
            }

            return proposalQuarterDto;
        }

        private int _GetQuarterId(int quarter, int year)
        {
            return Convert.ToInt32(quarter + year.ToString().Substring(2));
        }

        private IEnumerable<ProposalMediaWeek> _GetProposalMediaWeeks(ProposalDetailRequestDto proposalDetailRequestDto, IEnumerable<MediaWeek> mediaWeeks)
        {
            var proposalMediaWeeks = new List<ProposalMediaWeek>();

            foreach (var mediaWeek in mediaWeeks)
            {
                var mediaMonth = _MediaMonthAndWeekAggregateCache.GetMediaMonthById(mediaWeek.MediaMonthId);
                var isHiatus = false;

                if (proposalDetailRequestDto.FlightWeeks != null)
                    isHiatus = proposalDetailRequestDto.FlightWeeks.Any(weekDto => weekDto.StartDate == mediaWeek.StartDate &&
                                                                            weekDto.EndDate == mediaWeek.EndDate &&
                                                                            weekDto.IsHiatus);

                proposalMediaWeeks.Add(new ProposalMediaWeek
                {
                    MediaWeekId = mediaWeek.Id,
                    Month = mediaMonth.Month,
                    Year = mediaMonth.Year,
                    Quarter = (byte)mediaMonth.Quarter,
                    QuarterText = string.Format("{0} Q{1}", mediaMonth.Year, mediaMonth.Quarter),
                    QuarterId = _GetQuarterId(mediaMonth.Quarter, mediaMonth.Year),
                    Week = mediaWeek.StartDate.ToShortDateString(),
                    IsHiatus = isHiatus
                });
            }

            return proposalMediaWeeks;
        }

        /// <summary>
        /// Feature not available
        /// Generates one or more SCX files from the provided proposal Id, one file per proposal detail.
        /// </summary>
        /// <returns>Returns a list of proposal detail file names</returns>
        //public Tuple<string, Stream> GenerateScxFileArchive(int proposalId)
        //{
        //    string fileNameTemplate = "{0}({1}) - {2} - Export.scx";
        //    string fileArchiveTemplate = "{0}{1} Archive.zip";
        //    string detailInfoTemplate = "Flt {2:00} From {0} to {1}";

        //    var proposal = GetProposalById(proposalId);
        //    var scxFiles = _ProposalScxConverter.ConvertProposal(proposal);

        //    _ProposalOpenMarketInventoryService.SaveInventorySnapshot(proposal.Id.Value, proposal.Details.Select(x => x.Id.Value).ToList());

        //    MemoryStream archiveFile = new MemoryStream();

        //    string proposalName = proposal.ProposalName.PrepareForUsingInFileName();

        //    using (var arch = new ZipArchive(archiveFile, ZipArchiveMode.Create, true))
        //    {
        //        int ctr = 1;
        //        foreach (var scxFile in scxFiles)
        //        {
        //            var detailName = string.Format(detailInfoTemplate,
        //                scxFile.ProposalDetailDto.FlightStartDate.ToFileDateFormat(),
        //                scxFile.ProposalDetailDto.FlightEndDate.ToFileDateFormat(),
        //                ctr++);
        //            string scxFileName = string.Format(fileNameTemplate, proposalName, proposal.Id, detailName);

        //            var archiveEntry = arch.CreateEntry(scxFileName, CompressionLevel.Fastest);
        //            using (var zippedStreamEntry = archiveEntry.Open())
        //            {
        //                scxFile.ScxStream.CopyTo(zippedStreamEntry);
        //            }
        //        }
        //    }
        //    archiveFile.Seek(0, SeekOrigin.Begin);
        //    var archiveFileName = string.Format(fileArchiveTemplate, proposalName, proposal.Id);
        //    return new Tuple<string, Stream>(archiveFileName, archiveFile);
        //}

        public List<LookupDto> FindGenres(string genreSearchString)
        {
            return _GenreRepository.FindGenres(genreSearchString);
        }

        /// <summary>
        /// Finds show types based on the input string
        /// </summary>
        /// <param name="showTypeSearchString">Show type to filter by</param>
        /// <returns>List of LookupDto objects</returns>
        public List<LookupDto> FindShowType(string showTypeSearchString)
        {
            return _ShowTypeReporitory.FindMaestroShowType(showTypeSearchString);
        }

        public List<LookupDto> FindPrograms(ProgramSearchRequest request, string requestUrl)
        {
            if (request.Start < 1) request.Start = 1;
            string searchUrl;
            if (Debugger.IsAttached) //only for development
            {
                var url = new Uri(requestUrl);
                searchUrl = url.GetLeftPart(UriPartial.Authority) + "/api/Proposals/FindProgramsExternalApi";
            }
            else
            {
                searchUrl = _ConfigurationSettingsHelper.GetConfigValue<string>(ConfigKeys.ProgramSearchApiUrl);
            }

            var jsonRequest = JsonConvert.SerializeObject(request);

            using (var webClient = new WebClient())
            {
                webClient.UseDefaultCredentials = true;
                webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
                var jsonResult = webClient.UploadString(searchUrl, jsonRequest);
                var result = JsonConvert.DeserializeObject<BaseResponse<List<LookupDto>>>(jsonResult);
                return result.Data;
            }
        }

        public List<LookupDto> FindProgramsExternalApi(ProgramSearchRequest request)
        {
            if (request.Start < 1) request.Start = 1;
            return _ProgramNameRepository.FindPrograms(request.Name, request.Start, request.Limit);
        }

        public List<string> SaveProposalBuy(ProposalBuySaveRequestDto request)
        {
            var scxFile = new ScxFile(request.FileStream);
            var allStations = _StationRepository.GetBroadcastStations();
            var spotLengthsDict = _BroadcastDataRepositoryFactory.GetDataRepository<ISpotLengthRepository>().GetSpotLengthIdsByDuration();

            var proposalBuy = new ProposalBuyFile(scxFile, request.EstimateId, request.FileName, request.ProposalVersionDetailId,
                allStations, _MediaMonthAndWeekAggregateCache, _AudiencesCache, _DaypartCache, spotLengthsDict);

            if (!proposalBuy.Errors.Any())
            {
                using (var transaction = new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
                {
                    _BroadcastDataRepositoryFactory.GetDataRepository<IProposalBuyRepository>().DeleteProposalBuyByProposalDetail(proposalBuy.ProposalVersionDetailId);
                    _BroadcastDataRepositoryFactory.GetDataRepository<IProposalBuyRepository>().SaveProposalBuy(proposalBuy, request.Username, DateTime.Now);
                    transaction.Complete();
                }
            }

            return proposalBuy.Errors;
        }

        /// <summary>
        /// Feature not available
        /// </summary>
        /// <returns></returns>
        //public Tuple<string, Stream> GenerateScxFileDetail(int proposalDetailId)
        //{
        //    string fileNameTemplate = "{0}({1}) - {2} - Export.scx";
        //    string detailInfoTemplate = "Flt {2:00} From {0} to {1}";

        //    var proposal = _ProposalRepository.GetProposalByDetailId(proposalDetailId);
        //    var proposalDetail = proposal.Details.Single(d => d.Id == proposalDetailId);
        //    ProposalScxFile scxFile = null;
        //    _ProposalScxConverter.ConvertProposalDetail(proposal, proposalDetail, ref scxFile);
        //    if (scxFile == null)
        //        throw new InvalidOperationException("Could not generate SCX file.");

        //    _ProposalOpenMarketInventoryService.SaveInventorySnapshot(proposal.Id.Value, new List<int> { proposalDetailId });

        //    string proposalName = proposal.ProposalName.PrepareForUsingInFileName();

        //    var detailName = string.Format(detailInfoTemplate,
        //        scxFile.ProposalDetailDto.FlightStartDate.ToFileDateFormat(),
        //        scxFile.ProposalDetailDto.FlightEndDate.ToFileDateFormat(),
        //        1);

        //    var detailFileName = string.Format(fileNameTemplate, proposalName, proposal.Id, detailName);

        //    return new Tuple<string, Stream>(detailFileName, scxFile.ScxStream);
        //}

        public string AlignProposalDaypartsToZeroSeconds()
        {
            var messageBuilder = new StringBuilder();
            List<Tuple<int, int>> proposalDetailDaypartList = _ProposalRepository.GetIdsOfProposalDetailsWithMisalignedDayparts();
            List<Tuple<int, int>> updateProposalDetailDaypartMap = new List<Tuple<int, int>>();
            foreach (var detailDaypart in proposalDetailDaypartList)
            {
                messageBuilder.AppendLine($"Updating daypart on proposal detail {detailDaypart.Item1}");

                var daypart = _DaypartCache.GetDisplayDaypart(detailDaypart.Item2);
                daypart.StartTime = daypart.StartTime - (daypart.StartTime % 60);
                if (daypart.EndTime % 60 != 59)
                {
                    daypart.EndTime = daypart.EndTime - (daypart.EndTime % 60) - 1;
                }
                var daypartId = _DaypartCache.GetIdByDaypart(daypart);
                updateProposalDetailDaypartMap.Add(Tuple.Create(detailDaypart.Item1, daypartId));
            }
            _ProposalRepository.UpdateProposalDetailDayparts(updateProposalDetailDaypartMap);
            return messageBuilder.ToString();
        }

        public BroadcastLockResponse LockProposal(int proposalId)
        {
            var broadcastLockResponse = _LockingEngine.LockProposal(proposalId);

            return broadcastLockResponse;
        }

        public BroadcastReleaseLockResponse UnlockProposal(int proposalId)
        {
            var broadcastReleaseLockResponse = _LockingEngine.UnlockProposal(proposalId);

            return broadcastReleaseLockResponse;
        }
    }
}