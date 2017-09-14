//changed context to Navigation Bar - search proposal specific
var PlanningSearchViewModel = function (controller) {
    var $scope = this;

    $scope.controller = controller;

    $scope.proposalSearchId = ko.observable();

    $scope.searchProposal = function () {
        if ($scope.proposalSearchId()) {
            $scope.controller.editProposal($scope.proposalSearchId());
        }
    };

    $scope.proposalSearchKeypress = function (d, e) {
        if (e.keyCode == 13) {
            e.preventDefault();//prevent reload
            $scope.searchProposal();
        }

        return true;
    };

};