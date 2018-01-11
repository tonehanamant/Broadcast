//NEW version - revise create/edit as needed

var PlanningController = BaseController.extend({
    view: null,
    viewModel: null,

    $InventoryView: null,
    $OpenMarketView: null,

    activeModalParams: null,  //in modal mode from open inventory/open markets - proposalId, detailId, modal, readOnly

    //proposalsController: null,

    //only load proposals (init) if not urlModalParams
    initController: function () {
        var $scope = this;
        var urlModalParams = $scope.checkOpenInventoryUrl();
        console.log('urlModalParams', urlModalParams);
        // view
        //$scope.view = new PlanningMainView();
        //$scope.view.initView($scope);

        // view model - nav search specific context
        $scope.viewModel = new PlanningSearchViewModel($scope);
        ko.applyBindings($scope.viewModel, document.getElementById("planning_nav_search_view"));

        //determine if open modal
        if (urlModalParams) {
            this.activeModalParams = urlModalParams;
            if (this.activeModalParams.modal == 'inventory') {
                this.openDetailInventory(this.activeModalParams);
            } else if (this.activeModalParams.modal == 'openMarket') {
                this.openDetailOpenMarketInventory(this.activeModalParams);
            }
        } else {
            // view
            $scope.view = new PlanningMainView();
            $scope.view.initView($scope);
        }

        
    },

    //check url for params - open modal mode
    checkOpenInventoryUrl: function () {
        //tbd if need proposalid?
        //associative array - look for proposalId, detaiId and modal, determine readOnly from params default false
        var ret = null;
        var params = util.getUrlVars();
        // console.log('checkOpenInventoryUrl', params);
        if (params['modal'] && params['proposalId'] && params['detailId']) {
            var readOnly = (params['readOnly'] && (params['readOnly'] == 'true')) ? true : false;
            ret = { modal: params['modal'], proposalId: parseInt(params['proposalId']), detailId: parseInt(params['detailId']), readOnly: readOnly };
        }
        return ret;
    },

    //CHANGE - open react view 
    // loads options -- if successful then opens the proposal form
    createProposal: function () {

        var url = baseUrl + 'broadcastreact/planning/proposal/create';
        window.location = url;
        //var $scope = this;

        //$scope.proposalsController.proposalViewModel.viewMode(viewModes.create);

        //$scope.proposalsController.apiGetOptions(function (options) {
        //    $scope.proposalsController.proposalViewModel.loadOptions(options);
        //    $scope.proposalsController.proposalView.showModal();
        //});
    },

    //CHANGE - open react view after locking - should we call lock always if going back from modal?
    // loads options everytime -- if successful then loads the proposal form
    editProposal: function (proposalId) {
        var $scope = this;
        //TEMP
        //util.alert('Edit Feature Not Currently Active', 'ID Selected: ' + proposalId);

        $scope.apiGetLock(proposalId, function (lockResponse) {
            if (lockResponse.Success) {
                var url = baseUrl + 'broadcastreact/planning/proposal/' + proposalId;
                window.location = url;
                //$scope.proposalsController.apiGetOptions(function(options) {
                //    $scope.proposalsController.proposalViewModel.loadOptions(options);

                //    $scope.apiGetPrimaryProposal(proposalId, function(proposal) {
                //        $scope.proposalsController.proposalViewModel.viewMode(viewModes.view);
                //        $scope.proposalsController.proposalViewModel.load(proposal);
                //        $scope.proposalsController.proposalView.setProposal(proposal);
                //        $scope.proposalsController.proposalView.showModal();
                //    });
                //}, proposalId);
            } else {
                util.notify("Proposal Locked", "danger");
                var msg = 'This Proposal is currently in use by ' + lockResponse.LockedUserName + '. Please try again later.';
                util.alert('Proposal Locked', msg);
            }
        });
    },

    /*** API ***/

    apiGetProposals: function (callback) {
        var url = baseUrl + 'api/Proposals/GetProposals';

        httpService.get(url,
            callback.bind(this),
            null,
            {
                $ViewElement: $('#planning_view'),
                ErrorMessage: 'Load Proposals',
                TitleErrorMessage: 'No Proposals Data Returned',
                StatusMessage: 'Load Proposals'
            });
    },

    /*

    apiGetPrimaryProposal: function (proposalId, callback) {
        if (!isNaN(proposalId)) {
            var $scope = this;
            var data = null,
                url = baseUrl + 'api/Proposals/Proposal/' + Number(proposalId);

            httpService.get(url,
                callback.bind(this),
                $scope.onApiPrimaryProposalError.bind($scope, proposalId),
                {
                    data: data,
                    $ViewElement: $('#planning_view'),
                    ErrorMessage: 'Find Proposal',
                    TitleErrorMessage: 'No Proposal Data Returned',
                    StatusMessage: 'Find Proposal'
                });
        }
    },

    //unlock if error either in gettin initialData (that is locked) or primary proposal
    onApiPrimaryProposalError: function (proposalId) {
        if (proposalId) this.proposalsController.apiGetUnlock(proposalId);
    }
    */


    //NEW - revised from ProposalController


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

    //moved here though inactive in BE/UI
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

    //redirect back to react proposal
    handleOpenInventoryError: function () {
        var $scope = this;
        setTimeout(function () {
            $scope.editProposal($scope.activeModalParams.proposalId);
        }, 3000);
    },

    //INVENTORY MODAL

    //from proposalView - open here

    //react version should check that was saved and warn read only befor this point
    openDetailInventory: function (params) {
        var $scope = this;
        $scope.$InventoryView = new ProposalDetailInventoryView();
        $scope.$InventoryView.initView($scope);
        var detailId = params.detailId;
        $scope.apiGetProposalInventory(detailId, function (inventory) {
            $scope.$InventoryView.setInventory(params, inventory);
        });
 
    },

    apiGetProposalInventory: function (detailId, callback, fromInventory) {
        var url = baseUrl + 'api/Inventory/Proprietary/Detail/' + detailId;
        
        httpService.get(url,
            callback.bind(this),
            fromInventory ? null : this.handleOpenInventoryError.bind(this),
            {
                $ViewElement: fromInventory ? $('#proposal_inventory_view') : $('#planning_view'),
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
            callback.bind(this),
            null,
            jsonObj,
            {
                $ViewElement: $('#proposal_inventory_view'),
                ErrorMessage: 'Proposal Inventory Totals',
                TitleErrorMessage: 'Proposal Inventory Totals',
                StatusMessage: 'Proposal Inventory Totals'
            });
    },

    //OPEN MARKET MODAL

    //from proposalView - open here
    //TODO change to open market versions
    //react version should check that was saved and warn read only befor this point
    openDetailOpenMarketInventory: function (params) {
        var $scope = this;
        $scope.$OpenMarketView = new ProposalDetailOpenMarketView();
        $scope.$OpenMarketView.initView($scope);
        var detailId = params.detailId;
        $scope.apiGetProposalOpenMarketInventory(detailId, function (inventory) {
            $scope.$OpenMarketView.loadInventory(params, inventory);
        });

    },

    apiGetProposalOpenMarketInventory: function (detailId, callback, fromInventory) {
        var url = baseUrl + 'api/Inventory/OpenMarket/Detail/' + detailId;

        httpService.get(url,
            callback.bind(this),
            fromInventory ? null : this.handleOpenInventoryError.bind(this),
            {
                $ViewElement: fromInventory ? $('#proposal_openmarket_view') : $('#planning_view'),
                ErrorMessage: 'Proposal Open Market Inventory',
                TitleErrorMessage: 'Proposal Open Market Inventory',
                StatusMessage: 'Proposal Open Market Inventory'
            });
    },

    apiPostOpenMarketRefine: function (data, callback, errorCallback) {
        var jsonObj = JSON.stringify(data);
        var url = baseUrl + 'api/Inventory/OpenMarket/Detail/' + data.ProposalDetailId + '/Refine';

        httpService.post(url,
            callback.bind(this),
            null,
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
            callback.bind(this),
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
        var jsonObj = JSON.stringify(params);
        var url = baseUrl + 'api/Inventory/OpenMarket';

        httpService.post(url,
            callback.bind(this),
            null,
            jsonObj,
            {
                $ViewElement: $('#proposal_openmarket_view'),
                ErrorMessage: 'Proposal Inventory Save',
                TitleErrorMessage: 'Proposal Inventory Save',
                StatusMessage: 'Proposal Inventory Save'
            });

    },

    //not used ?
    /*
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
    */

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
