var data = {
    ProposalId: 66862,
    Advertiser: "Turkish Airlines",
    Agency: "Accuen",
    Product: "Turkish Delight",
    SalesPerson: "Rick Beispel",
    StartDate: "2016-03-28T00:00:00",
    EndDate: "2016-04-24T00:00:00",
    FlightText: "03/28 - 04/24/2016 (4 Weeks)",
    GuaranteedDemographicCode: "A18-24",
    GuaranteedAudienceId: 245,
    TotalCost: 110720.0000,
    Budget: 0.0000,
    Type: "Brand",
    EqHhCpm: 5.5122,
    EqDemoCpm: 39.2115
}

var ProposalViewModel = function (controller) {
    //private controller
    var controller = controller;
    //public
    var $scope = this;
    $scope.Header = ko.observable();

    $scope.init = function (data) {

    };

    $scope.saveProposal = function (e) {
        var records = controller.view.$InventoryGrid.records;

        var details = records.filter(function (x) {
            return x.recid !== "N-1"
        })

        controller.apiSaveProposal(details);
    }
};