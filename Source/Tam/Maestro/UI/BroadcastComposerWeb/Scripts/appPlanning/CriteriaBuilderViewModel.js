function Criterion(property, action, values) {
    var $scope = this;

    $scope.Property = ko.observable(property);
    $scope.Property.subscribe(function () {
        $scope.Action(null);
        $scope.Values([]);
    });

    $scope.Action = ko.observable(action);
    $scope.Values = ko.observableArray(values);

    $scope.IsValid = ko.computed(function () {
        if (!$scope.Values() || $scope.Values().length == 0) {
            return false;
        }

        return true;
    }, $scope);
};

function CriteriaBuilderViewModel(inventoryView) {
    var $scope = this;

    /*** PROPERTIES ***/

    $scope.Controller = inventoryView.ProposalView.controller;
    $scope.InventoryView = inventoryView;
    $scope.activeDetailId = null;

    $scope.LOSE_SPOTS_WARNING = 'New Criteria affects existing allocations. Select "Continue" to apply new criteria.';
    $scope.MIN_ENUM = ko.observable(1);
    $scope.MAX_ENUM = ko.observable(2);
    $scope.MinMaxOptions = [{ EnumValue: $scope.MIN_ENUM(), Display: "Min" }, { EnumValue: $scope.MAX_ENUM(), Display: "Max" }];

    $scope.INCLUDE_ENUM = ko.observable(1);
    $scope.EXCLUDE_ENUM = ko.observable(2);
    $scope.IncludeExcludeOptions = [{ EnumValue: $scope.INCLUDE_ENUM(), Display: "Includes" }, { EnumValue: $scope.EXCLUDE_ENUM(), Display: "Excludes" }];

    $scope.ShowModal = ko.observable(false);
    $scope.CriteriaList = ko.observableArray();
    $scope.IsProcessing = ko.observable(false);

    $scope.Genres = ko.observableArray();
    $scope.ProgramNamesOptions = ko.observableArray();
    $scope.Options = ko.observableArray(['Program Name', 'CPM', 'Genre']);
    $scope.SelectedOption = ko.observable();
    $scope.SelectedOption.subscribe(function (option) {
        if (option && option != 'none') {
            $scope.AddCriterion(option);
        }
    });

    /*** METHODS ***/

    $scope.Open = function () {
        $scope.activeDetailId = $scope.InventoryView.activeDetailSet ? $scope.InventoryView.activeDetailSet.activeDetail.Id : $scope.activeDetailId;

        $scope.Controller.apiGetGenres(function (genres) {
            $scope.Genres(genres);
            var criteria = $scope.InventoryView.activeInventoryData.Criteria;

            if (criteria) {
                /** Program Name Criteria **/
                if (!_.isEmpty(criteria.ProgramNameSearchCriteria)) {
                    var programNamesToInclude = criteria.ProgramNameSearchCriteria.reduce(function (result, program) {
                        if (program.Contain == $scope.INCLUDE_ENUM()) {
                            result.push(program.ProgramName);
                        }

                        return result;
                    }, []);

                    if (!_.isEmpty(programNamesToInclude)) {
                        var includeProgramCriterion = new Criterion('Program Name', $scope.INCLUDE_ENUM(), programNamesToInclude);
                        $scope.CriteriaList.push(includeProgramCriterion);
                    }

                    var programNamesToExclude = criteria.ProgramNameSearchCriteria.reduce(function (result, program) {
                        if (program.Contain == $scope.EXCLUDE_ENUM()) {
                            result.push(program.ProgramName);
                        }

                        return result;
                    }, []);

                    if (!_.isEmpty(programNamesToExclude)) {
                        var excludeProgramCriterion = new Criterion('Program Name', $scope.EXCLUDE_ENUM(), programNamesToExclude);
                        $scope.CriteriaList.push(excludeProgramCriterion);
                    }
                }

                /** CPM Criteria **/
                if (!_.isEmpty(criteria.CpmCriteria)) {
                    criteria.CpmCriteria.forEach(function (cpm) {
                        var cpmCriterion = new Criterion('CPM', cpm.MinMax, [cpm.Value]);
                        $scope.CriteriaList.push(cpmCriterion);
                    });

                    $('.cpm_money').w2field('money');
                }

                /** Genre Criteria **/
                if (!_.isEmpty(criteria.GenreSearchCriteria)) {
                    var genresToInclude = criteria.GenreSearchCriteria.reduce(function (result, genres) {
                        if (genres.Contain == $scope.INCLUDE_ENUM()) {
                            result.push({ Id: genres.GenreId });
                        }

                        return result;
                    }, []);

                    if (!_.isEmpty(genresToInclude)) {
                        var includeGenreCriterion = new Criterion('Genre', $scope.INCLUDE_ENUM(), genresToInclude);
                        $scope.CriteriaList.push(includeGenreCriterion);
                    }

                    var genresToExclude = criteria.GenreSearchCriteria.reduce(function (result, genres) {
                        if (genres.Contain == $scope.EXCLUDE_ENUM()) {
                            result.push({ Id: genres.GenreId });
                        }

                        return result;
                    }, []);

                    if (!_.isEmpty(genresToExclude)) {
                        var excludeGenreCriterion = new Criterion('Genre', $scope.EXCLUDE_ENUM(), genresToExclude);
                        $scope.CriteriaList.push(excludeGenreCriterion);
                    }
                }
            }
        });
    };

    $scope.Hide = function () {
        $scope.IsProcessing(false);
        $scope.CriteriaList.removeAll();
    };

    $scope.IsValid = function () {
        for (var i = 0; i < $scope.CriteriaList().length; i++) {
            var criterion = $scope.CriteriaList()[i];

            if (!criterion.IsValid()) {
                return false;
            }
        }

        return true;
    };

    $scope.AddCriterion = function (selectedProperty) {
        var canAdd = true;
        if (selectedProperty == "CPM") {
            var cpmCount = $scope.CriteriaList().reduce(function(count, criterion) {
                if (criterion.Property() == "CPM")
                    count++;

                return count;
            }, 0);

            if (cpmCount >= 2) {
                canAdd = false;
                $scope.SelectedOption(undefined);
                util.notify("You can only add two CPM criteria", "danger");
            }
        }

        if (canAdd) {
            var criterion = new Criterion(selectedProperty);
            $scope.CriteriaList.push(criterion);
            $scope.SelectedOption(undefined);
            $('.cpm_money').w2field('money');
        }
    };

    $scope.RemoveCriterion = function () {
        if ($scope.CriteriaList().length >= 1) {
            var item = this;
            //timeout so popover close has time to reset (else will not close)
            setTimeout(function () {
                $scope.CriteriaList.remove(item);
            }, 200);
        }
    };

    $scope.IsCpmValid = function () {
        var validCpm = true;
        var cpmMinCount = 0;
        var cpmMinValue;
        var cpmMaxCount = 0;
        var cpmMaxValue;

        $scope.CriteriaList().forEach(function (criterion) {
            if (criterion.Property() == "CPM") {
                if (criterion.Action() == $scope.MIN_ENUM()) {
                    cpmMinCount++;
                    cpmMinValue = Number(criterion.Values().replace(/[^0-9\.]+/g, ""));
                }

                if (criterion.Action() == $scope.MAX_ENUM()) {
                    cpmMaxCount++;
                    cpmMaxValue = Number(criterion.Values().replace(/[^0-9\.]+/g, ""));
                }

                if (cpmMinCount >= 2 || cpmMaxCount >= 2) {
                    util.notify("You can only add a single CPM 'max' and a single CPM 'min' criteria", "danger");
                    validCpm = false;
                }

                if (cpmMinValue && cpmMaxValue && (cpmMinValue >= cpmMaxValue)) {
                    util.notify("CPM min value must be lesser than CPM max value", "danger");
                    validCpm = false;
                }
            }
        });

        return validCpm;
    }

    $scope.Apply = function () {
        if ($scope.IsValid() && !$scope.IsProcessing()) {
            $scope.IsProcessing(true);

            var programNameSearchCriteria = [];
            var cpmSearchCriteria = [];
            var genreSearchCriteria = [];

            $scope.CriteriaList().map(function (criteria) {
                switch (criteria.Property()) {
                    case "Program Name":
                        criteria.Values().map(function (programName) {
                            programNameSearchCriteria.push({
                                Contain: criteria.Action(),
                                ProgramName: programName,
                                Id: null
                            });
                        });

                        break;

                    case "CPM":
                        cpmSearchCriteria.push({
                            MinMax: criteria.Action(),
                            Value: Number(criteria.Values().replace(/[^0-9\.]+/g, "")),
                            Id: null
                        });

                        break;

                    case "Genre":
                        criteria.Values().map(function (genreId) {
                            genreSearchCriteria.push({
                                Contain: criteria.Action(),
                                GenreId: genreId,
                                Id: null
                            });
                        });

                        break;
                }
            });

            var request = {
                ProposalDetailId: $scope.activeDetailId,
                Criteria: {
                    ProgramNameSearchCriteria: programNameSearchCriteria,
                    CpmCriteria: cpmSearchCriteria,
                    GenreSearchCriteria: genreSearchCriteria,
                },
                IgnoreExistingAllocation: false
            };

            $scope.processRefineResponse = function (response) {
                $scope.InventoryView.ProposalView.openMarketInventory = response;
                $scope.InventoryView.FilterVM.clearFilters();

                $scope.IsProcessing(false);
                util.notify('Criteria applied successfully', 'success');
                $scope.ShowModal(false);
                $scope.InventoryView.refreshInventory(response, true, true);
            };

            $scope.Controller.apiPostOpenMarketRefine(request,
                function (response) {
                    if (response.NewCriteriaAffectsExistingAllocations) {
                        util.confirm('Warning', $scope.LOSE_SPOTS_WARNING,
                            function () {
                                request.IgnoreExistingAllocation = true;
                                $scope.processRefineResponse(response);
                            },

                            function() {
                                $scope.IsProcessing(false);
                            });
                    } else {
                        $scope.processRefineResponse(response);
                    }
                },
                function() {
                    $scope.IsProcessing(false);
                });   
        }
    };
}