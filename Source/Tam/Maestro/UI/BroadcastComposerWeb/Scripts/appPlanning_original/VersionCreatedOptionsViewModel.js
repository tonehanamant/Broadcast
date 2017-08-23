function VersionCreatedOptionsViewModel(controller) {
    var $scope = this;

    $scope.controller = controller;
    $scope.proposalVM;
    $scope.newVersion = ko.observable();

    $scope.show = ko.observable(false);

    $scope.continueWork = function () {
        $scope.show(false);
    };

    $scope.closeProposal = function () {
        $scope.show(false);
        $scope.controller.proposalViewModel.showModal(false);
    };
}