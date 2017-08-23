var SwitchProposalVersionView = BaseView.extend({
    controller: null,
    grid: null,

    initView: function (controller) {
        var $scope = this;

        $scope.controller = controller;
        $scope.grid = $('#switch_proposal_version_grid').w2grid(PlanningConfig.getSwitchProposalVersionGridCfg($scope));

        // handler - modal open
        $('#switch_proposal_version_modal').on('shown.bs.modal', function () {
            $scope.loadGrid.call($scope);
        });

        // handler - open button
        $('#open_btn').on('click', function () {
            var selection = $scope.grid.getSelection();
            if (selection.length == 0) {
                util.notify('Please, select one version', 'danger');
            } else {
                var item = $scope.grid.get(selection[0]);
                $scope.openVersion(item.Version);
            }
        });
    },

    loadGrid: function () {
        var $scope = this;

        $scope.grid.clear(false);
        var proposalId = $scope.controller.proposalViewModel.proposalId();

        $scope.controller.apiGetProposalVersions(proposalId, function (proposalVersions) {
            var proposals = proposalVersions.map(function (proposalVersion) {
                proposalVersion.recid = proposalVersion.Version;
                return proposalVersion;
            });

            $scope.grid.add(proposals);
        });
    },

    openVersion: function (version) {
        var $scope = this;

        var proposalId = $scope.controller.proposalViewModel.proposalId();

        $scope.controller.apiGetProposal(proposalId, version, function (proposal) {
            $scope.controller.proposalViewModel.resetState();
            $scope.controller.proposalViewModel.load(proposal);
            $scope.controller.switchProposalViewModel.show(false);
        });
    }
});