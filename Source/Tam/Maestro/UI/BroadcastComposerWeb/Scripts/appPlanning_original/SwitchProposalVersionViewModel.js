function SwitchProposalVersionViewModel(controller) {
    var $scope = this;
    $scope.controller = controller;

    $scope.show = ko.observable(false);

    $scope.cancel = function () {
        $scope.show(false);
    };
}