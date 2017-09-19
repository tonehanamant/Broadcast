//NEW version - revise create/edit as needed

var PlanningMainController = BaseController.extend({
    view: null,
    viewModel: null,

    proposalsController: null,

    initController: function () {
        var $scope = this;

        // view
        $scope.view = new PlanningMainView();
        $scope.view.initView($scope);

        // view model - nav search specific context
        $scope.viewModel = new PlanningSearchViewModel($scope);
        ko.applyBindings($scope.viewModel, document.getElementById("planning_nav_search_view"));

        // child controller(s)
        $scope.proposalsController = new ProposalController();
        $scope.proposalsController.initController($scope);
    },

    // loads options -- if successful then opens the proposal form
    createProposal: function () {
        var $scope = this;

        $scope.proposalsController.proposalViewModel.viewMode(viewModes.create);

        $scope.proposalsController.apiGetOptions(function (options) {
            $scope.proposalsController.proposalViewModel.loadOptions(options);
            $scope.proposalsController.proposalView.showModal();
        });
    },

    // loads options everytime -- if successful then loads the proposal form
    editProposal: function (proposalId) {
        var $scope = this;
        //TEMP
        //util.alert('Edit Feature Not Currently Active', 'ID Selected: ' + proposalId);

        $scope.proposalsController.apiGetLock(proposalId, function (lockResponse) {
            if (lockResponse.Success) {
                $scope.proposalsController.apiGetOptions(function(options) {
                    $scope.proposalsController.proposalViewModel.loadOptions(options);

                    $scope.apiGetPrimaryProposal(proposalId, function(proposal) {
                        $scope.proposalsController.proposalViewModel.viewMode(viewModes.view);
                        $scope.proposalsController.proposalViewModel.load(proposal);
                        $scope.proposalsController.proposalView.setProposal(proposal);
                        $scope.proposalsController.proposalView.showModal();
                    });
                }, proposalId);
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

    apiGetPrimaryProposal: function (proposalId, callback) {
        if (!isNaN(proposalId)) {
            var $scope = this;
            var data = null,
                url = baseUrl + 'api/Proposals/Proposal/' + Number(proposalId);

            httpService.get(url,
                function (proposal) {
                    if (callback) {
                        callback($scope.convertFromProposalImpressions(proposal));
                    }
                },
                //error - unlock
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
    },

    /*** HELPERS ***/

    convertFromProposalImpressions: function(proposal) {
        proposal.TargetImpressions = proposal.TargetImpressions / 1000;
        if (proposal.TotalImpressions) {
            proposal.TotalImpressions = proposal.TotalImpressions / 1000;
        }

        return proposal;
    }
});
