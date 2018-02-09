//REVISED version for open from React - remove ProposalView dependencies

function FilterViewModel(inventoryView) {
    var $scope = this;

    $scope.inventoryView = inventoryView;
    $scope.controller = inventoryView.PlanningController;

    /*** DISPLAY ***/

    $scope.genresOptions = ko.observableArray();
    $scope.marketsOptions = ko.observableArray();
    $scope.affiliatesOptions = ko.observableArray();
    $scope.programNamesOptions = ko.observableArray();
    $scope.formattedFilterCriteria = ko.observable();

    $scope.showModal = ko.observable(false);

    open: open = function () {
        $scope.tempCriteriaList($scope.criteriaList.slice(0));

        $scope.controller.apiGetGenres(function (genres) {
            $scope.genresOptions(genres);
        });
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
            var filterCriteria = {
                ProgramNames: $scope.getCriteriaValuesByProperty("Program Name"),
                Genres: $scope.getCriteriaValuesByProperty("Genre"),
                DayParts: $scope.getCriteriaValuesByProperty("Airing Time"),
                Affiliations: $scope.getCriteriaValuesByProperty("Affiliation"),
                Markets: $scope.getCriteriaValuesByProperty("Market")
            };

            $scope.formattedFilterCriteria(filterCriteria);
            $scope.inventoryView.applyFilter();
        }
    };

    $scope.getCriteriaValuesByProperty = function (property) {
        var result = [];
        result = $scope.criteriaList().reduce(function (result, criteria) {
            if (criteria.Property() == property) {
                if (criteria.Property() == "Airing Time") {
                    result.push((criteria.Values())[0]);
                } else {
                    var newValues = criteria.Values().filter(function (value) {
                        return result.indexOf(value) == -1;
                    });

                    result.push.apply(result, newValues);
                }
            }

            return result;
        }, []);

        return result;
    };

    $scope.setAvailableValues = function (filterValues) {
        $scope.marketsOptions(filterValues.Markets);
        $scope.affiliatesOptions(filterValues.Affiliations);
        $scope.programNamesOptions(filterValues.ProgramNames);
    }

    $scope.clearFilters = function () {
        $scope.tempCriteriaList([]);
        $scope.criteriaList([]);
        $scope.inventoryView.OpenMarketVM.hasFiltersApplied(false);
    }

    /*** MENU ***/

    $scope.options = ko.observableArray(["Program Name", "Airing Time", "Affiliation", "Market"]);
    $scope.selectedOption = ko.observable();
    $scope.selectedOption.subscribe(function (option) {
        if (option && option != 'none') {
            var criterion = new Criterion(option);
            $scope.tempCriteriaList.push(criterion);
            $scope.selectedOption(undefined);
        }
    });
}