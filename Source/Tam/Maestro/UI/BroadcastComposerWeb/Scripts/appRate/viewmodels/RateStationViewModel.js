//Station Detail  View Model

var RateStationViewModel = function (controller) {
    //private controller
    var $scope = this;
    var controller = controller; //leave private

    /*** STATION RELATED ***/
    $scope.StationName = ko.observable();
    $scope.Affiliate = ko.observable();
    $scope.Market = ko.observable();

    $scope.isThirdParty = ko.observable(false); //determine if thirdParty

    $scope.setActiveStation = function (data, isThirdParty) {
        $scope.StationName(data.StationName);
        $scope.Affiliate(data.Affiliate);
        $scope.Market(data.Market);

        $scope.isThirdParty(isThirdParty);
    };

    //Deprecate this usage for now - use KO mark-up directly for each in context of the modal-title classes etc 

    //$scope.StationTitle = ko.computed(function () {
    //    var stationName = '<span>' + $scope.StationName() + '</span>',
    //        stationAffiliate = '<span>' + $scope.Affiliate() + '</span>',
    //        stationMarket = '<span>' + $scope.Market() + '</span>',

    //        html = '<div class="row" style="margin-left: 10px">' + stationName + ' - ' + stationAffiliate + ' - ' + stationMarket + '</div>';

    //    return html;
    //});


};