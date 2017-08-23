function CriteriaBuilderViewModel(controller) {
    var $scope = this;

    $scope.Controller = controller;

    /*** DISPLAY ***/

    $scope.ShowModal = ko.observable(false);

    $scope.Genres = ko.observableArray();

    $scope.Open = function () {
        $scope.Controller.apiGetGenres(function (data) {
            $scope.Genres(data);

            /*** populate the criteria list ***/

            var programNameSearchCriteria = $scope.Controller.proposalViewModel.programNameSearchCriteria();    // ["Seinfeld", "Muppets"];
            var daypartSearchCriteria = $scope.Controller.proposalViewModel.daypartSearchCriteria();            // DaypartDto
            var genreSearchCriteria = $scope.Controller.proposalViewModel.genreSearchCriteria();                // [{ Id: 13 }, { Id: 14 }]; 

            if (programNameSearchCriteria) {
                programNameSearchCriteria.forEach(function (item) {
                    var criterion = new Criterion('Program Name', item.Contain, item.SearchCriteria);
                    $scope.CriteriaList.push(criterion);
                });
            }

            if (daypartSearchCriteria) {
                daypartSearchCriteria.forEach(function (item) {
                    var criterion = new Criterion('Day Part', item.Contain, [item.SearchCriteria]);
                    $scope.CriteriaList.push(criterion);
                });
            }

            if (genreSearchCriteria) {
                genreSearchCriteria.forEach(function (item) {
                    var selectedGenres = item.SearchCriteria.map(function (value) {
                        return {
                            Id: value
                        }
                    });

                    var criterion = new Criterion('Genre', item.Contain, selectedGenres);
                    $scope.CriteriaList.push(criterion);
                });
            }
        });
    };

    $scope.Hide = function () {
        $scope.CriteriaList.removeAll();
    };

    /*** CRITERIA ***/

    $scope.CriteriaList = ko.observableArray();

    $scope.IsValid = function () {
        for (var i = 0; i < $scope.CriteriaList().length; i++) {
            if (!$scope.CriteriaList()[i].IsValid()) {
                return false;
            }
        }

        return true;
    };

    $scope.AddCriterion = function (selectedProperty) {
        var criterion = new Criterion();
        criterion.Property(selectedProperty);
        $scope.CriteriaList.push(criterion);
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

    $scope.GetPrograms = function () {
        if ($scope.IsValid()) {
            var programNameSearchCriteria = [];
            var daypartSearchCriteria = [];
            var genreSearchCriteria = [];

            $scope.CriteriaList().map(function (criteria) {
                switch (criteria.Property()) {
                    case "Program Name":
                        programNameSearchCriteria.push({
                            Contain: criteria.Action(),
                            SearchCriteria: criteria.Values().split(",")
                        });

                        break;

                    case "Day Part":
                        daypartSearchCriteria.push({
                            DayPartId: 0,
                            Contain: criteria.Action(),
                            SearchCriteria: (criteria.Values())[0]
                        });

                        break;

                    case "Genre":
                        genreSearchCriteria.push({
                            Contain: criteria.Action(),
                            SearchCriteria: criteria.Values()
                        });

                        break;
                }
            });

            // update view model
            $scope.Controller.proposalViewModel.programNameSearchCriteria(programNameSearchCriteria);
            $scope.Controller.proposalViewModel.daypartSearchCriteria(daypartSearchCriteria);
            $scope.Controller.proposalViewModel.genreSearchCriteria(genreSearchCriteria);

            var proposalDto = $scope.Controller.proposalViewModel.getProposalForCriteriaFilter();
            $scope.Controller.apiGetPrograms(proposalDto, function (proposalDto) {
                if (proposalDto && proposalDto.ProposalPrograms) {
                    $scope.Controller.proposalViewModel.resetState();
                    $scope.Controller.proposalViewModel.unsavedPrograms(proposalDto.UnsavedPrograms);
                    $scope.Controller.proposalViewModel.programs(proposalDto.ProposalPrograms);

                    var filter = proposalDto.ProgramDisplayFilter;
                    $scope.Controller.filterViewModel.genresOptions(filter ? filter.Genres : null);
                    $scope.Controller.filterViewModel.marketsOptions(filter ? filter.Markets : null);
                    $scope.Controller.filterViewModel.affiliatesOptions(filter ? filter.Affiliations : null);
                    $scope.Controller.filterViewModel.programNamesOptions(filter ? filter.ProgramNames : null);
                }

                $scope.ShowModal(false);
            });
        }
    };

    /*** MENU ***/

    $scope.Options = ko.observableArray(['Program Name', 'Day Part', 'Genre']);
    $scope.SelectedOption = ko.observable();
    $scope.SelectedOption.subscribe(function (option) {
        if (option && option != 'none') {
            var criterion = new Criterion(option);
            $scope.CriteriaList.push(criterion);
            $scope.SelectedOption(undefined);
        }
    });
}