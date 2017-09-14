function CustomMarketsSelectorViewModel(title) {
    var $scope = this;

    $scope.title = ko.observable(title);

    // options: markets and selected Ids
    $scope.customMarketsOptions = ko.observableArray([]);
    $scope.customMarketIds = ko.observableArray([]);

    // results: group and list of markets
    $scope.customMarketGroup = ko.observable();
    $scope.customMarkets = ko.observableArray([]);

    $scope.customMarketIds.subscribe(function (marketIds) {
        if (!_.isEmpty(marketIds)) {
            // finds full market object for the given marketId -- ko.utils.arrayFirst used for IE support
            var marketObj = ko.utils.arrayFirst($scope.customMarketsOptions(), function (market) {
                return market.Id == marketIds[0];
            });

            // only a single group can be selected (in the beginning of the array)
            if (marketObj.hasOwnProperty('Count')) {
                var isFirstGroup = !_.isEmpty($scope.customMarkets()) && $scope.customMarkets()[0].hasOwnProperty('Count');
                if (isFirstGroup) {
                    $scope.customMarkets.shift();
                } 

                $scope.customMarkets.unshift(marketObj);
                $scope.customMarketGroup(marketObj);
            } else {
                var exists = ko.utils.arrayFirst($scope.customMarkets(), function (addedMarket) {
                    return addedMarket.Id == marketObj.Id;
                });

                if (!exists) {
                    $scope.customMarkets.push(marketObj);
                }
            }

            // clear input after adding new market or market group
            $scope.customMarketIds([]);
        }
    });

    $scope.counter = ko.computed(function () {
        if (_.isEmpty($scope.customMarkets())) {
            return 0;
        }

        return $scope.customMarkets().reduce(function (counter, market) {
            if (market.hasOwnProperty('Count')) {
                counter += market.Count;
            } else {
                counter++;
            }

            return counter;
        }, 0);
    });

    $scope.excludeMarket = function (market) {
        var excluded = $scope.customMarkets.remove(function (marketObj) {
            return marketObj.Id == market.Id;
        });

        if (!_.isEmpty(excluded) && excluded[0].hasOwnProperty('Count')) {
            $scope.customMarketGroup(null);
        }
    };

    $scope.excludeAllMarkets = function () {
        $scope.customMarketGroup(null);
        $scope.customMarkets([]);
    };

    $scope.clear = function () {
        $scope.customMarketsOptions([]);
        $scope.customMarkets([]);
        $scope.customMarketGroup(null);
        $scope.customMarketIds([]);
    };
}