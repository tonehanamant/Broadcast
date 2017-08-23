function ManageRatingsViewModel(controller) {
    var $scope = this;

    $scope.controller = controller;

    /*** DISPLAY ***/

    $scope.showRatingsModal = ko.observable(false);

    $scope.open = function (detailId, shareBookId, hutBookId, playbackTypeId) {
        $scope.detailId(detailId),
        $scope.selectedShareBook(shareBookId);
        $scope.selectedHutBook(hutBookId);
        $scope.selectedPlaybackType(playbackTypeId);
        $scope.showRatingsModal(true);
        $scope.isReadOnly(controller.proposalViewModel.isReadOnly());
    };

    $scope.isReadOnly = ko.observable(false);

    /*** RATINGS ***/

    $scope.detailId = ko.observable();

    $scope.shareBooksOptions = ko.observableArray();
    $scope.selectedShareBook = ko.observable();

    $scope.hutBookOptions = ko.observableArray();
    $scope.selectedHutBook = ko.observable();

    $scope.playbackTypeOptions = ko.observableArray();
    $scope.selectedPlaybackType = ko.observable();

    $scope.openConfirmation = function () {
        $scope.showConfirmationModal(true);
    }

    /*** CONFIRMATION MODAL ***/

    $scope.showConfirmationModal = ko.observable(false);

    $scope.save = function () {
        var detailSet = $scope.controller.proposalView.getDetailSet($scope.detailId());
        detailSet.activeDetail.SharePostingBookId = $scope.selectedShareBook();
        detailSet.vm.SharePostingBookId($scope.selectedShareBook());

        detailSet.activeDetail.HutPostingBookId = $scope.selectedHutBook();
        detailSet.vm.HutPostingBookId($scope.selectedHutBook());

        detailSet.activeDetail.PlaybackType = $scope.selectedPlaybackType();
        detailSet.vm.PlaybackType($scope.selectedPlaybackType());

        $scope.showRatingsModal(false);
        $scope.showConfirmationModal(false);
    }
}