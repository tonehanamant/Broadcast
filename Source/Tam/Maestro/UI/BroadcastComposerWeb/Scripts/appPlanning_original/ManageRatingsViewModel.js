function ManageRatingsViewModel(controller) {
    var $scope = this;

    $scope.controller = controller;

    /*** DISPLAY ***/

    $scope.showRatingsModal = ko.observable(false);

    $scope.open = function () {
        if (!_.isNil($scope.controller.proposalViewModel.shareBook())) {
            $scope.selectedShareBook($scope.controller.proposalViewModel.shareBook());
        }

        if (!_.isNil($scope.controller.proposalViewModel.hutBook())) {
            $scope.selectedHutBook($scope.controller.proposalViewModel.hutBook());
        }

        if (!_.isNil($scope.controller.proposalViewModel.playbackType())) {
            $scope.selectedPlaybackType($scope.controller.proposalViewModel.playbackType());
        }
    };

    /*** RATINGS ***/

    $scope.shareBooksOptions = ko.observableArray();
    $scope.selectedShareBook = ko.observable();

    $scope.hutBookOptions = ko.observableArray();
    $scope.selectedHutBook = ko.observable();

    $scope.playbackTypeOptions = ko.observableArray();
    $scope.selectedPlaybackType = ko.observable();

    $scope.canSave = ko.computed(function () {
        var shareBookValid = $scope.selectedShareBook() != null && $scope.selectedShareBook() != undefined;
        var hutBookValid = $scope.selectedHutBook() != null && $scope.selectedHutBook() != undefined;
        var playbackTypeValid = $scope.selectedPlaybackType() != null && $scope.selectedPlaybackType() != undefined;

        return shareBookValid || hutBookValid || playbackTypeValid;
    });

    $scope.openConfirmation = function () {
        if ($scope.canSave()) {
            $scope.showConfirmationModal(true);
        }
    }

    /*** CONFIRMATION MODAL ***/

    $scope.showConfirmationModal = ko.observable(false);

    $scope.save = function () {
        if ($scope.canSave()) {
            var shareBook = $scope.selectedShareBook();
            var hutBook = $scope.selectedHutBook();
            var playbackType = $scope.selectedPlaybackType();

            $scope.controller.proposalViewModel.shareBook(shareBook);
            $scope.controller.proposalViewModel.hutBook(hutBook);
            $scope.controller.proposalViewModel.playbackType(playbackType);

            $scope.controller.apiGetFilteredPrograms($scope.controller.proposalViewModel.getProposalForSave(),
                function (proposalPrograms) {
                    if (proposalPrograms) {
                        $scope.controller.proposalViewModel.programs(proposalPrograms);
                    }

                    $scope.showRatingsModal(false);
                    $scope.showConfirmationModal(false);
                    util.notify("Ratings updated");
                },

                function () {
                    // restore previous values on error
                    $scope.controller.proposalViewModel.shareBook(shareBook);
                    $scope.controller.proposalViewModel.hutBook(hutBook);
                    $scope.controller.proposalViewModel.playbackType(playbackType);
                });
        }
    }
}