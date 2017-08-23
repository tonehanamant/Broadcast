function FilterViewModel(controller) {
    var $scope = this;

    $scope.controller = controller;

    /*** DISPLAY ***/

    $scope.genresOptions = ko.observableArray();
    $scope.marketsOptions = ko.observableArray();
    $scope.affiliatesOptions = ko.observableArray();
    $scope.programNamesOptions = ko.observableArray();

    $scope.showModal = ko.observable(false);

    open: open = function () {
        $scope.tempCriteriaList($scope.criteriaList.slice(0));
    }

    $scope.hide = function () {
        $scope.tempCriteriaList($scope.tempCriteriaList().filter(function (criterion) {
            return criterion.IsValid();
        }));
    };

    /*** CRITERIA ***/

    $scope.criteriaList = ko.observableArray();
    $scope.tempCriteriaList = ko.observableArray();

    $scope.isValid = function () {
        for (var i = 0; i < $scope.tempCriteriaList().length; i++) {
            if (!$scope.tempCriteriaList()[i].IsValid()) {
                return false;
            }
        }

        return true;
    };

    $scope.addCriterion = function (selectedProperty) {
        var criterion = new Criterion();
        criterion.Property(selectedProperty);
        $scope.tempCriteriaList.push(criterion);
    };

    $scope.removeCriterion = function () {
        if ($scope.tempCriteriaList().length > 0) {
            var item = this;
            //timeout so popover close has time to reset (else will not close)
            setTimeout(function () {
                $scope.tempCriteriaList.remove(item);
            }, 200);
        }
    };

    $scope.apply = function () {
        if ($scope.isValid()) {
            $scope.criteriaList($scope.tempCriteriaList.slice(0));

            $scope.controller.apiGetFilteredPrograms($scope.controller.proposalViewModel.getProposalForSave(), function (proposalPrograms) {
                if (proposalPrograms) {
                    $scope.controller.proposalViewModel.programs(proposalPrograms);
                }

                $scope.showModal(false);
            });
        }
    };

    $scope.getCriteriaValuesByProperty = function (property) {
        var result = [];
        result = $scope.criteriaList().reduce(function (result, criteria) {
            if (criteria.Property() == property) {
                if (criteria.Property() == "Airing Time") {
                    result.push({
                            DayPartId: 0,
                            SearchCriteria: (criteria.Values())[0]
                        });
                } else {
                    var newValues = criteria.Values().filter(function(value) {
                        return result.indexOf(value) == -1;
                    });

                    result.push.apply(result, newValues);
                }
            }

            return result;
        }, []);

        return result;
    };

    /*** MENU ***/

    $scope.options = ko.observableArray(["Program Name", "Airing Time", "Genre", "Affiliation", "Market"]);
    $scope.selectedOption = ko.observable();
    $scope.selectedOption.subscribe(function (option) {
        if (option && option != 'none') {
            var criterion = new Criterion(option);
            $scope.tempCriteriaList.push(criterion);
            $scope.selectedOption(undefined);
        }
    });
}