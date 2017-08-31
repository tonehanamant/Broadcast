//NEW VERSION - adjust as needed - no criteria, filter etc

var ProposalController = BaseController.extend({
    planningController: null,

    proposalView: null,
    proposalViewModel: null,

    switchProposalVersionView: null,
    switchProposalViewModel: null,
    versionCreatedOptionsViewModel: null,
    customMarketsViewModel: null,
    manageRatingsViewModel: null,

    initController: function (controller) {
        var $scope = this;

        $scope.planningController = controller;

        /*** SUB COMPONENTS ***/
        
        $scope.switchProposalVersionView = new SwitchProposalVersionView();
        $scope.switchProposalVersionView.initView($scope);

        $scope.switchProposalViewModel = new SwitchProposalVersionViewModel($scope);
        ko.applyBindings($scope.switchProposalViewModel, document.getElementById("switch_proposal_version_modal"));

        $scope.versionCreatedOptionsViewModel = new VersionCreatedOptionsViewModel($scope);
        ko.applyBindings($scope.versionCreatedOptionsViewModel, document.getElementById("version_created_options_modal"));

        $scope.customMarketsViewModel = new CustomMarketsViewModel($scope);
        ko.applyBindings($scope.customMarketsViewModel, document.getElementById("custom_markets_modal"));

        $scope.manageRatingsViewModel = new ManageRatingsViewModel($scope);
        ko.applyBindings($scope.manageRatingsViewModel, document.getElementById("manage_ratings"));

        /*** MAIN COMPONENTS ***/

        $scope.proposalView = new ProposalView();
        $scope.proposalView.initView($scope);

        $scope.proposalViewModel = new ProposalViewModel($scope);
        ko.applyBindings($scope.proposalViewModel, document.getElementById("proposal_modal"));
        //ko.applyBindings($scope.proposalViewModel, document.getElementById("proposal_update_warning_modal"));
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

    apiGenerateScx: function (proposalId) {
        var url = baseUrl + 'api/Proposals/generate_scx_archive/' + proposalId;
        window.location = url;
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

    apiSaveProposal: function (proposal, callback, errorCallback) {
        var jsonObj = JSON.stringify(proposal);
        var url = baseUrl + 'api/Proposals/SaveProposal';

        httpService.post(url,
            callback.bind(this),
            errorCallback ? errorCallback.bind(this) : null,
            jsonObj,
            {
                $ViewElement: $('#proposal_view'),
                ErrorMessage: 'Error Saving Planning Proposal',
                TitleErrorMessage: 'Planning Proposal Not Saved',
                StatusMessage: 'Planning Proposal Save'
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
    },

    unorderProposal: function (proposalId, callback, errorCallback) {
        var url = baseUrl + 'api/Proposals/UnorderProposal?proposalId=' + proposalId;

        httpService.post(url,
            callback.bind(this),
            errorCallback ? errorCallback.bind(this) : null,
            null,
            {
                $ViewElement: $('#proposal_view'),
                ErrorMessage: 'Error Unordering the Proposal',
                TitleErrorMessage: 'Error Unordering the Proposal',
                StatusMessage: 'Error Unordering the Proposal'
            });
    },

    //PROPOSAL DETAIL
    
    onProposalDetailFlightSelect: function (event, params, set) {
        this.apiGetProposalDetail(set.id, params);
    },

    apiGetProposalDetail: function (setId, params) {
        var jsonObj = JSON.stringify(params);
        var url = baseUrl + 'api/Proposals/GetProposalDetail';
        var $scope = this;
        httpService.post(url,
            $scope.onApiGetProposalDetail.bind($scope, setId),
            null,
            jsonObj,
            {
                $ViewElement: $('#proposal_view'),
                ErrorMessage: 'Get Proposal Detail',
                TitleErrorMessage: 'No Proposal Detail Data',
                StatusMessage: 'Get Proposal Detail'
            });
    },

    onApiGetProposalDetail: function (setId, detail) {
        this.proposalView.setDetailSetInitial(setId, detail);
    },

    //update detail based on set grid changes (quarter or week) - passed full set of details from all sets; returns full proposal
    apiProposalDetailUpdate: function (details, callback) {
        var jsonObj = JSON.stringify(details);
        var url = baseUrl + 'api/Proposals/UpdateProposal';
        httpService.post(url,
            callback.bind(this),
            null,
            jsonObj,
            {
                $ViewElement: $('#proposal_view'),
                ErrorMessage: 'Update Proposal Detail',
                TitleErrorMessage: 'No Update Proposal Detail Data',
                StatusMessage: 'Update Proposal Detail'
            });
    },
   
    apiGetProposalInventory: function(detailId, callback, fromInventory) {
        //var url = baseUrl + 'api/Inventory/Detail/' + detailId;
        var url = baseUrl + 'api/Inventory/Proprietary/Detail/' + detailId;
        //if fromInventory - change view el context
        httpService.get(url,
            callback ? callback.bind(this) : null,
            null,
            {
                $ViewElement: fromInventory ? $('#proposal_inventory_view') : $('#proposal_view'),
                ErrorMessage: 'Proposal Inventory',
                TitleErrorMessage: 'Proposal Inventory',
                StatusMessage: 'Proposal Inventory'
            });
    },

    apiSaveInventoryProprietary: function (params, callback) {
        //var url = baseUrl + 'api/Inventory/';
        var url = baseUrl + 'api/Inventory/Proprietary';
        var jsonObj = JSON.stringify(params);
        httpService.post(url,
            callback ? callback.bind(this) : null,
            null,
            jsonObj,
            {
                $ViewElement: $('#proposal_inventory_view'),
                ErrorMessage: 'Proposal Inventory Save',
                TitleErrorMessage: 'Proposal Inventory Save',
                StatusMessage: 'Proposal Inventory Save'
            });

    },

    apiPostDetailTotals: function (data, callback) {
        var jsonObj = JSON.stringify(data);
        var url = baseUrl + 'api/Inventory/Detail/Totals';

        httpService.post(url,
            callback ? callback.bind(this) : null,
            null,
            jsonObj,
            {
                $ViewElement: $('#proposal_inventory_view'),
                ErrorMessage: 'Proposal Inventory Totals',
                TitleErrorMessage: 'Proposal Inventory Totals',
                StatusMessage: 'Proposal Inventory Totals'
            });
    },

    //OPEN MARKET

    apiGetProposalOpenMarketInventory: function (detailId, callback, fromInventory) {
        var url = baseUrl + 'api/Inventory/OpenMarket/Detail/' + detailId;

        httpService.get(url,
            callback ? callback.bind(this) : null,
            null,
            {
                $ViewElement: fromInventory ? $('#proposal_openmarket_view') : $('#proposal_view'),
                ErrorMessage: 'Proposal Open Market Inventory',
                TitleErrorMessage: 'Proposal Open Market Inventory',
                StatusMessage: 'Proposal Open Market Inventory'
            });
    },

    apiPostOpenMarketRefine: function (data, callback, errorCallback) {
        var jsonObj = JSON.stringify(data);
        var url = baseUrl + 'api/Inventory/OpenMarket/Detail/' + data.ProposalDetailId + '/Refine';

        httpService.post(url,
            callback ? callback.bind(this) : null,
            errorCallback ? errorCallback.bind(this) : null,
            jsonObj,
            {
                $ViewElement: $('#criteria_builder_view'),
                ErrorMessage: 'Refine Programs',
                TitleErrorMessage: 'Refine Programs',
                StatusMessage: 'Refine Programs'
            });
    },

    apiUpdateInventoryOpenMarketTotals: function (data, callback) {
        var jsonObj = JSON.stringify(data);
        var url = baseUrl + 'api/Inventory/OpenMarket/UpdateTotals';

        httpService.post(url,
            callback ? callback.bind(this) : null,
            null,
            jsonObj,
            {
                $ViewElement: $('#proposal_openmarket_view'),
                ErrorMessage: 'Proposal Inventory Totals',
                TitleErrorMessage: 'Proposal Inventory Totals',
                StatusMessage: 'Proposal Inventory Totals'
            });
    },

    apiSaveInventoryOpenMarket: function (params, callback) {
        var url = baseUrl + 'api/Inventory/OpenMarket';
        var jsonObj = JSON.stringify(params);
        httpService.post(url,
            callback ? callback.bind(this) : null,
            null,
            jsonObj,
            {
                $ViewElement: $('#proposal_openmarket_view'),
                ErrorMessage: 'Proposal Inventory Save',
                TitleErrorMessage: 'Proposal Inventory Save',
                StatusMessage: 'Proposal Inventory Save'
            });

    },

    apiUpdateOpenMarketInventoryTotals: function (inventory, callback, errorCallback) {
        var jsonObj = JSON.stringify(inventory);
        var url = baseUrl + 'api/Inventory/OpenMarket/UpdateTotals';

        httpService.post(url,
            callback.bind(this),
            errorCallback ? errorCallback.bind(this) : null,
            jsonObj,
            {
                $ViewElement: $('#proposal_openmarket_view'),
                ErrorMessage: 'Update Open Market Inventory Totals',
                TitleErrorMessage: 'Update Open Market Inventory Totals',
                StatusMessage: 'Update Open Market Inventory Totals'
            });
    },

    apiApplyOpenMarketInventoryFilter: function (inventory, callback, errorCallback) {
        var jsonObj = JSON.stringify(inventory);
        var url = baseUrl + 'api/Inventory/OpenMarket/ApplyFilter';

        httpService.post(url,
            callback.bind(this),
            errorCallback ? errorCallback.bind(this) : null,
            jsonObj,
            {
                $ViewElement: $('#proposal_openmarket_view'),
                ErrorMessage: 'Update Open Market Inventory Totals',
                TitleErrorMessage: 'Update Open Market Inventory Totals',
                StatusMessage: 'Update Open Market Inventory Totals'
            });
    }
});