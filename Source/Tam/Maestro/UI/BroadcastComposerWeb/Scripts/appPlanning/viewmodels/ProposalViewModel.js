var viewModes = {
    create: "create",
    view: "view"
};

var MARKET_GROUP_ID_OFFSET = 10000;

//NEW VERSION - go through all and revise
//start with fresh slate most likely
//move modal and programs to view

var ProposalViewModel = function (controller) {
    var $scope = this;
    var selectedAudienceSubscribe = null;  //subcription reference
    var storedAudiences = [];  //stored audiences to reset on changes
    $scope.controller = controller;

    /*** DISPLAY ***/

    $scope.viewMode = ko.observable();
    $scope.viewMode.subscribe(function (viewMode) {
        if (viewMode == viewModes.create) {
            $scope.clear();
        } else {
            // $scope.canUpdatePrograms(true);
        }
    });

    /*** PROPOSAL ***/

    $scope.proposalId = ko.observable();
    $scope.version = ko.observable();
    $scope.primaryVersionId = ko.observable();

    /*** OPTIONS ***/

    $scope.statusOptions = ko.observableArray();
    $scope.status = ko.observable();
    $scope.isReadOnly = ko.observable(false);
    $scope.canDelete = ko.observable(true);
    $scope.advertisers = ko.observableArray();
    $scope.markets = ko.observableArray();
    $scope.audiences = ko.observableArray();
    $scope.secondaryAudiences = ko.observableArray();
    $scope.postTypes = ko.observableArray();
    $scope.spotLengths = ko.observableArray();
    $scope.equivalizedOptions = ko.observableArray([{ Display: 'Yes', Val: true }, { Display: 'No', Val: false }]);
    $scope.selectedMarket = ko.observable();
    $scope.forceSave = ko.observable(false);

    /*** FORM ***/

    // read only
    $scope.totalCpm = ko.observable();
    $scope.targetCpm = ko.observable();
    $scope.totalCPMPercent = ko.observable(0);
    $scope.totalCPMMarginAchieved = ko.observable(false);

    $scope.totalCost = ko.observable();
    $scope.targetBudget = ko.observable();
    $scope.totalCostPercent = ko.observable(0);
    $scope.totalCostMarginAchieved = ko.observable(false);

    $scope.totalImpressions = ko.observable();
    $scope.targetImpressions = ko.observable();
    $scope.totalImpressionsPercent = ko.observable(0);
    $scope.totalImpressionsMarginAchieved = ko.observable(false);

    $scope.targetUnits = ko.observable();
    $scope.spotLength = ko.observable();
    $scope.spotLengthDisplays = ko.computed(function () {
        if (_.isEmpty($scope.spotLength())) {
            return "-";
        }

        var result = $scope.spotLength().reduce(function (aggregated, spotLength) {
            if (spotLength.hasOwnProperty('Display')) {
                aggregated.push(spotLength.Display);
            }

            return aggregated;
        }, []);

        return result.join(",");
    });
    $scope.flightStartDate = ko.observable();
    $scope.flightEndDate = ko.observable();
    $scope.flightWeeks = ko.observable();
    $scope.flightIcon = ko.observable();
    $scope.formatDate = function (dateString) {
        if (!dateString)
            return "";

        dateString = dateString.replace(/-/g, '\/').replace(/T.+/, '');
        return moment(new Date(dateString)).format('MM/DD/YY');
    }
    $scope.flight = ko.computed(function () {
        // hiatus icon
        var hiatusWeeks = [];
        var tooltipText = '<div><span>Hiatus Weeks</span><br>';

        var html = '';
        $.each($scope.flightWeeks(), function (idx, week) {
            if (week.IsHiatus == true) {
                var hiatusWeekFormated = $scope.formatDate(week.StartDate) + ' - ' + $scope.formatDate(week.EndDate);
                tooltipText += '<div>' + hiatusWeekFormated + '</div>';
                hiatusWeeks.push(hiatusWeekFormated);
            }
        });

        if (hiatusWeeks.length > 0) {
            tooltipText += '</div>';
            var mrk = '<span class="glyphicon glyphicon-info-sign" style="" aria-hidden="false"></span>';
            html = '<span class="proposal-hiatus span-block" data-container="body" data-html="true" data-toggle="tooltip" title="' + tooltipText + '">' + mrk + '</span>';
        }

        $scope.flightIcon(html);

        // formatted date
        var startDate = $scope.flightStartDate() ? moment($scope.flightStartDate()).format('MM/DD/YYYY') : "";
        var endDate = $scope.flightEndDate() ? moment($scope.flightEndDate()).format('MM/DD/YYYY') : "";

        return startDate + " - " + endDate;
    });
    $scope.notes = ko.observable();

    // form inputs
    $scope.proposalName = ko.observable();
    $scope.selectedAdvertiser = ko.observable();
    $scope.selectedAudience = ko.observable();

    // TODO:  need to use copy of Audiences without the selected from there
    $scope.selectedSecondaryAudiences = ko.observableArray([]);
    $scope.selectedPostType = ko.observable();
    $scope.equivalized = ko.observable();

    // PROPOSAL DETAIL
    $scope.proposalDetails = ko.observableArray();

    // using version now?
    $scope.proposalTitle = ko.computed(function () {
        if ($scope.version()) {
            return $scope.proposalName() + " - Version: " + $scope.version();
        } else {
            return $scope.proposalName();
        }
    });

    $scope.isValid = function () {
        return $scope.controller.proposalView.isValid();
    };

    // DIRTY handling - needs save checks

    $scope.checkDirty = ko.observable(false);
    $scope.isDirty = ko.observable(false);

    $scope.setDirty = function (val) {
        if ($scope.checkDirty()) {
            //console.log('setDirty', $scope.checkDirty(), val);
            $scope.isDirty(true);
        }
    };

    // from view check if header needs save
    $scope.proposalHeaderDirty = function () {
        if ($scope.viewMode() == viewModes.create) return true;
        return $scope.checkDirty() ? $scope.isDirty() : false;
    };

    $scope.status.subscribe($scope.setDirty);
    $scope.proposalName.subscribe($scope.setDirty);
    $scope.selectedMarket.subscribe($scope.setDirty);
    $scope.selectedPostType.subscribe($scope.setDirty);
    $scope.equivalized.subscribe($scope.setDirty);
    $scope.selectedAdvertiser.subscribe($scope.setDirty);
    $scope.selectedAudience.subscribe($scope.setDirty);
    $scope.selectedSecondaryAudiences.subscribe($scope.setDirty);
    $scope.notes.subscribe($scope.setDirty);

    //set secondary based on the selected in Guaranteed (all but selected) - check the selected values in secondary to remove if conflict? (seems to remove automatically)
    //deal with stored value that contains all so can be reset
    $scope.setSecondaryAudiences = function (selectedId) {
        //set initially back to stored full version; then remove the selected
        var stored = util.copyArray(storedAudiences);
        $scope.secondaryAudiences(stored);
        $scope.secondaryAudiences.remove(function (item) { return item.Id == selectedId; });
        //var secondary = $scope.secondaryAudiences();
        //console.log('setSecondaryAudiences', selectedId, secondary, stored);

    };

    $scope.onShowModal = function (type) {
        //to change secondary when changes - only set after initial data - or will get extraneous changes (dispose on hide)
        selectedAudienceSubscribe = $scope.selectedAudience.subscribe(function (value) {
            if (value) {
                $scope.setSecondaryAudiences(value);
            }
        });

        // default values for new proposal
        if ($scope.viewMode() == viewModes.create) {
            $scope.selectedAdvertiser($scope.advertisers()[0].Id);

            var top100 = ko.utils.arrayFirst($scope.markets(), function (marketOption) {
                return marketOption.Id == 100;
            });
            $scope.selectedMarket(top100);

            $scope.selectedAudience(31);
            $scope.selectedPostType(1);
            $scope.equivalized(true);
            $scope.selectedSecondaryAudiences($scope.secondaryAudiences()[0].Id);
            $scope.status(1);
        }
    };

    $scope.onHideModal = function () {
        selectedAudienceSubscribe.dispose(); // remove subscription before change
        $scope.checkDirty(false);
        $scope.isDirty(false);
        $scope.clear();
        //remove detail - do from view as need to clear array there as well
        //$scope.removeAllProposalDetails();

    };

    //revise as needed; add default items needed
    $scope.loadOptions = function (options) {
        $scope.advertisers(options.Advertisers);
        $scope.audiences(options.Audiences);

        $scope.markets(_.cloneDeep(options.MarketGroups));

        // offset group Ids to prevent collision with single markets
        var customMarketGroups = options.MarketGroups.map(function (group) {
            group.Id += MARKET_GROUP_ID_OFFSET;
            return group;
        });

        var customMarketOptions = customMarketGroups.concat(options.Markets);
        $scope.controller.customMarketsViewModel.setOptions(customMarketOptions);

        $scope.audiences(_.sortBy(options.Audiences, ['Display']));

        //set initially then will be changed after Guaranteed Audience check
        $scope.secondaryAudiences(_.sortBy(options.Audiences, ['Display']));//todo
        storedAudiences = _.sortBy(options.Audiences, ['Display']);  //store sorted so can be reset on change
        $scope.postTypes(options.SchedulePostTypes);
        $scope.spotLengths(options.SpotLengths);

        // manage ratings options
        if (options.ForecastDefaults) {
            $scope.controller.manageRatingsViewModel.shareBooksOptions(options.ForecastDefaults.CrunchedMonths);
            $scope.controller.manageRatingsViewModel.hutBookOptions(options.ForecastDefaults.CrunchedMonths);
            $scope.controller.manageRatingsViewModel.playbackTypeOptions(options.ForecastDefaults.PlaybackTypes);
        }

        $scope.statusOptions(options.Statuses);
    };

    //load proposal either after save or loading existing
    $scope.load = function (proposal) {
        $scope.checkDirty(false);
        $scope.proposalId(proposal.Id);
        $scope.version(proposal.Version);
        $scope.primaryVersionId(proposal.PrimaryVersionId);
        $scope.proposalName(proposal.ProposalName);
        $scope.selectedAdvertiser(proposal.AdvertiserId);

        //load static field separately
        $scope.loadStaticFields(proposal);

        $scope.selectedAudience(proposal.GuaranteedDemoId);
        $scope.selectedSecondaryAudiences(proposal.SecondaryDemos);

        //sync the audiences
        $scope.setSecondaryAudiences($scope.selectedAudience());

        $scope.selectedPostType(proposal.PostType);
        $scope.equivalized(proposal.Equivalized);
        $scope.notes(proposal.Notes);

        if (!_.isEmpty(proposal.Markets) || proposal.BlackoutMarketGroupId) {
            $scope.controller.customMarketsViewModel.loadSelectors(proposal.Markets, proposal.MarketGroup, proposal.BlackoutMarketGroup);
            $scope.createCustomMarketOption();
        } else {
            $scope.selectedMarket(proposal.MarketGroup);
        }

        $scope.status(proposal.Status);

        if (proposal.Status != 4) {
            $scope.statusOptions.remove(function (status) {
                return status.Id == 4;
            });
        }

        $scope.isDirty(false);
        $scope.checkDirty(true);
        $scope.isReadOnly(proposal.Status == 3 || proposal.Status == 4);
        $scope.canDelete(proposal.CanDelete);
    };

    $scope.loadStaticFields = function (proposal) {
        $scope.totalCpm(proposal.TotalCPM);
        $scope.targetCpm(proposal.TargetCPM);
        $scope.totalCPMPercent(proposal.TotalCPMPercent);
        $scope.totalCPMMarginAchieved(proposal.TotalCPMMarginAchieved);

        $scope.totalCost(proposal.TotalCost);
        $scope.targetBudget(proposal.TargetBudget);
        $scope.totalCostPercent(proposal.TotalCostPercent);
        $scope.totalCostMarginAchieved(proposal.TotalCostMarginAchieved);

        $scope.totalImpressions(util.divideImpressions(proposal.TotalImpressions));
        $scope.targetImpressions(util.divideImpressions(proposal.TargetImpressions));
        $scope.totalImpressionsPercent(proposal.TotalImpressionsPercent);
        $scope.totalImpressionsMarginAchieved(proposal.TotalImpressionsMarginAchieved);

        $scope.targetUnits(proposal.TargetUnits);
        $scope.spotLength(proposal.SpotLengths);
        $scope.flightStartDate(proposal.FlightStartDate);
        $scope.flightEndDate(proposal.FlightEndDate);
        $scope.flightWeeks(proposal.FlightWeeks);
    };

    $scope.clear = function () {
        $scope.proposalId(null);
        $scope.version(null);
        $scope.primaryVersionId(null);
        $scope.proposalName(null);
        $scope.selectedAdvertiser(null);
        $scope.totalCost(null);
        $scope.targetBudget(null);
        $scope.totalCostPercent(0);
        $scope.totalCostMarginAchieved(false);
        $scope.totalImpressions(null);
        $scope.targetImpressions(null);
        $scope.totalImpressionsPercent(0);
        $scope.totalImpressionsMarginAchieved(false);
        $scope.targetUnits(null);
        $scope.flightStartDate(null);
        $scope.flightEndDate(null);
        $scope.flightWeeks(null);
        $scope.spotLength(null);
        $scope.selectedMarket(null);
        $scope.selectedAudience(null);
        $scope.notes(null);
        $scope.selectedPostType(null);
        $scope.selectedSecondaryAudiences([]);
        $scope.equivalized(null);
        $scope.status(null);
        $scope.forceSave(false);
        $scope.isReadOnly(false);
        $scope.canDelete(true);
        $scope.totalCpm(null);
        $scope.targetCpm(null);
        $scope.totalCPMPercent(0);
        $scope.totalCPMMarginAchieved(false);

        $scope.controller.customMarketsViewModel.clear();
    };

    $scope.getProposalForSave = function () {
        var proposalForSave = {
            Id: $scope.proposalId(),
            Version: $scope.version(),
            PrimaryVersionId: $scope.primaryVersionId(),
            ProposalName: $scope.proposalName(),
            AdvertiserId: $scope.selectedAdvertiser(),
            FlightStartDate: $scope.flightStartDate(),
            FlightEndDate: $scope.flightEndDate(),
            FlightWeeks: $scope.flightWeeks(),
            SpotLengths: $scope.spotLength(),
            GuaranteedDemoId: $scope.selectedAudience(),
            Notes: $scope.notes(),
            PostType: $scope.selectedPostType(),
            SecondaryDemos: $scope.selectedSecondaryAudiences(),
            Equivalized: $scope.equivalized(),

            TargetImpressions: $scope.targetImpressions() ? util.multiplyImpressions($scope.targetImpressions()) : 0,
            TargetBudget: $scope.targetBudget() ? $scope.targetBudget() : 0,
            TargetCPM: $scope.targetCpm() ? $scope.targetCpm() : 0,
            TargetUnits: $scope.targetUnits() ? $scope.targetUnits() : 0,

            Status: $scope.status(),
            ForceSave: $scope.forceSave()
        }

        $scope.getMarketsForSave(proposalForSave);

        return proposalForSave;
    };

    /*** MENU ***/

    $scope.save = function () {
        var proposalSaveFn = function () {
            if ($scope.isValid()) {
                var proposalRequest = $scope.getProposalForSave();
                proposalRequest.Details = $scope.controller.proposalView.getProposalDetailForSave();

                $scope.controller.apiSaveProposal(proposalRequest,
                    function (proposal) {
                        if (proposal.ValidationWarning && proposal.ValidationWarning.HasWarning) {
                            var warningMessage = proposal.ValidationWarning.Message;
                            if (!warningMessage)
                                warningMessage = "This action will force save the proposal. Do you confirm the changes?";
                            util.confirm('Warning', warningMessage, function () {
                                $scope.forceSave(true);
                                proposalSaveFn();
                            });
                        } else {
                            util.notify('Proposal saved successfully', 'success');
                            $scope.load(proposal);
                            $scope.viewMode(viewModes.view);
                            $scope.controller.apiGetLock($scope.proposalId());
                            $scope.controller.proposalView.onAfterProposalSave(proposal);
                            $scope.forceSave(false);
                        }
                    }
                );
            }
        }

        proposalSaveFn();
    };

    $scope.saveAsVersion = function () {
        if ($scope.isValid()) {
            var newVersionRequest = $scope.getProposalForSave();
            newVersionRequest.Version = null;
            newVersionRequest.Details = $scope.controller.proposalView.getProposalDetailForSave();

            $scope.controller.apiSaveProposal(newVersionRequest, function (proposal) {
                $scope.load(proposal);
                $scope.controller.versionCreatedOptionsViewModel.newVersion(proposal.Version);
                $scope.controller.versionCreatedOptionsViewModel.show(true);
                $scope.controller.proposalView.onAfterProposalSave(proposal);

            });
        }
    };

    // PROPOSAL DETAIL

    $scope.addProposalDetail = function (detail) {
        $scope.proposalDetails.push(detail);
    };

    $scope.removeProposalDetail = function (setVM) {
        var removed = $scope.proposalDetails.remove(setVM);
        return removed; //array
    };

    $scope.removeAllProposalDetails = function () {
        var all = $scope.proposalDetails.removeAll();
        return all; //array
    };

    // SWITCH VERSION

    $scope.switchVersion = function () {
        $scope.controller.switchProposalViewModel.show(true);
    };

    // GEN SCX

    $scope.generateScx = function () {
        var confirmed = function () {
            $scope.controller.apiGenerateScx($scope.proposalId());
        }

        util.confirm('Generate SCX'
                     , 'Operation will Produce SCX files for All open Market Inventory in each Proposal Detail.<br />Select Continue to proceed</br />Select Cancel to cancel'
                     , confirmed);
    };

    // CUSTOM MARKETS


    $scope.openCustomMarketModal = function () {
        $scope.controller.customMarketsViewModel.show(true);
    };

    $scope.selectMarket = function (data) {
        $scope.selectedMarket(data);
    };

    $scope.createCustomMarketOption = function () {
        var customOption = { Id: -1, Display: 'Custom', Count: $scope.controller.customMarketsViewModel.totalCounter() };

        $scope.markets.remove(function (option) {
            return option.Id == customOption.Id;
        });

        $scope.selectedMarket(customOption);
        $scope.markets.push(customOption);
    };

    $scope.deleteCustomMarketOption = function () {
        var top100 = ko.utils.arrayFirst($scope.markets(), function (marketOption) {
            return marketOption.Id == 100;
        });

        $scope.selectedMarket(top100);

        $scope.markets.remove(function (option) {
            return option.Id == -1;
        });
    };

    // prepares data for custom markets (or simple market group selection)
    $scope.getMarketsForSave = function (proposal) {
        var isCustom = $scope.selectedMarket().Id == -1;

        var allMarkets = isCustom ? $scope.controller.customMarketsViewModel.allMarkets() : null;
        var marketGroupId = $scope.selectedMarket().Id;
        var blackoutMarketGroupId = null;

        if (isCustom) {
            marketGroupId = null;
            blackoutMarketGroupId = null;

            if (!_.isEmpty($scope.controller.customMarketsViewModel.simpleMarketGroup())) {
                marketGroupId = $scope.controller.customMarketsViewModel.simpleMarketGroup().Id;
                marketGroupId -= MARKET_GROUP_ID_OFFSET;
            }

            if (!_.isEmpty($scope.controller.customMarketsViewModel.blackoutMarketGroup())) {
                blackoutMarketGroupId = $scope.controller.customMarketsViewModel.blackoutMarketGroup().Id;
                blackoutMarketGroupId -= MARKET_GROUP_ID_OFFSET;
            }
        }

        proposal.Markets = allMarkets;
        proposal.MarketGroupId = marketGroupId;
        proposal.BlackoutMarketGroupId = blackoutMarketGroupId;
    };

    $scope.unorder = function () {
        var msg = '<p>Operation will Archive contracted version of the proposal and create new version for editing.</p><p>Select <strong>Continue</strong> to complete.<p>Select <strong>Cancel</strong> to cancel</p>';
        var confirmUnorder = function () {
            $scope.controller.unorderProposal($scope.proposalId(), function (proposal) {
                util.notify('Proposal unordered successfully', 'success');
                $scope.load(proposal);
                $scope.controller.proposalView.onAfterProposalSave(proposal);
            });
        }

        util.confirm('Warning', msg, confirmUnorder, null, 'Continue', null);
    };

    $scope.getStatusDisplay = function () {
        var statusDisplay;
        switch ($scope.status()) {
            case 1:
                statusDisplay = "Proposed";
                break;

            case 2:
                statusDisplay = "Agency on Hold";
                break;

            case 3:
                statusDisplay = "Contracted";
                break;

            case 4:
                statusDisplay = "Previously Contracted";
                break;

            default:
                statusDisplay = "Unknown";
                break;
        }

        return statusDisplay;
    };

    //DELETE Proposal

    $scope.deleteProposal = function () {
        if ($scope.canDelete()) {
            var confirmed = function () {
                //console.log("Delete Proposal");
                $scope.controller.apiDeleteProposal($scope.proposalId());
            }

            util.confirm('Delete Proposal'
                         , 'Are you sure you wish to delete this proposal? </br>Any reserved inventory will be lost.'
                         , confirmed);
            

        }
    };


};