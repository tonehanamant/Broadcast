var viewModes = {
    create: "create",
    view: "view"
};

var ProposalViewModel = function (controller) {
    var $scope = this;

    $scope.controller = controller;

    /*** DISPLAY ***/


    /*** MODAL HANDLING ***/
    //NOTE: set modal events directly - going through KO binded delays execution

    $scope.showModal = ko.observable(false);

    //prevent click outside closing (todo: possibly add properties to KO custom modal)
    $('#proposal_modal').modal({ backdrop: 'static', show: false, keyboard: false });

    //hide event
    //intercept - before hide to check unsaved on view (todo: possibly add event to KO custom modal);
    $('#proposal_modal').on('hide.bs.modal', function (e) {
        
        if ($scope.showModal() && $scope.controller.proposalView.checkUnsavedSpots()) {
            e.preventDefault();
            e.stopImmediatePropagation();
            return false;
        }

    });

    //hidden event
    $('#proposal_modal').on('hidden.bs.modal', function (e) {
        if ($scope.proposalId()) {
            $scope.controller.apiGetUnlock($scope.proposalId(), function(unlockResponse) {
                if (!unlockResponse.Success) {
                    util.notify("Error unlocking the proposal", "danger");
                }
            });
        }
      
        //reset collapse state in header
        $scope.controller.proposalView.resetCollapse();
        $scope.canUpdatePrograms(false);
        $scope.showModal(false);
        $scope.clear();

        //refresh main
        $scope.controller.planningController.view.loadGrid();
    });

    //shown event
    $('#proposal_modal').on('shown.bs.modal', function (e) {
        $scope.resetState();
        $scope.controller.proposalView.clearValidation();

        if ($scope.programs() || $scope.viewMode() == viewModes.view) {
            $scope.loadViewGrid($scope.programs());
            $scope.canUpdatePrograms(true);
        }

        $scope.controller.proposalView.initInputMasks();
        $scope.showModal(true);

        // default values for new proposal
        if ($scope.viewMode() == viewModes.create) {
            $scope.selectedAdvertiser($scope.advertisers()[0].Id);
            $scope.selectedSpotLength(1); 
            $scope.selectedMarket(100);
            $scope.selectedAudience(31);
        }
    });

    $scope.viewMode = ko.observable();
    $scope.viewMode.subscribe(function (viewMode) {
        if (viewMode == viewModes.create) {
            $scope.clear();
        } else {
            $scope.canUpdatePrograms(true);
        }
    });

    $scope.resetState = function () {
        $scope.clearFilters();
        $scope.selectedAdditionalDemographicId(null);
        $scope.controller.proposalView.setResetState(true);
    };

    /*** PROPOSAL ***/

    $scope.cacheGuid = ko.observable();
    $scope.proposalId = ko.observable();
    $scope.version = ko.observable();
    $scope.primaryVersionId = ko.observable();

    $scope.proposalName = ko.observable();
    $scope.proposalTitle = ko.computed(function () {
        if ($scope.version()) {
            return $scope.proposalName() + " - Version: " + $scope.version();
        } else {
            return $scope.proposalName();
        }
    });

    $scope.targetBudget = ko.observable();
    $scope.targetImpressions = ko.observable();
    $scope.targetUnits = ko.observable();
    $scope.targetUnits.subscribe(function (value) {
        if (!value)
            $scope.targetUnits(1);
    });

    $scope.flightStartDate = ko.observable();
    $scope.flightEndDate = ko.observable();
    $scope.flightWeeks = ko.observable();

    $scope.notes = ko.observable();
    $scope.sweepMonthId = ko.observable();

    $scope.programNameSearchCriteria = ko.observableArray();
    $scope.daypartSearchCriteria = ko.observableArray();
    $scope.genreSearchCriteria = ko.observableArray();

    $scope.unsavedPrograms = ko.observable(true);
    $scope.programs = ko.observable();
    $scope.programs.subscribe(function (programs) {
        if (!$scope.canUpdatePrograms() && $scope.programs() && $scope.programs().DisplayProposalMarketPrograms && $scope.programs().DisplayProposalMarketPrograms.length) {
            $scope.canUpdatePrograms(true);
        }
        $scope.loadViewGrid(programs);
    });
    $scope.hasPrograms = ko.computed(function () {
        return $scope.programs() && $scope.programs().DisplayProposalMarketPrograms && $scope.programs().DisplayProposalMarketPrograms.length > 0;
    });

    $scope.status = ko.observable();

    $scope.shareBook = ko.observable();
    $scope.hutBook = ko.observable();
    $scope.playbackType = ko.observable();

    $scope.isValid = function () {
        $scope.controller.proposalView.clearValidation();
        var isValid = $scope.controller.proposalView.isValid();

        if (isValid && $scope.viewMode == viewModes.create) {
            if (($scope.programNameSearchCriteria() && $scope.programNameSearchCriteria().length > 0) ||
                ($scope.daypartSearchCriteria() && $scope.daypartSearchCriteria().length > 0) ||
                ($scope.genreSearchCriteria() && $scope.genreSearchCriteria().length > 0)) {

                isValid = $scope.controller.proposalView.isValidForCriteria();
            }
        }

        return isValid;
    };

    /*** OPTIONS ***/

    $scope.advertisers = ko.observableArray();
    $scope.selectedAdvertiser = ko.observable();

    $scope.spotLengths = ko.observableArray();
    $scope.selectedSpotLength = ko.observable();

    $scope.markets = ko.observableArray();
    $scope.selectedMarket = ko.observable();

    $scope.audiences = ko.observableArray();
    $scope.selectedAudience = ko.observable();
    $scope.selectedAudience.subscribe(function (newValue) {
        if (newValue === 31) {
            $scope.controller.proposalView.displayGridHHcolumns(false);

        } else {
            $scope.controller.proposalView.displayGridHHcolumns(true);
        }
    });

    /** METHODS ***/

    $scope.loadViewGrid = function (proposalData) {
        if ($scope.showModal()) {
            console.log('load grid');
            $scope.controller.proposalView.loadGrid(proposalData);
        }
    };

    $scope.loadOptions = function (options) {
        $scope.advertisers(options.Advertisers);
        $scope.spotLengths(options.SpotLengths);
        $scope.markets(options.MarketGroups);
        $scope.audiences(options.Audiences);

        // manage ratings options
        if (options.ForecastDefaults) {
            $scope.controller.manageRatingsViewModel.shareBooksOptions(options.ForecastDefaults.CrunchedMonths);
            $scope.controller.manageRatingsViewModel.selectedShareBook(options.ForecastDefaults.DefaultShareBookId);

            $scope.controller.manageRatingsViewModel.hutBookOptions(options.ForecastDefaults.CrunchedMonths);
            $scope.controller.manageRatingsViewModel.selectedHutBook(options.ForecastDefaults.DefaultHutBookId);

            $scope.controller.manageRatingsViewModel.playbackTypeOptions(options.ForecastDefaults.PlaybackTypes);
            $scope.controller.manageRatingsViewModel.selectedPlaybackType(options.ForecastDefaults.DefaultPlaybackType);
        }
    };

    $scope.load = function (proposal) {
        var canUpdate = $scope.canUpdatePrograms();
        $scope.canUpdatePrograms(false);

        $scope.cacheGuid(proposal.CacheGuid);
        $scope.proposalId(proposal.Id);
        $scope.version(proposal.Version);
        $scope.primaryVersionId(proposal.PrimaryVersionId);
        $scope.proposalName(proposal.ProposalName);
        $scope.selectedAdvertiser(proposal.AdvertiserId);
        $scope.targetBudget(proposal.TargetBudget);
        $scope.targetImpressions(proposal.TargetImpressions);
        $scope.targetUnits(proposal.TargetUnits);

        $scope.flightStartDate(proposal.FlightStartDate);
        $scope.flightEndDate(proposal.FlightEndDate);

        $scope.flightWeeks(proposal.FlightWeeks);
        $scope.selectedSpotLength(proposal.SpotLength);
        $scope.selectedMarket(proposal.Market);
        $scope.selectedAudience(proposal.GuaranteedDemoId);
        $scope.notes(proposal.Notes);

        var criteria = proposal.ProposalProgramsCriteria;
        $scope.programNameSearchCriteria(criteria ? criteria.ProgramNameSearchCriteria : null);
        $scope.daypartSearchCriteria(criteria ? criteria.DaypartSearchCriteria : null);
        $scope.genreSearchCriteria(criteria ? criteria.GenreSearchCriteria : null);
        $scope.sweepMonthId(proposal.SweepMonthId);
        $scope.status(proposal.Status);

        $scope.unsavedPrograms(proposal.UnsavedPrograms);
        $scope.programs(proposal.ProposalPrograms);
        $scope.canUpdatePrograms(canUpdate);

        $scope.controller.proposalView.initInputMasks();

        var filter = proposal.ProgramDisplayFilter;
        $scope.controller.filterViewModel.genresOptions(filter ? filter.Genres: null);
        $scope.controller.filterViewModel.marketsOptions(filter ? filter.Markets: null);
        $scope.controller.filterViewModel.affiliatesOptions(filter ? filter.Affiliations : null);
        $scope.controller.filterViewModel.programNamesOptions(filter ? filter.ProgramNames : null);

        // manage ratings
        $scope.shareBook(proposal.ShareBookMonthId);
        $scope.hutBook(proposal.HutBookMonthId);
        $scope.playbackType(proposal.PlayBackType);
    };

    $scope.clear = function () {
        var canUpdate = $scope.canUpdatePrograms();
        $scope.canUpdatePrograms(false);

        $scope.cacheGuid(null);
        $scope.proposalId(null);
        $scope.version(null);
        $scope.primaryVersionId(null);
        $scope.proposalName(null);
        $scope.selectedAdvertiser(null);
        $scope.targetBudget(null);
        $scope.targetImpressions(null);
        $scope.targetUnits(1);
        $scope.flightStartDate(null);
        $scope.flightEndDate(null);
        $scope.flightWeeks(null);
        $scope.selectedSpotLength(null);
        $scope.selectedMarket(null);
        $scope.selectedAudience(null);
        $scope.notes(null);
        $scope.programNameSearchCriteria(null);
        $scope.daypartSearchCriteria(null);
        $scope.genreSearchCriteria(null);
        $scope.sweepMonthId(null);
        $scope.status(null);
        $scope.unsavedPrograms(true);
        $scope.programs(null);
        $scope.selectedAdditionalDemographicId(null);
        $scope.mustFilter(false);

        // ratings
        $scope.shareBook(null);
        $scope.hutBook(null);
        $scope.playbackType(null);

        $scope.canUpdatePrograms(canUpdate);
    };

    $scope.getProposalForSave = function () {
        return {
            Id: $scope.proposalId(),
            Version: $scope.version(),
            PrimaryVersionId: $scope.primaryVersionId(),
            ProposalName: $scope.proposalName(),
            AdvertiserId: $scope.selectedAdvertiser(),
            TargetBudget: $scope.targetBudget() ? $scope.targetBudget().toString().replace(/\$|,/g, "") : null,
            TargetImpressions: $scope.targetImpressions(),
            TargetUnits: $scope.targetUnits() ? $scope.targetUnits().toString().replace(/,/g, "") : null,
            FlightStartDate: $scope.flightStartDate(),
            FlightEndDate: $scope.flightEndDate(),
            FlightWeeks: $scope.flightWeeks(),
            SpotLength: $scope.selectedSpotLength(),
            Market: $scope.selectedMarket(),
            GuaranteedDemoId: $scope.selectedAudience(),
            Notes: $scope.notes(),
            SweepMonthId: $scope.sweepMonthId() || 413,
            Status: $scope.status() || 0,
            CacheGuid: $scope.cacheGuid(),
            AdditionalAudienceId: $scope.selectedAdditionalDemographicId(),

            ProgramFilter: {
                ProgramNames: $scope.controller.filterViewModel.getCriteriaValuesByProperty("Program Name"),
                Genres: $scope.controller.filterViewModel.getCriteriaValuesByProperty("Genre"),
                DayParts: $scope.controller.filterViewModel.getCriteriaValuesByProperty("Airing Time"),
                Affiliations: $scope.controller.filterViewModel.getCriteriaValuesByProperty("Affiliation"),
                Markets: $scope.controller.filterViewModel.getCriteriaValuesByProperty("Market"),
                ProgramWithSpots: $scope.withSpots(),
            },

            ShareBookMonthId: $scope.shareBook(),
            HutBookMonthId: $scope.hutBook(),
            PlayBackType: $scope.playbackType()
        }
    };

    $scope.getProposalForCriteriaFilter = function () {
        return {
            Id: $scope.proposalId(),
            Version: $scope.version(),
            PrimaryVersionId: $scope.primaryVersionId(),
            ProposalName: $scope.proposalName(),
            AdvertiserId: $scope.selectedAdvertiser(),
            TargetBudget: $scope.targetBudget() ? $scope.targetBudget().toString().replace(/\$|,/g, "") : null,
            TargetImpressions: $scope.targetImpressions(),
            TargetUnits: $scope.targetUnits() ? $scope.targetUnits().toString().replace(/,/g, "") : null,
            FlightStartDate: $scope.flightStartDate(),
            FlightEndDate: $scope.flightEndDate(),
            FlightWeeks: $scope.flightWeeks(),
            SpotLength: $scope.selectedSpotLength(),
            Market: $scope.selectedMarket(),
            GuaranteedDemoId: $scope.selectedAudience(),

            Notes: $scope.notes(),
            ProposalProgramsCriteria: {
                DaypartSearchCriteria: $scope.daypartSearchCriteria(),
                GenreSearchCriteria: $scope.genreSearchCriteria(),
                ProgramNameSearchCriteria: $scope.programNameSearchCriteria()
            },

            SweepMonthId: $scope.sweepMonthId() || 413,
            Status: $scope.status() || 0,
            CacheGuid: $scope.cacheGuid(),
            AdditionalAudienceId: $scope.selectedAdditionalDemographicId(),

            ShareBookMonthId: $scope.shareBook(),
            HutBookMonthId: $scope.hutBook(),
            PlayBackType: $scope.playbackType()
        }
    };

    /*** GRID UPDATE ***/

    $scope.canUpdatePrograms = ko.observable(false);

    $scope.displayProgramsGrid = ko.computed(function () {
        if ($scope.viewMode() == viewModes.view) {
            return true;
        }

        return $scope.canUpdatePrograms();
    });

    $scope.enableGridUpdate = ko.computed(function () {
        return $scope.canUpdatePrograms() && $scope.displayProgramsGrid();
    });

    $scope.reloadPrograms = function (subscribedValue) {
        $scope.controller.proposalView.clearValidation();
        $scope.resetState();

        if (!subscribedValue && $scope.programs() && $scope.programs().DisplayProposalMarketPrograms && $scope.programs().DisplayProposalMarketPrograms.length) {
            $scope.programs(null);
            $scope.controller.proposalView.isValidForCriteria();
        } else {
            if ($scope.enableGridUpdate() && $scope.controller.proposalView.isValidForCriteria()) {
                $scope.controller.apiGetPrograms($scope.getProposalForCriteriaFilter(), function (proposalDto) {
                    if (proposalDto && proposalDto.ProposalPrograms) {
                        $scope.unsavedPrograms(proposalDto.UnsavedPrograms);
                        $scope.load(proposalDto);

                        util.notify('Programs Updated');
                    }
                });
            }
        }
    };

    $scope.flightStartDate.subscribe($scope.reloadPrograms);
    $scope.flightEndDate.subscribe($scope.reloadPrograms);
    $scope.selectedMarket.subscribe($scope.reloadPrograms);
    $scope.selectedSpotLength.subscribe($scope.reloadPrograms);
    $scope.selectedAudience.subscribe($scope.reloadPrograms);

    /*** MENU ***/

    $scope.save = function () {
        if ($scope.isValid()) {
            var proposalRequest = $scope.getProposalForSave();

            $scope.controller.apiSaveProposal(proposalRequest, function (proposal) {
                $scope.controller.proposalView.setResetState(false);
                $scope.controller.proposalView.setPendingUnsavedSpots(false);
                $scope.load(proposal);
                $scope.viewMode(viewModes.view);

                util.notify('Proposal saved successfully', 'success');
               
                $scope.controller.apiGetLock($scope.proposalId());
            });
        }
    };

    $scope.saveAsVersion = function () {
        if ($scope.isValid()) {
            var newVersionRequest = $scope.getProposalForSave();
            newVersionRequest.Version = null;
           
            $scope.controller.apiSaveProposal(newVersionRequest, function (proposal) {
               
                $scope.controller.proposalView.setResetState(false);
                $scope.controller.proposalView.setPendingUnsavedSpots(false);
                $scope.load(proposal);
                $scope.controller.versionCreatedOptionsViewModel.newVersion(proposal.Version);
                $scope.controller.versionCreatedOptionsViewModel.show(true);
               
            });
        }
    };

    $scope.switchVersion = function () {
        $scope.controller.switchProposalViewModel.show(true);
    };

    $scope.openCriteriaBuilder = function () {
        $scope.controller.proposalView.clearValidation();

        if ($scope.controller.proposalView.isValidForCriteria()) {
            $scope.controller.criteriaBuilderViewModel.ShowModal(true);
        }
    };

    $scope.enableDistribute = ko.computed(function () {
        return $scope.hasPrograms() && !$scope.unsavedPrograms() && $scope.targetUnits() && $scope.targetUnits() > 0;
    });

    $scope.distributeSpots = function () {
        if ($scope.controller.proposalView.isValidForCriteria() && $scope.enableDistribute()) {
            var distributeSpotsRequest = {
                CacheGuid: $scope.cacheGuid(),
                TargetUnits: $scope.targetUnits()
            };

            $scope.controller.apiDistributeSpots(distributeSpotsRequest, function (proposalPrograms) {
                if (proposalPrograms) {
                    $scope.programs(proposalPrograms);
                    util.notify("Spots distributed");
                }
            });          
        }
    },

    $scope.openManageRatings = function () {
        $scope.controller.manageRatingsViewModel.showRatingsModal(true);
    },

    /*** FILTER ***/

    $scope.mustFilter = ko.observable(false);
    $scope.withSpots = ko.observable();
    $scope.spotFilter = ko.observable();
    $scope.spotFilter.subscribe(function (value) {
        var validOption = value != null && value.length > 0;
        if ((validOption || $scope.mustFilter()) && $scope.canUpdatePrograms()) {
            $scope.withSpots(value ? value == "with_spots" : null);
            $scope.controller.apiGetFilteredPrograms($scope.controller.proposalViewModel.getProposalForSave(), function (proposalPrograms) {
                if (proposalPrograms) {
                    $scope.controller.proposalViewModel.programs(proposalPrograms);
                }
            })
            $scope.mustFilter(true);
        }
    });

    $scope.openCriteriaFilter = function () {
        if ($scope.hasPrograms() || $scope.hasFiltersApplied()) {
            $scope.controller.filterViewModel.showModal(true);
        }
    };

    $scope.hasFiltersApplied = ko.computed(function () {
        return $scope.spotFilter() || ($scope.controller.filterViewModel && $scope.controller.filterViewModel.criteriaList().length > 0);
    });

    $scope.clearFilters = function() {
        $scope.spotFilter(null);

        if ($scope.controller.filterViewModel) {
            $scope.controller.filterViewModel.criteriaList([]);
        }
    };

    /*** ADDITIONAL INFO ***/

    $scope.sortByMarketRank = ko.observable(true);
    
    $scope.setSortByMarketRank = function (isRank) {
        $scope.sortByMarketRank(isRank);
    };

    $scope.toggleMarketSort = function (isRank, data, event) {
        //check is changed
        if (isRank !== $scope.sortByMarketRank()) {
        $scope.sortByMarketRank(!$scope.sortByMarketRank());
       $scope.controller.proposalView.sortMarket($scope.sortByMarketRank());
        }
        event.stopPropagation();
    };

    // additional demographic: updates program group on change; clears observable and hides grid columns on error
    //change api to use filter
    $scope.selectedAdditionalDemographicId = ko.observable();
    $scope.selectedAdditionalDemographicId.subscribe(function () {
        if ($scope.selectedAdditionalDemographicId()) {
            $scope.controller.apiGetFilteredPrograms($scope.getProposalForSave(),
                function (proposalPrograms) {
                    if (proposalPrograms) {
                        $scope.controller.proposalView.setResetState(false);
                        $scope.programs(proposalPrograms);
                        util.notify('Additional Demographic Changed');
                    }
                },

                function () {
                    $scope.selectedAdditionalDemographicId("");
                    $scope.controller.proposalView.displayAdditionalDemoColumns(false);
                });
        } else {
            $scope.controller.proposalView.displayAdditionalDemoColumns(false);
        }
    });

    $scope.additionalDemographic = ko.computed(function () {
        if ($scope.selectedAdditionalDemographicId() && $scope.selectedAdditionalDemographicId() != "None") {
            $scope.displayDemographicOptions(false);

            for (var i = 0; i < $scope.audiences().length; i++) {
                if ($scope.audiences()[i].Id == $scope.selectedAdditionalDemographicId())
                    return $scope.audiences()[i].Display;
            }
        }

        return 'None';
    });

    $scope.displayDemographicOptions = ko.observable(false);
    $scope.openDemographicOptions = function (data, event) {
        $scope.displayDemographicOptions(!$scope.displayDemographicOptions());
        event.stopPropagation();
    };
};