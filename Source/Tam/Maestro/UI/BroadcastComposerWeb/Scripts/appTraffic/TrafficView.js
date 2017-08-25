var TrafficView = BaseView.extend({
    trafficController: null,
    trafficGrid: null,

    initView: function (controller) {
        var $scope = this;

        $scope.trafficController = controller;
        $scope.trafficGrid = $("#traffic_grid").w2grid(TrafficConfig.getTrafficGridConfig(this));

        $scope.loadGrid();
    },

    loadGrid: function () {
        var $scope = this;

        $scope.trafficController.apiGetTraffic(function (trafficData) {
            var gridWeeks = trafficData.Weeks.map(function (week) {
                week.recid = week.WeekId;
                week.editable = false;
                week.w2ui = {
                    style: "background-color: #dedede",
                    children: week.TrafficProposalInventories.map(function(trafficProposalInventory) {
                        trafficProposalInventory.recid = "" + week.WeekId + trafficProposalInventory.Id;
                        trafficProposalInventory.Flight = moment(trafficProposalInventory.StartDate).format('MM/DD/YY') + "-" + moment(trafficProposalInventory.EndDate).format('MM/DD/YY');
                        return trafficProposalInventory;
                    })
                }
                week.Id = 'Week ' + week.Week;
                return week;
            });

            $scope.trafficGrid.records = gridWeeks;
            $scope.trafficGrid.refresh();
        });
    }
});