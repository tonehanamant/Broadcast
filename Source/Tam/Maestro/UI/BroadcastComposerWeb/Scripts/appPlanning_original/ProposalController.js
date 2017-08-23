/*** used by CriteriaBuilderViewModel and FilterViewModel ***/
function Criterion(property, action, values) {
    var $scope = this;

    $scope.Property = ko.observable(property);
    $scope.Property.subscribe(function () {
        $scope.Action(null);
        $scope.Values([]);
    });

    $scope.Action = ko.observable(action);
    $scope.Values = ko.observableArray(values);

    $scope.IsValid = ko.computed(function () {
        if (!$scope.Values() || $scope.Values().length == 0) {
            return false;
        }

        return true;
    }, $scope);
};

var ProposalController = BaseController.extend({
    planningController: null,

    proposalView: null,
    proposalViewModel: null,

    switchProposalVersionView: null,
    switchProposalViewModel: null,
    versionCreatedOptionsViewModel: null,
    criteriaBuilderViewModel: null,
    filterViewModel: null,
    manageRatingsViewModel: null,

    initController: function (controller) {
        var $scope = this;

        $scope.planningController = controller;

        /*** MAIN COMPONENTS ***/

        $scope.proposalView = new ProposalView();
        $scope.proposalView.initView($scope);

        $scope.proposalViewModel = new ProposalViewModel($scope);
        ko.applyBindings($scope.proposalViewModel, document.getElementById("proposal_modal"));

        /*** SUB COMPONENTS ***/

        $scope.switchProposalVersionView = new SwitchProposalVersionView();
        $scope.switchProposalVersionView.initView($scope);

        $scope.switchProposalViewModel = new SwitchProposalVersionViewModel($scope);
        ko.applyBindings($scope.switchProposalViewModel, document.getElementById("switch_proposal_version_modal"));

        $scope.versionCreatedOptionsViewModel = new VersionCreatedOptionsViewModel($scope);
        ko.applyBindings($scope.versionCreatedOptionsViewModel, document.getElementById("version_created_options_modal"));

        $scope.criteriaBuilderViewModel = new CriteriaBuilderViewModel($scope);
        ko.applyBindings($scope.criteriaBuilderViewModel, document.getElementById("criteria_builder_modal"));

        $scope.filterViewModel = new FilterViewModel($scope);
        ko.applyBindings($scope.filterViewModel, document.getElementById("filter_modal"));

        $scope.manageRatingsViewModel = new ManageRatingsViewModel($scope);
        ko.applyBindings($scope.manageRatingsViewModel, document.getElementById("manage_ratings"));
    },

    /*** API ***/

    apiGetOptions: function(callback, proposalId) {
        var url = baseUrl + 'api/Proposals/InitialData';
        var $scope = this;
        httpService.get(url,
           callback.bind(this),
           //error if from edit proposalId - unlock
           $scope.planningController.onApiPrimaryProposalError.bind($scope.planningController, proposalId),
            {
                $ViewElement: $('#planning_view'),
                ErrorMessage: 'Load Proposals Initial Data Error',
                TitleErrorMessage: 'No Proposals Inital Data Returned',
                StatusMessage: 'Load Proposals Initial Data'
            });
    },

    apiGetProposalVersions: function (proposalId, callback) {
        var url = baseUrl + 'api/Proposals/Proposal/' + proposalId + '/Versions';

        httpService.get(url,
            callback.bind(this),
            null,
            {
                $ViewElement: $('#proposal_view'),
                ErrorMessage: 'Error Getting Proposal Versions',
                TitleErrorMessage: 'Proposal Versions',
                StatusMessage: 'Proposal Versions'
            });
    },

    apiGetProposal: function (proposalId, version, callback) {
        version = version || 1;
        var url = baseUrl + 'api/Proposals/Proposal/' + proposalId + '/Versions/' + version;

        httpService.get(url,
            callback.bind(this),
            null,
            {
                $ViewElement: $('#proposal_view'),
                ErrorMessage: 'Error Getting Proposal Version',
                TitleErrorMessage: 'Get Proposal Version',
                StatusMessage: 'Get Proposal Version'
            });
    },

    apiSaveProposal: function (proposal, callback) {
        var jsonObj = JSON.stringify(proposal);
        var url = baseUrl + 'api/Proposals/SaveProposal';

        httpService.post(url,
            callback.bind(this),
            null,
            jsonObj,
            {
                $ViewElement: $('#proposal_view'),
                ErrorMessage: 'Error Saving New Planning Proposal',
                TitleErrorMessage: 'Planning Proposal Not Saved',
                StatusMessage: 'Planning Proposal Saved'
            });
    },

    apiGetGenres: function (callback) {
        var url = baseUrl + "api/RatesManager/Genres";

        httpService.get(url,
            callback.bind(this),
            null,
            {
                $ViewElement: $('#criteria_builder_view'),
                ErrorMessage: 'Get Genres Data Error',
                TitleErrorMessage: 'Get Genres',
                StatusMessage: 'Get Genres'
            });
    },

    apiGetPrograms: function (proposal, callback, errorCallback) {
        var jsonObj = JSON.stringify(proposal);
        var url = baseUrl + 'api/Proposals/GetStationProgramsByCriteria';

        httpService.post(url,
            callback.bind(this),
            errorCallback ? errorCallback.bind(this) : null,
            jsonObj,
            {
                $ViewElement: $('#proposal_view'),
                ErrorMessage: 'Error Manage Programs Criteria',
                TitleErrorMessage: 'Error Manage Programs',
                StatusMessage: 'Manage Programs'
            });
    },

    apiGetNewProposalGuid: function (callback) {
        var url = baseUrl + "api/Proposals/New";

        httpService.get(url,
            callback.bind(this),
            null,
            {
                $ViewElement: $('#planning_view'),
                ErrorMessage: 'Error Getting Proposal GUID',
                TitleErrorMessage: 'Error Proposal GUID',
                StatusMessage: 'Get Proposal GUID'
            });
    },

    apiEditProposalProgramSpot: function (id, spot, successCallback, errorCallback) {
        var params = { CacheGuid: this.proposalViewModel.cacheGuid(), ProposalProgramId: id, Spots: spot };
        var jsonObj = JSON.stringify(params);
        var url = baseUrl + 'api/Proposals/EditProposalProgramSpot';

        httpService.post(url,
            successCallback.bind(this),
            errorCallback.bind(this),
            jsonObj,
            {
                $ViewElement: $('#proposal_view'),
                ErrorMessage: 'Error Editing Program Spot',
                TitleErrorMessage: 'Edit Error',
                StatusMessage: 'Edit Program Spot'
            });

    },

    apiDistributeSpots: function (proposal, callback, errorCallback) {
        var jsonObj = JSON.stringify(proposal);
        var url = baseUrl + 'api/Proposals/DistributeSpots';

        httpService.post(url,
            callback.bind(this),
            errorCallback ? errorCallback.bind(this) : null,
            jsonObj,
            {
                $ViewElement: $('#proposal_view'),
                ErrorMessage: 'Error Distributing Programs',
                TitleErrorMessage: 'Error Distributing',
                StatusMessage: 'Distribute Programs'
            });
    },

    apiGetFilteredPrograms: function(proposal, callback, errorCallback) {
        var jsonObj = JSON.stringify(proposal);
        var url = baseUrl + 'api/Proposals/GetProgramsByFilter';

        httpService.post(url,
            callback.bind(this),
            errorCallback ? errorCallback.bind(this) : null,
            jsonObj,
            {
                $ViewElement: $('#proposal_view'),
                ErrorMessage: 'Error Filtering Programs',
                TitleErrorMessage: 'Error Filtering',
                StatusMessage: 'Filter Programs'
            });
    },

    apiGetLock: function (proposalId, callback) {
        var url = baseUrl + 'api/Proposals/Proposal/' + proposalId + '/Lock';

        httpService.get(url,
            callback ? callback.bind(this) : null,
            null,
            {
                $ViewElement: $('#planning_view'),
                ErrorMessage: 'Proposal Lock',
                TitleErrorMessage: 'Proposal Lock',
                StatusMessage: 'Proposal Lock'
            });
    },

    apiGetUnlock: function (proposalId, callback) {
        var url = baseUrl + 'api/Proposals/Proposal/' + proposalId + '/UnLock';

        httpService.get(url,
            callback ? callback.bind(this) : null,
            null,
            {
                $ViewElement: $('#planning_view'),
                ErrorMessage: 'Proposal Unlock',
                TitleErrorMessage: 'Proposal Unlock',
                StatusMessage: 'Proposal Unlock'
            });
    }
});