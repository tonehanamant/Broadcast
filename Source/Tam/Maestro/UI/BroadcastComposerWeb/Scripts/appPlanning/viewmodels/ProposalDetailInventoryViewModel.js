//Proposal Detail Inventory View Model - tied to the Inventory View for an active detail set  within proposal
function ProposalDetailInventoryViewModel(view) {
    var $scope = this;
    $scope.view = view;

    // based on proposal status
    $scope.readOnly = ko.observable(false);

    // header 1
    $scope.inventoryPlanId = ko.observable();
    $scope.proposalName = ko.observable();
    $scope.proposalStartDate = ko.observable();
    $scope.proposalEndDate = ko.observable();

    // header 2
    $scope.detailSpotLength = ko.observable();
    $scope.daypartText = ko.observable();
    $scope.detailFlightStartDate = ko.observable();
    $scope.detailFlightEndDate = ko.observable();

    // header 3
    // impressions
    $scope.totalImpressions = ko.observable(0);
    $scope.detailTargetImpressions = ko.observable(0);
    $scope.impressionsPercent = ko.observable(0);
    $scope.detailImpressionsMarginAchieved = ko.observable();

    // budget
    $scope.totalCost = ko.observable(0);
    $scope.detailTargetBudget = ko.observable();
    $scope.budgetPercent = ko.observable(0);
    $scope.detailBudgetMarginAchieved = ko.observable();

    // cpm
    $scope.totalCpm = ko.observable(0);
    $scope.detailCpm = ko.observable();
    $scope.cpmPercent = ko.observable(0);
    $scope.detailCpmMaginAchieved = ko.observable();;

    $scope.setInventory = function (inventory, readOnly) {
        $scope.readOnly(readOnly || false);

        // header 1
        $scope.inventoryPlanId('Plan ' + inventory.ProposalId);
        $scope.proposalName(inventory.ProposalName);
        $scope.proposalStartDate(moment(new Date(inventory.ProposalFlightStartDate)).format('MM/DD/YY'));
        $scope.proposalEndDate(moment(new Date(inventory.ProposalFlightEndDate)).format('MM/DD/YY'));

        // header 2
        $scope.detailSpotLength(inventory.DetailSpotLength);
        $scope.daypartText(inventory.DetailDaypart.Text);
        $scope.detailFlightStartDate(moment(new Date(inventory.DetailFlightStartDate)).format('MM/DD/YY'));
        $scope.detailFlightEndDate(moment(new Date(inventory.DetailFlightEndDate)).format('MM/DD/YY'));

        // header 3

        // impressions
        $scope.totalImpressions(inventory.DetailTotalImpressions);
        $scope.detailTargetImpressions(inventory.DetailTargetImpressions);
        $scope.impressionsPercent(inventory.DetailImpressionsPercent);
        $scope.detailImpressionsMarginAchieved(inventory.DetailImpressionsMarginAchieved);

        // budget
        $scope.totalCost(inventory.DetailTotalBudget);
        $scope.detailTargetBudget(inventory.DetailTargetBudget);
        $scope.budgetPercent(inventory.DetailBudgetPercent);
        $scope.detailBudgetMarginAchieved(inventory.DetailBudgetMarginAchieved);

        // cpm
        $scope.totalCpm(inventory.DetailTotalCpm);
        $scope.detailCpm(inventory.DetailCpm);
        $scope.cpmPercent(inventory.DetailCpmPercent);
        $scope.detailCpmMaginAchieved(inventory.DetailCpmMarginAchieved);
    };

    $scope.saveInventory = function () {
        $scope.view.saveInventory(false);
    };

    $scope.applyInventory = function () {
        $scope.view.saveInventory(true);
    };


};