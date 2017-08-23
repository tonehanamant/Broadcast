function CustomMarketsViewModel(controller) {
    var $scope = this;
    $scope.controller = controller;

    // SELECTORS
    // ----------------------------------------------------

    $scope.simpleMarketSelector =  ko.observable(new CustomMarketsSelectorViewModel("Markets"));

    $scope.blackoutMarketSelector = ko.observable(new CustomMarketsSelectorViewModel("Blackout Markets"));

    $scope.setOptions = function(options) {
        $scope.simpleMarketSelector().customMarketsOptions(options);
        $scope.blackoutMarketSelector().customMarketsOptions(options);
    };

    $scope.loadSelectors = function (allMarkets, simpleMarketGroup, blackoutMarketGroup) {
        var simpleMarkets = allMarkets.filter(function (market) {
            return !market.IsBlackout;
        });

        var blackoutMarkets = allMarkets.filter(function (market) {
            return market.IsBlackout;
        });

        // market group + list of markets
        if (!_.isEmpty(simpleMarketGroup)) {
            simpleMarkets.unshift(simpleMarketGroup);
        }
        $scope.simpleMarketSelector().customMarkets(simpleMarkets);

        if (!_.isEmpty(blackoutMarketGroup)) {
            blackoutMarkets.unshift(blackoutMarketGroup);
        }
        $scope.blackoutMarketSelector().customMarkets(blackoutMarkets);
    };

    // CUSTOM MARKET MANAGER
    // ----------------------------------------------------

    $scope.show = ko.observable(false);
    $scope.originalCustomMarketGroup = null;
    $scope.originalSelectedCustomMarkets = [];
    $scope.originalBlackoutMarketGroup = null;
    $scope.originalSelectedBlackoutMarkets = [];
    
    $scope.totalCounter = ko.computed(function () {
        return $scope.simpleMarketSelector().counter() + $scope.blackoutMarketSelector().counter();
    });

    $scope.simpleMarketGroup = ko.computed(function() {
        return $scope.simpleMarketSelector().customMarketGroup();
    });

    $scope.simpleMarkets = ko.computed(function () {
        var markets = _.cloneDeep($scope.simpleMarketSelector().customMarkets()).filter(function (market) {
            market.IsBlackout = false;
            return !market.hasOwnProperty('Count');
        });

        return markets;
    });

    $scope.blackoutMarketGroup = ko.computed(function () {
        return $scope.blackoutMarketSelector().customMarketGroup();
    });

    $scope.blackoutMarkets = ko.computed(function () {
        var markets = _.cloneDeep($scope.blackoutMarketSelector().customMarkets()).filter(function (market) {
            market.IsBlackout = true;
            return !market.hasOwnProperty('Count');
        });

        return markets;
    });

    // list of 'simple markets' + 'blackout markets' (without market groups)
    $scope.allMarkets = ko.computed(function () {
        var allMarkets = [];
        allMarkets.push.apply(allMarkets, $scope.simpleMarkets());
        allMarkets.push.apply(allMarkets, $scope.blackoutMarkets());
        return allMarkets;
    });

    // keeps original values (needed if edited and then cancelled)
    $scope.open = function () {
        $scope.originalCustomMarketGroup = _.cloneDeep($scope.simpleMarketSelector().customMarketGroup());
        $scope.originalSelectedCustomMarkets = _.cloneDeep($scope.simpleMarketSelector().customMarkets());

        $scope.originalBlackoutMarketGroup = _.cloneDeep($scope.blackoutMarketSelector().customMarketGroup());
        $scope.originalSelectedBlackoutMarkets = _.cloneDeep($scope.blackoutMarketSelector().customMarkets());
    };

    $scope.cancel = function () {
        $scope.simpleMarketSelector().customMarketGroup($scope.originalCustomMarketGroup);
        $scope.simpleMarketSelector().customMarkets($scope.originalSelectedCustomMarkets);

        $scope.blackoutMarketSelector().customMarketGroup($scope.originalBlackoutMarketGroup);
        $scope.blackoutMarketSelector().customMarkets($scope.originalSelectedBlackoutMarkets);

        $scope.show(false);
    };

    $scope.save = function () {
        if ($scope.totalCounter() > 0) {
            $scope.controller.proposalViewModel.createCustomMarketOption();
        } else {
            $scope.controller.proposalViewModel.deleteCustomMarketOption();
        }

        $scope.show(false);
    };

    $scope.clear = function () {
        $scope.originalSelectedCustomMarkets = [];
        $scope.originalSelectedBlackoutMarkets = [];
        $scope.simpleMarketSelector().clear();
        $scope.blackoutMarketSelector().clear();
    };
}