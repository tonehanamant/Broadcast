using Common.Services;
using Common.Services.ApplicationServices;
using Common.Services.Extensions;
using Common.Services.Repositories;
using Common.Systems.LockTokens;
using EntityFrameworkMapping.Broadcast;
using Services.Broadcast.BusinessEngines;
using Services.Broadcast.Entities;
using Services.Broadcast.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Transactions;
using Services.Broadcast.Converters;
using Services.Broadcast.Entities.spotcableXML;
using Tam.Maestro.Common;
using Tam.Maestro.Common.DataLayer;
using Tam.Maestro.Data.Entities;
using Tam.Maestro.Data.Entities.DataTransferObjects;
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
        ProposalDto UpdateProposal(List<ProposalDetailDto> proposalDetailDtos);
        ProposalDto UnorderProposal(int proposalId, string username);
        Tuple<string, Stream> GenerateScxFileArchive(int proposalIds);
        ValidationWarningDto DeleteProposal(int proposalId);
        Dictionary<int, ProposalDto> GetProposalsByQuarterWeeks(List<int> quarterWeekIds);
        List<LookupDto> FindGenres(string genreSearchString);        
    }

    public class ProposalService : IProposalService
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
        private readonly IProposalMarketsCalculationEngine _ProposalMarketsCalculationEngine;
        private readonly IProposalScxConverter _ProposalScxConverter;
        private readonly IPostingBooksService _PostingBooksService;
        private readonly IRatingForecastService _RatingForecastService;
        private readonly IProposalTotalsCalculationEngine _ProposalTotalsCalculationEngine;
        private readonly IProposalProprietaryInventoryService _ProposalProprietaryInventoryService;
        private readonly IProposalOpenMarketInventoryService _ProposalOpenMarketInventoryService;

        public ProposalService(IDataRepositoryFactory broadcastDataRepositoryFactory,
            IMediaMonthAndWeekAggregateCache mediaMonthAndWeekAggregateCache,
            IBroadcastAudiencesCache audiencesCache,
            ISMSClient smsClient,
            IDaypartCache daypartCache,
            IProposalCalculationEngine proposalCalculationEngine,
            IProposalMarketsCalculationEngine proposalMarketsCalculationEngine,
            IProposalScxConverter proposalScxConverter,
            IPostingBooksService postingBooksService,
            IRatingForecastService ratingForecastService,
            IProposalTotalsCalculationEngine proposalTotalsCalculationEngine,
            IProposalProprietaryInventoryService proposalProprietaryInventoryService,
            IProposalOpenMarketInventoryService proposalOpenMarketInventoryService)
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
            _ProposalMarketsCalculationEngine = proposalMarketsCalculationEngine;
            _ProposalScxConverter = proposalScxConverter;
            _PostingBooksService = postingBooksService;
            _RatingForecastService = ratingForecastService;
            _ProposalTotalsCalculationEngine = proposalTotalsCalculationEngine;
            _ProposalProprietaryInventoryService = proposalProprietaryInventoryService;
            _ProposalOpenMarketInventoryService = proposalOpenMarketInventoryService;
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
            using (saveRequest.Id.HasValue ? new BomsLockManager(_SmsClient, new ProposalToken(saveRequest.Id.Value)) : null)
            {
                _ValidateProposalDtoBeforeSave(saveRequest, userName);

                if (saveRequest.Id.HasValue && saveRequest.Version.HasValue)
                {
                    var proposal = GetProposalByIdWithVersion(saveRequest.Id.Value, saveRequest.Version.Value);

                    if (proposal.Status == ProposalEnums.ProposalStatusType.Proposed && saveRequest.Status == ProposalEnums.ProposalStatusType.Contracted)
                        throw new Exception("Cannot change proposal status from Proposed to Contracted");

                    EnsureContractedOnHoldStatus(saveRequest, proposal);

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
                    EnsureContractedOnHoldStatus(saveRequest);
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

        private void EnsureContractedOnHoldStatus(ProposalDto saveRequest, ProposalDto proposal = null)
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
                if (proposalDetailDto.HutPostingBookId == null &&
                    proposalDetailDto.SharePostingBookId != null)
                {
                    proposalDetailDto.SinglePostingBookId = proposalDetailDto.SharePostingBookId;
                    proposalDetailDto.HutPostingBookId = null;
                    proposalDetailDto.SharePostingBookId = null;
                }
                else
                {
                    proposalDetailDto.SinglePostingBookId = null;
                }
            }
        }

        private int _SaveProposal(ProposalDto proposalDto, string userName)
        {
            using (var transaction = new TransactionScopeWrapper(IsolationLevel.ReadUncommitted))
            {
                _SetProposalDefaultValues(proposalDto);

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
                }
                else
                {
                    _ProposalRepository.CreateProposal(proposalDto, userName);
                }

                transaction.Complete();

                return proposalDto.Id.Value;
            }
        }

        private void _SetProposalDefaultValues(ProposalDto proposalDto)
        {
            // set target and default margin that are nullable when first creating a proposal
            proposalDto.TargetCPM = proposalDto.TargetCPM ?? 0;
            proposalDto.Margin = proposalDto.Margin ?? ProposalConstants.ProposalDefaultMargin;

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

                var markets = _ProposalMarketsCalculationEngine.GetProposalMarketsList(proposalDto.Id.Value, proposalDto.Version.Value, proposalDetailDto.Id.Value);
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

                        if (marketIds.Contains(broadcastStation.MarketCode))
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

            _ValidatePreviouslyContractedStatus(proposalDto);

            //Will throw an exception if advertiser not found:
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

        private void _ValidateProposalRatingsBooks(ProposalDetailDto proposalDetailDto)
        {
            if (proposalDetailDto.SharePostingBookId == null)
                throw new Exception("Cannot save proposal without specifying a Share Book");

            if (proposalDetailDto.PlaybackType == null)
                throw new Exception("Cannot save proposal without specifying a Playback Type");

            if (proposalDetailDto.SharePostingBookId == proposalDetailDto.HutPostingBookId ||
                proposalDetailDto.HutPostingBookId > proposalDetailDto.SharePostingBookId)
                throw new Exception("Hut Media Month must be earlier (less than) the Share Media Month");
        }

        private void _ValidateProposalDetailBeforeSave(ProposalDto proposalDto)
        {
            var validSpothLengths = _SpotLengthRepository.GetSpotLengthAndIds();

            foreach (var detail in proposalDto.Details)
            {
                _ValidateProposalRatingsBooks(detail);

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

                if (detail.ProgramCriteria.Exists(g => g.Contain == ContainTypeEnum.Include) && detail.ProgramCriteria.Exists(g => g.Contain == ContainTypeEnum.Exclude))
                    throw new Exception("Cannot save proposal detail that contains both program name inclusion and program name exclusion criteria.");
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
                if (proposalDetailDto.SinglePostingBookId != null)
                {
                    proposalDetailDto.SharePostingBookId = proposalDetailDto.SinglePostingBookId;
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
                _SetProposalDetailDaypart(proposal.Details);
                _SetProposalSpotLengths(proposal);
                _SetProposalDetailFlightWeeks(proposal);
                _SetProposalFlightWeeksAndIds(proposal);
                _SetProposalMarketGroups(proposal);
                _SetProposalRatingBooks(proposal);
                _SetProposalMargins(proposal);
                _SetProposalCanBeDeleted(proposal);
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
            var spotsLengths = _SpotLengthRepository.GetSpotLengthAndIds().OrderBy(m => m.Key);
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

            var proposalDetail = new ProposalDetailDto
            {
                FlightStartDate = proposalDetailRequestDto.StartDate,
                FlightEndDate = proposalDetailRequestDto.EndDate,
                Quarters = proposalQuarterDto.ToList(),
                DefaultPostingBooks = _PostingBooksService.GetDefaultPostingBooks(proposalDetailRequestDto.StartDate)
            };

            _ProposalCalculationEngine.SetQuarterTotals(proposalDetail);

            return proposalDetail;
        }

        public ProposalDto UpdateProposal(List<ProposalDetailDto> proposalDetailDtos)
        {
            var proposalDto = new ProposalDto
            {
                Details = proposalDetailDtos
            };

            // deal with edited flights
            if (proposalDetailDtos.Any(a => a.FlightEdited))
                _SetEditedProposalDetailFlights(proposalDetailDtos);

            _ProposalCalculationEngine.UpdateProposal(proposalDto);

            _SetProposalSpotLengths(proposalDto);

            _SetProposalDetailFlightWeeks(proposalDto);

            _SetProposalFlightWeeksAndIds(proposalDto);

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

                editedDetail.DefaultPostingBooks = newProposalDetail.DefaultPostingBooks;

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
            var broadcastSpothLengths = new List<int>()
            {
                15,
                30,
                60,
                90,
                120
            };

            var result = new ProposalLoadDto();
            result.Advertisers = _SmsClient.GetActiveAdvertisers();
            result.Audiences = _AudiencesCache.GetAllLookups();
            result.SpotLengths = _SpotLengthRepository.GetSpotLengthsByLength(broadcastSpothLengths);
            result.SchedulePostTypes =
                Enum.GetValues(typeof(SchedulePostType))
                    .Cast<SchedulePostType>()
                    .Select(e => new LookupDto { Display = e.ToString(), Id = (int)e })
                    .ToList();
            result.Markets = _BroadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>().GetMarketDtos().OrderBy(m => m.Display).ToList();
            result.MarketGroups = _GetMarketGroupList(result.Markets.Count);
            result.ForecastDefaults = new ForecastRatingsDefaultsDto
            {
                PlaybackTypes = EnumExtensions.ToLookupDtoList<ProposalEnums.ProposalPlaybackType>(),
                CrunchedMonths = _RatingForecastService.GetMediaMonthCrunchStatuses()
                    .Where(a => a.Crunched == CrunchStatus.Crunched)
                    .Select(
                        m => new LookupDto()
                        {
                            Display = m.MediaMonth.LongMonthNameAndYear,
                            Id = m.MediaMonth.Id
                        })
                    .ToList()
            };
            result.Statuses = EnumExtensions.ToLookupDtoList<ProposalEnums.ProposalStatusType>();
            return result;
        }

        private List<MarketGroupDto> _GetMarketGroupList(int totalMarkets)
        {
            //Assumes Enum ID corresponds to the number of markets (except for ALL)
            var marketGroups =
                EnumExtensions.ToLookupDtoList<ProposalEnums.ProposalMarketGroups>().Select(
                    g => new MarketGroupDto()
                    {
                        Display = g.Display,
                        Id = g.Id,
                        Count = g.Id
                    }).ToList();

            var totalMarketsGroup =
                marketGroups.Where(g => g.Id == (int)ProposalEnums.ProposalMarketGroups.All).Single();
            totalMarketsGroup.Count = totalMarkets;

            var customGroup = marketGroups.Where(g => g.Id == (int)ProposalEnums.ProposalMarketGroups.Custom).Single();
            customGroup.Count = 0;

            return marketGroups;
        }

        private MarketGroupDto _GetMarketGroupDto(ProposalEnums.ProposalMarketGroups? marketGroup)
        {
            if (marketGroup == null || marketGroup == ProposalEnums.ProposalMarketGroups.Custom)
                return null;

            var marketCount = _GetCountForMarketGroup(marketGroup.Value);

            return new MarketGroupDto
            {
                Count = marketCount,
                Display = marketGroup.Description(),
                Id = (int)marketGroup
            };
        }

        private int _GetCountForMarketGroup(ProposalEnums.ProposalMarketGroups marketGroup)
        {
            int marketCount;

            if (marketGroup == ProposalEnums.ProposalMarketGroups.All)
            {
                marketCount =
                    _BroadcastDataRepositoryFactory.GetDataRepository<IMarketRepository>().GetMarketDtos().Count;
            }
            else if (marketGroup == ProposalEnums.ProposalMarketGroups.Custom)
            {
                marketCount = 0;
            }
            else
            {
                marketCount = (int)marketGroup;
            }

            return marketCount;
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
        /// Generates one or more SCX files from the provided proposal Id, one file per proposal detail.
        /// </summary>
        /// <returns>Returns a list of proposal detail file names</returns>
        public Tuple<string, Stream> GenerateScxFileArchive(int proposalId)
        {
            string fileNameTemplate = "{0}({1}) - {2} - Export.scx";
            string fileArchiveTemplate = "{0}{1} Archive.zip";
            string detailInfoTemplate = "Flt {2:00} From {0} to {1}";

            var proposal = GetProposalById(proposalId);
            var scxFiles = _ProposalScxConverter.ConvertProposal(proposal);

            MemoryStream archiveFile = new MemoryStream();

            string proposalName = FormatProposalName(proposal.ProposalName);
            using (var arch = new ZipArchive(archiveFile, ZipArchiveMode.Create, true))
            {
                int ctr = 1;
                foreach (var scxFile in scxFiles)
                {
                    var detailName = string.Format(detailInfoTemplate,
                        _FileDateFormat(scxFile.ProposalDetailDto.FlightStartDate),
                        _FileDateFormat(scxFile.ProposalDetailDto.FlightEndDate),
                        ctr++);
                    string scxFileName = string.Format(fileNameTemplate, proposalName, proposal.Id, detailName);

                    var archiveEntry = arch.CreateEntry(scxFileName, CompressionLevel.Fastest);
                    using (var zippedStreamEntry = archiveEntry.Open())
                    {
                        scxFile.ScxStream.CopyTo(zippedStreamEntry);
                    }
                }
            }
            archiveFile.Seek(0, SeekOrigin.Begin);
            var archiveFileName = string.Format(fileArchiveTemplate, proposalName, proposal.Id);
            return new Tuple<string, Stream>(archiveFileName, archiveFile);
        }

        /// <summary>
        /// Preps the proposal name for using in a file name.
        /// </summary>
        private string FormatProposalName(string proposalName)
        {
            return proposalName.Replace("\\", string.Empty)
                                .Replace(":", string.Empty)
                                .Replace("*", string.Empty)
                                .Replace("?", string.Empty)
                                .Replace("/", string.Empty)
                                .Replace("<", string.Empty)
                                .Replace(">", string.Empty)
                                .Replace("|", string.Empty)
                                .Replace("\"", string.Empty);
        }

        private string _FileDateFormat(DateTime date)
        {
            return date.ToString("MMddyyyyy");
        }


        public List<LookupDto> FindGenres(string genreSearchString)
        {
            return _GenreRepository.FindGenres(genreSearchString);
        }        
    }
}