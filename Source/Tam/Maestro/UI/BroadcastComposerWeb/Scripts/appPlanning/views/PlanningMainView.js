var PlanningMainView = BaseView.extend({

    controller: null,
    grid: null,

    initView: function (controller) {
        var $scope = this;

        $scope.controller = controller;
        $scope.grid = $('#proposal_grid').w2grid(PlanningConfig.getProposalsGridCfg($scope));

        $scope.loadGrid();
        
        // grid handlers - search
        $("#planning_view").on("click", "#proposal_grid_search_btn", $scope.gridSearch.bind($scope));
        $("#planning_view").on("click", "#proposal_grid_search_clear_btn", $scope.clearGridSearch.bind($scope));
        $("#planning_view").on("keypress", "#proposal_grid_search_input", function (e) {
            var key = e.which;
            if (key == 13) {
                $scope.gridSearch();
            }
        });

        // IE 10 - the disabled form is needed to prevent submitting other forms
        $("#search_form").submit(function (e) {
            e.preventDefault();
        });

        //handle create proposal click
        $("#create_proposal_btn").on('click', function () {
            $scope.controller.createProposal();
        });
    },

    loadGrid: function () {
        var $scope = this;
        //do after the load
       // $scope.grid.clear(false);

        $scope.controller.apiGetProposals(function (proposals) {
            $scope.setGrid(proposals);
        });
    },

    setGrid: function (proposals) {
        if (proposals && proposals.length) {
            proposals = proposals.map(function (proposal) {
                proposal.recid = proposal.Id;
                proposal.Advertiser = proposal.Advertiser.Display;

                switch (proposal.Status) {
                    case 1:
                        proposal.DisplayStatus = "Proposed";
                        break;

                    case 2:
                        proposal.DisplayStatus = "Agency on Hold";
                        break;

                    case 3:
                        proposal.DisplayStatus = "Contracted";
                        break;

                    case 4:
                        proposal.DisplayStatus = "Previously Contracted";
                        break;

                    default:
                        proposal.DisplayStatus = "Undefined";
                        break;
                }


                return proposal;
            });
        }

        this.grid.clear(false);
        this.grid.add(proposals);
        this.grid.localSort();
        this.grid.resize();

    },

    gridSearch: function (event) {
        var $scope = this;

        var val = $("#proposal_grid_search_input").val();
        if (val && val.length) {
            val = val.toLowerCase();
            var search = [{ field: 'Id', type: 'text', value: [val], operator: 'is' }, { field: 'ProposalName', type: 'text', value: [val], operator: 'contains' }, { field: 'Advertiser', type: 'text', value: [val], operator: 'contains' }, { field: 'Owner', type: 'text', value: [val], operator: 'contains' }, { field: 'DisplayStatus', type: 'text', value: [val], operator: 'contains' }];
            $scope.grid.search(search, 'OR');
            $("#proposal_grid_search_clear_btn").show();
        } else {
            $scope.clearGridSearch();
        }
    },

    clearGridSearch: function () {
        var $scope = this;

        $scope.grid.searchReset();
        $("#proposal_grid_search_input").val('');
        $("#proposal_grid_search_clear_btn").hide();
    }
});